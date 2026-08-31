using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Interfaces;

/// <summary>
/// Unit of Work: espone i repository e centralizza il salvataggio delle modifiche in un'unica
/// transazione EF Core (SaveChangesAsync popola anche i campi di audit su BaseEntity).
/// </summary>
public interface IUnitOfWork
{
    IRepository<Utente> Utenti { get; }
    IRepository<Agente> Agenti { get; }
    IRepository<Cliente> Clienti { get; }
    IRepository<Ordine> Ordini { get; }
    IRepository<Importazione> Importazioni { get; }
    IRepository<Attivita> Attivita { get; }
    IRepository<NotaCliente> NoteCliente { get; }
    IRepository<Calendario> Calendario { get; }
    IRepository<ObiettivoVendita> ObiettiviVendita { get; }
    IRepository<StoricoKPI> StoricoKPI { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Notifica> Notifiche { get; }
    IRepository<Comunicazione> Comunicazioni { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
