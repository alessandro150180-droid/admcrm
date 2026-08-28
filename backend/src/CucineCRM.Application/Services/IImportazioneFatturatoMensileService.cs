using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IImportazioneFatturatoMensileService
{
    /// <summary>
    /// Importa fatturato mensile da un file Excel "a pivot": una riga per cliente, con una colonna
    /// per ogni mese (es. "Aprile 2026", "Maggio 2026", ...) contenente il fatturato di quel mese.
    /// Colonne fisse attese: CodiceCliente (obbligatoria), Provvigione (opzionale, aggiorna quella
    /// del cliente se presente). Ogni cella mese/cliente con importo diverso da zero genera un
    /// Ordine sintetico (RiferimentoEsterno = "FATT-{codiceCliente}-{anno}{mese}", per riconoscere
    /// i duplicati se lo stesso file viene re-importato).
    /// </summary>
    Task<ImportazioneRisultatoDto> ImportaFatturatoMensileAsync(Stream file, string nomeFile, CancellationToken ct = default);
}
