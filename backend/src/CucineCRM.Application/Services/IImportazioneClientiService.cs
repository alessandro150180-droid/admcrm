using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IImportazioneClientiService
{
    /// <summary>
    /// Importa anagrafiche clienti da un file Excel (.xlsx). Colonne attese (intestazione in prima
    /// riga, nomi non case-sensitive): RagioneSociale, CodiceCliente, PartitaIVA, Indirizzo, Citta,
    /// Provincia, Regione, CAP, Telefono, Email, EmailAgente (deve corrispondere a un agente esistente).
    /// I clienti con CodiceCliente già presente in anagrafica vengono considerati duplicati e scartati:
    /// l'import crea solo clienti nuovi, non aggiorna quelli esistenti.
    /// </summary>
    Task<ImportazioneRisultatoDto> ImportaClientiAsync(
        Stream file, string nomeFile, string periodoCompetenza, CancellationToken ct = default);
}
