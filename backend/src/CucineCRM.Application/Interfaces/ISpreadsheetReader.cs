namespace CucineCRM.Application.Interfaces;

/// <summary>
/// Legge un file Excel (.xlsx) come righe di testo, senza esporre all'Application layer la libreria
/// di parsing concreta (implementata in Infrastructure, vedi ClosedXmlSpreadsheetReader).
/// La prima riga del foglio è trattata come intestazione: le chiavi dei dizionari restituiti sono
/// gli header di colonna (case-insensitive), i valori il contenuto testuale della cella.
/// </summary>
public interface ISpreadsheetReader
{
    IReadOnlyList<IReadOnlyDictionary<string, string>> LeggiRighe(Stream file);
}
