using System.Globalization;
using System.Text.Json;
using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class ImportazioneClientiService : IImportazioneClientiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ISpreadsheetReader _spreadsheetReader;
    private readonly ICurrentUserService _currentUser;

    public ImportazioneClientiService(
        IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor, ISpreadsheetReader spreadsheetReader, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
        _spreadsheetReader = spreadsheetReader;
        _currentUser = currentUser;
    }

    public async Task<ImportazioneRisultatoDto> ImportaClientiAsync(
        Stream file, string nomeFile, string periodoCompetenza, CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var righe = _spreadsheetReader.LeggiRighe(file);

        // Prefetch: matching agente sia per email che per cognome (i file reali dei clienti spesso
        // riportano solo il cognome dell'agente, non l'email), e un set dei codici cliente già in DB.
        var agenti = await _queryExecutor.ToListAsync(_unitOfWork.Agenti.Query()
            .Select(a => new { a.Id, a.Cognome, a.Email }), ct);

        var agentiPerEmail = agenti
            .GroupBy(a => a.Email, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var agentiPerCognome = agenti
            .GroupBy(a => a.Cognome, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(a => a.Id).ToList(), StringComparer.OrdinalIgnoreCase);

        var clientiEsistenti = (await _queryExecutor.ToListAsync(_unitOfWork.Clienti.Query()
            .Select(c => new { c.Id, c.CodiceCliente }), ct))
            .GroupBy(c => c.CodiceCliente, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var log = new List<RigaImportLogDto>();
        var nuoviClienti = new List<Cliente>();
        var codiciVistiInQuestoFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var scartate = 0;
        var aggiornate = 0;

        for (var i = 0; i < righe.Count; i++)
        {
            var numeroRiga = i + 2; // +1 per header, +1 perché l'utente conta le righe da 1
            var riga = NormalizzaChiavi(righe[i]);

            var ragioneSociale = Cella(riga, "RagioneSociale", "Ragione Sociale");
            if (string.IsNullOrWhiteSpace(ragioneSociale))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "Ragione sociale mancante."));
                scartate++;
                continue;
            }

            var codiceCliente = Cella(riga, "CodiceCliente", "Codice Cliente");
            if (string.IsNullOrWhiteSpace(codiceCliente))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "Codice cliente mancante."));
                scartate++;
                continue;
            }

            var valoreAgente = Cella(riga, "EmailAgente", "Email Agente", "Agente");
            if (string.IsNullOrWhiteSpace(valoreAgente))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "Agente mancante."));
                scartate++;
                continue;
            }

            int agenteId;
            if (valoreAgente.Contains('@'))
            {
                if (!agentiPerEmail.TryGetValue(valoreAgente, out agenteId))
                {
                    log.Add(new RigaImportLogDto(numeroRiga, "Scartata", $"Nessun agente con email '{valoreAgente}'."));
                    scartate++;
                    continue;
                }
            }
            else if (agentiPerCognome.TryGetValue(valoreAgente, out var idCandidati))
            {
                if (idCandidati.Count > 1)
                {
                    log.Add(new RigaImportLogDto(numeroRiga, "Scartata",
                        $"Più agenti hanno cognome '{valoreAgente}': indicare l'email in colonna EmailAgente per questa riga."));
                    scartate++;
                    continue;
                }
                agenteId = idCandidati[0];
            }
            else
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", $"Nessun agente trovato per '{valoreAgente}' (né come email né come cognome)."));
                scartate++;
                continue;
            }

            if (!codiciVistiInQuestoFile.Add(codiceCliente))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", $"Codice cliente '{codiceCliente}' ripetuto più volte in questo file."));
                scartate++;
                continue;
            }

            var partitaIva = Cella(riga, "PartitaIVA", "Partita IVA", "PIVA");
            var indirizzo = Cella(riga, "Indirizzo");
            var citta = Cella(riga, "Citta", "Città");
            var provincia = Cella(riga, "Provincia");
            var regione = Cella(riga, "Regione");
            var cap = Cella(riga, "CAP");
            var telefono = Cella(riga, "Telefono");
            var emailCliente = Cella(riga, "EmailCliente", "E Mail Cliente", "E-mail Cliente", "Email");
            var provvigioneTesto = Cella(riga, "PercentualeProvvigione", "Provvigione", "% Provvigione");

            // Se il codice cliente esiste già, aggiorna la scheda invece di scartarla come duplicato:
            // i file reali vengono spesso re-inviati con più campi valorizzati rispetto alla prima
            // importazione (es. indirizzo e contatti aggiunti in un secondo momento). I campi assenti
            // in questa riga non cancellano quelli già presenti in anagrafica.
            if (clientiEsistenti.TryGetValue(codiceCliente, out var idEsistente))
            {
                var esistente = await _unitOfWork.Clienti.GetByIdAsync(idEsistente, ct);
                if (esistente is null)
                {
                    log.Add(new RigaImportLogDto(numeroRiga, "Scartata", $"Cliente '{codiceCliente}' non trovato durante l'aggiornamento."));
                    scartate++;
                    continue;
                }

                esistente.RagioneSociale = ragioneSociale;
                esistente.AgenteId = agenteId;
                if (partitaIva is not null) esistente.PartitaIVA = partitaIva;
                if (indirizzo is not null) esistente.Indirizzo = indirizzo;
                if (citta is not null) esistente.Citta = citta;
                if (provincia is not null) esistente.Provincia = provincia;
                if (regione is not null) esistente.Regione = regione;
                if (cap is not null) esistente.CAP = cap;
                if (telefono is not null) esistente.Telefono = telefono;
                if (emailCliente is not null) esistente.Email = emailCliente;
                if (provvigioneTesto is not null) esistente.PercentualeProvvigione = ParsaPercentualeProvvigione(provvigioneTesto);

                _unitOfWork.Clienti.Update(esistente);
                log.Add(new RigaImportLogDto(numeroRiga, "Aggiornata", $"Codice cliente '{codiceCliente}' già presente: dati aggiornati."));
                aggiornate++;
                continue;
            }

            nuoviClienti.Add(new Cliente
            {
                RagioneSociale = ragioneSociale,
                CodiceCliente = codiceCliente,
                PartitaIVA = partitaIva,
                Indirizzo = indirizzo,
                Citta = citta,
                Provincia = provincia,
                Regione = regione,
                CAP = cap,
                Telefono = telefono,
                Email = emailCliente,
                AgenteId = agenteId,
                DataInserimento = DateTime.UtcNow,
                PercentualeProvvigione = ParsaPercentualeProvvigione(provvigioneTesto)
            });

            log.Add(new RigaImportLogDto(numeroRiga, "Importata", null));
        }

        var importazione = new Importazione
        {
            NomeFile = nomeFile,
            DataImportazione = DateTime.UtcNow,
            UtenteImportazioneId = utenteId,
            PeriodoCompetenza = periodoCompetenza,
            RighePlesse = righe.Count,
            RigheImportate = nuoviClienti.Count,
            RigheScartate = scartate,
            RigheDuplicate = aggiornate,
            LogEsito = JsonSerializer.Serialize(log),
            Completata = true
        };

        await _unitOfWork.Importazioni.AddAsync(importazione, ct);

        foreach (var cliente in nuoviClienti)
            await _unitOfWork.Clienti.AddAsync(cliente, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new ImportazioneRisultatoDto(
            importazione.Id, importazione.NomeFile, importazione.DataImportazione, importazione.PeriodoCompetenza,
            importazione.RighePlesse, importazione.RigheImportate, importazione.RigheScartate, importazione.RigheDuplicate,
            importazione.Completata, importazione.LogEsito);
    }

    /// <summary>
    /// Alcuni file esprimono la provvigione come frazione (0,07 = 7%) invece che come percentuale
    /// diretta (7 = 7%): un valore compreso tra 0 (escluso) e 1 viene quindi interpretato come
    /// frazione e riportato a scala 0-100, altrimenti è già una percentuale.
    /// </summary>
    private static decimal ParsaPercentualeProvvigione(string? testo)
    {
        if (string.IsNullOrWhiteSpace(testo))
            return 0m;

        if (!decimal.TryParse(testo, NumberStyles.Number, CultureInfo.InvariantCulture, out var valore) &&
            !decimal.TryParse(testo, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"), out valore))
        {
            return 0m;
        }

        return valore > 0 && valore <= 1 ? valore * 100 : valore;
    }

    /// <summary>Normalizza le intestazioni di una riga (case/spazi/apostrofi non contano) per un
    /// matching tollerante a variazioni comuni ("CODICE CLIENTE" vs "CodiceCliente" vs "Codice Cliente").</summary>
    private static Dictionary<string, string> NormalizzaChiavi(IReadOnlyDictionary<string, string> riga)
    {
        var normalizzata = new Dictionary<string, string>();
        foreach (var (chiave, valore) in riga)
        {
            var chiaveNormalizzata = Normalizza(chiave);
            if (!normalizzata.ContainsKey(chiaveNormalizzata))
                normalizzata[chiaveNormalizzata] = valore;
        }
        return normalizzata;
    }

    private static string Normalizza(string testo) =>
        new string(testo.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    private static string? Cella(IReadOnlyDictionary<string, string> rigaNormalizzata, params string[] alias)
    {
        foreach (var nome in alias)
        {
            if (rigaNormalizzata.TryGetValue(Normalizza(nome), out var valore) && !string.IsNullOrWhiteSpace(valore))
                return valore.Trim();
        }
        return null;
    }
}
