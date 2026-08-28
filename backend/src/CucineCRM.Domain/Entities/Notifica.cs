using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class Notifica : BaseEntity
{
    public int UtenteId { get; set; }
    public Utente Utente { get; set; } = null!;

    public string Tipo { get; set; } = string.Empty; // es. "AttivitaScaduta", "ObiettivoNonRaggiunto"
    public string Titolo { get; set; } = string.Empty;
    public string? Messaggio { get; set; }

    // Riferimento libero (senza FK) all'entità che ha generato la notifica, per il click-through lato frontend.
    public int? RiferimentoEntitaId { get; set; }

    public bool Letta { get; set; }
    public DateTime? DataLettura { get; set; }
}
