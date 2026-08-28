using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class Importazione : BaseEntity
{
    public string NomeFile { get; set; } = string.Empty;
    public DateTime DataImportazione { get; set; }

    public int UtenteImportazioneId { get; set; }
    public Utente UtenteImportazione { get; set; } = null!;

    public string PeriodoCompetenza { get; set; } = string.Empty; // es. "2026-06"

    public int RighePlesse { get; set; }
    public int RigheImportate { get; set; }
    public int RigheScartate { get; set; }
    public int RigheDuplicate { get; set; }

    public string? LogEsito { get; set; } // JSON con dettaglio errori/warning riga per riga
    public bool Completata { get; set; }

    public ICollection<Ordine> Ordini { get; set; } = new List<Ordine>();
}
