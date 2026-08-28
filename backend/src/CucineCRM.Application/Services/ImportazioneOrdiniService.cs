using System.Globalization;
using System.Text.Json;
using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.Services;

public class ImportazioneOrdiniService : IImportazioneOrdiniService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ISpreadsheetReader _spreadsheetReader;
    private readonly ICurrentUserService _currentUser;

    public ImportazioneOrdiniService(
        IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor, ISpreadsheetReader spreadsheetReader, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
        _spreadsheetReader = spreadsheetReader;
        _currentUser = currentUser;
    }

    public async Task<ImportazioneRisultatoDto> ImportaOrdiniAsync(
        Stream file, string nomeFile, string periodoCompetenza, CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var righe = _spreadsheetReader.LeggiRighe(file);

        // Prefetch: una query sola per i clienti e una per i riferimenti già in DB, invece di
        // interrogare il database una volta per ogni riga del file (potenzialmente migliaia).
        var clientiPerCodice = (await _queryExecutor.ToListAsync(_unitOfWork.Clienti.Query()
            .Select(c => new { c.Id, c.CodiceCliente }), ct))
            .GroupBy(c => c.CodiceCliente, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var riferimentiEsistenti = new HashSet<string>(
            await _queryExecutor.ToListAsync(_unitOfWork.Ordini.Query()
                .Where(o => o.RiferimentoEsterno != null)
                .Select(o => o.RiferimentoEsterno!), ct),
            StringComparer.OrdinalIgnoreCase);

        var log = new List<RigaImportLogDto>();
        var nuoviOrdini = new List<Ordine>();
        var scartate = 0;
        var duplicate = 0;

        for (var i = 0; i < righe.Count; i++)
        {
            var numeroRiga = i + 2; // +1 per header, +1 perché l'utente conta le righe da 1
            var riga = righe[i];

            var codiceCliente = ValoreCella(riga, "CodiceCliente");
            if (string.IsNullOrWhiteSpace(codiceCliente))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "CodiceCliente mancante."));
                scartate++;
                continue;
            }

            if (!clientiPerCodice.TryGetValue(codiceCliente, out var clienteId))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", $"Nessun cliente con codice '{codiceCliente}'."));
                scartate++;
                continue;
            }

            if (!TryParseData(ValoreCella(riga, "DataOrdine"), out var dataOrdine))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "DataOrdine mancante o non valida."));
                scartate++;
                continue;
            }

            if (!TryParseImporto(ValoreCella(riga, "Importo"), out var importo))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Scartata", "Importo mancante o non valido."));
                scartate++;
                continue;
            }

            var riferimentoEsterno = ValoreCella(riga, "RiferimentoEsterno");
            riferimentoEsterno = string.IsNullOrWhiteSpace(riferimentoEsterno) ? null : riferimentoEsterno.Trim();

            if (riferimentoEsterno is not null && riferimentiEsistenti.Contains(riferimentoEsterno))
            {
                log.Add(new RigaImportLogDto(numeroRiga, "Duplicata", $"Riferimento esterno '{riferimentoEsterno}' già presente."));
                duplicate++;
                continue;
            }

            nuoviOrdini.Add(new Ordine
            {
                ClienteId = clienteId,
                DataOrdine = dataOrdine,
                Importo = importo,
                NumeroCucine = ParseIntODefault(ValoreCella(riga, "NumeroCucine")),
                NumeroElettrodomestici = ParseIntODefault(ValoreCella(riga, "NumeroElettrodomestici")),
                NumeroComplementi = ParseIntODefault(ValoreCella(riga, "NumeroComplementi")),
                RiferimentoEsterno = riferimentoEsterno,
                StatoOrdine = StatoOrdine.InAttesa
            });

            if (riferimentoEsterno is not null)
                riferimentiEsistenti.Add(riferimentoEsterno); // evita duplicati anche tra righe dello stesso file

            log.Add(new RigaImportLogDto(numeroRiga, "Importata", null));
        }

        var importazione = new Importazione
        {
            NomeFile = nomeFile,
            DataImportazione = DateTime.UtcNow,
            UtenteImportazioneId = utenteId,
            PeriodoCompetenza = periodoCompetenza,
            RighePlesse = righe.Count,
            RigheImportate = nuoviOrdini.Count,
            RigheScartate = scartate,
            RigheDuplicate = duplicate,
            LogEsito = JsonSerializer.Serialize(log),
            Completata = true
        };

        await _unitOfWork.Importazioni.AddAsync(importazione, ct);
        await _unitOfWork.SaveChangesAsync(ct); // necessario per ottenere l'Id generato di Importazione, da assegnare agli Ordini sotto

        foreach (var ordine in nuoviOrdini)
        {
            ordine.ImportazioneId = importazione.Id;
            await _unitOfWork.Ordini.AddAsync(ordine, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);

        return new ImportazioneRisultatoDto(
            importazione.Id, importazione.NomeFile, importazione.DataImportazione, importazione.PeriodoCompetenza,
            importazione.RighePlesse, importazione.RigheImportate, importazione.RigheScartate, importazione.RigheDuplicate,
            importazione.Completata, importazione.LogEsito);
    }

    private static string? ValoreCella(IReadOnlyDictionary<string, string> riga, string colonna) =>
        riga.TryGetValue(colonna, out var valore) ? valore : null;

    private static bool TryParseData(string? testo, out DateTime data)
    {
        data = default;
        if (string.IsNullOrWhiteSpace(testo))
            return false;

        // Postgres "timestamp with time zone" richiede DateTime con Kind=Utc: il testo del file
        // non porta alcuna informazione di fuso, quindi lo trattiamo come UTC "as-is" (AssumeUniversal).
        const DateTimeStyles stili = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
        return DateTime.TryParse(testo, CultureInfo.InvariantCulture, stili, out data)
            || DateTime.TryParse(testo, CultureInfo.GetCultureInfo("it-IT"), stili, out data);
    }

    private static bool TryParseImporto(string? testo, out decimal importo)
    {
        importo = default;
        return !string.IsNullOrWhiteSpace(testo) &&
            (decimal.TryParse(testo, NumberStyles.Number, CultureInfo.InvariantCulture, out importo)
                || decimal.TryParse(testo, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"), out importo));
    }

    private static int ParseIntODefault(string? testo) =>
        int.TryParse(testo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valore) ? valore : 0;
}
