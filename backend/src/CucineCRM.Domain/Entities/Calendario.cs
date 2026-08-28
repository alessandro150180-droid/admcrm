using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class Calendario : BaseEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int AttivitaId { get; set; }
    public Attivita Attivita { get; set; } = null!;

    public string? GoogleEventId { get; set; }
    public DateTime DataEvento { get; set; }

    // stato della sincronizzazione bidirezionale con Google Calendar
    public DateTime? UltimaSincronizzazione { get; set; }
    public bool SincronizzatoConGoogle { get; set; }
}
