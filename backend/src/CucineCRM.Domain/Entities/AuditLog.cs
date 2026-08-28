using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

/// <summary>
/// Traccia automaticamente ogni Creazione/Modifica/Eliminazione delle entità applicative
/// (popolata da ApplicationDbContext.SaveChangesAsync, non da codice applicativo esplicito).
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UtenteId { get; set; }
    public Utente? Utente { get; set; }

    public string NomeEntita { get; set; } = string.Empty;
    public int EntitaId { get; set; }
    public string Azione { get; set; } = string.Empty; // Creazione, Modifica, Eliminazione
}
