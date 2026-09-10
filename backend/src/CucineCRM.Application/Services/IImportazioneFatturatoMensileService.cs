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

    /// <summary>
    /// Elimina (soft-delete, lo storico resta in DB con Eliminato = true) gli ordini sintetici generati
    /// dall'import fatturato mensile per un dato mese/anno, per permettere di correggere un import
    /// errato ricaricando il file: dopo l'eliminazione, un nuovo import per quel periodo non troverà
    /// più i vecchi RiferimentoEsterno come duplicati e potrà ricrearli con i valori corretti.
    /// Restituisce il numero di ordini eliminati.
    /// </summary>
    Task<int> EliminaFatturatoMensileAsync(int anno, int mese, CancellationToken ct = default);
}
