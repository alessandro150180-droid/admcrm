using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class Cliente : BaseEntity
{
    public string RagioneSociale { get; set; } = string.Empty;
    public string CodiceCliente { get; set; } = string.Empty;
    public string? PartitaIVA { get; set; }
    public string? Indirizzo { get; set; }
    public string? Citta { get; set; }
    public string? Provincia { get; set; } // sigla ("BA") o nome esteso ("Bari"): i file reali non sono sempre uniformi
    public string? Regione { get; set; }
    public string? CAP { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // Persona fisica di riferimento presso il cliente (il titolare del punto vendita): è il
    // nominativo che gli agenti usano per il contatto diretto, distinto dalla RagioneSociale.
    public string? NominativoTitolare { get; set; }

    public int AgenteId { get; set; }
    public Agente Agente { get; set; } = null!;

    public DateTime DataInserimento { get; set; }

    // Percentuale di provvigione riconosciuta all'agente sul fatturato di QUESTO cliente
    // specifico (non un valore fisso per agente): clienti diversi dello stesso agente possono
    // avere percentuali diverse, es. per accordi commerciali negoziati caso per caso.
    public decimal PercentualeProvvigione { get; set; }

    // Navigazione
    public ICollection<Ordine> Ordini { get; set; } = new List<Ordine>();
    public ICollection<Attivita> Attivita { get; set; } = new List<Attivita>();
    public ICollection<NotaCliente> Note { get; set; } = new List<NotaCliente>();
    public ICollection<Calendario> EventiCalendario { get; set; } = new List<Calendario>();
}
