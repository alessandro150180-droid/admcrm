namespace CucineCRM.Domain.Common;

/// <summary>
/// Classe base per tutte le entità: fornisce chiave primaria e campi di audit.
/// L'audit (creazione/modifica) è popolato automaticamente da ApplicationDbContext.SaveChangesAsync.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime DataCreazione { get; set; }
    public DateTime? DataModifica { get; set; }
    public bool Eliminato { get; set; } = false; // soft delete: le importazioni non devono mai perdere lo storico
}
