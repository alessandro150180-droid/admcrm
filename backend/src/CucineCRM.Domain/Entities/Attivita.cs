using CucineCRM.Domain.Common;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Domain.Entities;

public class Attivita : BaseEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int UtenteId { get; set; } // responsabile
    public Utente Utente { get; set; } = null!;

    public TipoAttivita TipoAttivita { get; set; }
    public string Titolo { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public PrioritaAttivita Priorita { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool Completata { get; set; }
    public StatoAttivita Stato { get; set; } = StatoAttivita.DaFare;

    // Promemoria: minuti prima della scadenza (es. 1440 = 1 giorno, 60 = 1 ora, 15 = 15 minuti)
    public string? PromemoriaMinutiPrima { get; set; } // CSV, es "1440,60,15"

    public Calendario? EventoCalendario { get; set; }
}
