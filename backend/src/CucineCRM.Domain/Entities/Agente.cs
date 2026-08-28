using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

/// <summary>
/// Agente della rete vendita. AreaManagerId, se valorizzato, indica l'Area Manager
/// a cui l'agente è assegnato: questo è ciò che determina la visibilità dei dati
/// per gli utenti con ruolo AreaManager (vedono solo gli agenti con AreaManagerId = loro Id).
/// </summary>
public class Agente : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;

    // Un Agente può essere gestito da un Area Manager (che a sua volta è un Agente con ruolo speciale
    // nella tabella Utenti). AreaManagerId referenzia l'Id dell'Agente-manager.
    public int? AreaManagerId { get; set; }
    public Agente? AreaManager { get; set; }

    // Navigazione
    public ICollection<Cliente> Clienti { get; set; } = new List<Cliente>();
    public ICollection<ObiettivoVendita> Obiettivi { get; set; } = new List<ObiettivoVendita>();
    public ICollection<Agente> AgentiGestiti { get; set; } = new List<Agente>();
}
