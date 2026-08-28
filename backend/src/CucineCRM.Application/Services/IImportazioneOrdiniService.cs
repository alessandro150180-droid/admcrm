using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IImportazioneOrdiniService
{
    /// <summary>
    /// Importa ordini da un file Excel (.xlsx). Colonne attese (intestazione in prima riga,
    /// nomi non case-sensitive): CodiceCliente, DataOrdine, Importo, NumeroCucine,
    /// NumeroElettrodomestici, NumeroComplementi, RiferimentoEsterno (opzionale).
    /// I duplicati sono riconosciuti tramite RiferimentoEsterno (sia contro gli ordini già in DB,
    /// sia tra righe dello stesso file) e vengono scartati senza creare un nuovo Ordine.
    /// </summary>
    Task<ImportazioneRisultatoDto> ImportaOrdiniAsync(
        Stream file, string nomeFile, string periodoCompetenza, CancellationToken ct = default);
}
