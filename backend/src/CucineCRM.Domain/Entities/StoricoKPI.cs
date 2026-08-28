using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

/// <summary>
/// Tabella di aggregazione mensile, popolata da un job (o ricalcolata on-demand)
/// per rendere veloci i grafici storici a 5 anni senza dover aggregare gli Ordini ogni volta.
/// </summary>
public class StoricoKPI : BaseEntity
{
    public int Mese { get; set; }
    public int Anno { get; set; }
    public decimal Fatturato { get; set; }
    public int CucineVendute { get; set; }
    public decimal OrdineMedio { get; set; }
    public int NuoviClienti { get; set; }

    // opzionale: se valorizzato, il KPI è calcolato per singolo agente invece che aggregato azienda
    public int? AgenteId { get; set; }
    public Agente? Agente { get; set; }
}
