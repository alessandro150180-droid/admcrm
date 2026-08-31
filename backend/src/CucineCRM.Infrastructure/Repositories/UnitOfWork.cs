using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;
using CucineCRM.Infrastructure.Persistence;

namespace CucineCRM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Utenti = new Repository<Utente>(_context);
        Agenti = new Repository<Agente>(_context);
        Clienti = new Repository<Cliente>(_context);
        Ordini = new Repository<Ordine>(_context);
        Importazioni = new Repository<Importazione>(_context);
        Attivita = new Repository<Attivita>(_context);
        NoteCliente = new Repository<NotaCliente>(_context);
        Calendario = new Repository<Calendario>(_context);
        ObiettiviVendita = new Repository<ObiettivoVendita>(_context);
        StoricoKPI = new Repository<StoricoKPI>(_context);
        AuditLogs = new Repository<AuditLog>(_context);
        Notifiche = new Repository<Notifica>(_context);
        Comunicazioni = new Repository<Comunicazione>(_context);
    }

    public IRepository<Utente> Utenti { get; }
    public IRepository<Agente> Agenti { get; }
    public IRepository<Cliente> Clienti { get; }
    public IRepository<Ordine> Ordini { get; }
    public IRepository<Importazione> Importazioni { get; }
    public IRepository<Attivita> Attivita { get; }
    public IRepository<NotaCliente> NoteCliente { get; }
    public IRepository<Calendario> Calendario { get; }
    public IRepository<ObiettivoVendita> ObiettiviVendita { get; }
    public IRepository<StoricoKPI> StoricoKPI { get; }
    public IRepository<AuditLog> AuditLogs { get; }
    public IRepository<Notifica> Notifiche { get; }
    public IRepository<Comunicazione> Comunicazioni { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
