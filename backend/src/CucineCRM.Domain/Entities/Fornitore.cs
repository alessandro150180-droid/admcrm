using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

/// <summary>
/// Fornitore/brand di cui ADM rappresenta la vendita (es. Nobilia, NobiSmart, NobiDirect, Comma,
/// GierreDue). Ogni Ordine appartiene a un solo fornitore, per poter distinguere il fatturato per
/// linea di prodotto in dashboard e report. Elenco estendibile: la direzione può aggiungerne altri.
/// </summary>
public class Fornitore : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public bool Attivo { get; set; } = true;

    public ICollection<Ordine> Ordini { get; set; } = new List<Ordine>();
}
