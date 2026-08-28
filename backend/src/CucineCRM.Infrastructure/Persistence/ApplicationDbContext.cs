using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Common;
using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CucineCRM.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<Agente> Agenti => Set<Agente>();
    public DbSet<Cliente> Clienti => Set<Cliente>();
    public DbSet<Ordine> Ordini => Set<Ordine>();
    public DbSet<Importazione> Importazioni => Set<Importazione>();
    public DbSet<Attivita> Attivita => Set<Attivita>();
    public DbSet<NotaCliente> NoteCliente => Set<NotaCliente>();
    public DbSet<Calendario> Calendario => Set<Calendario>();
    public DbSet<ObiettivoVendita> ObiettiviVendita => Set<ObiettivoVendita>();
    public DbSet<StoricoKPI> StoricoKPI => Set<StoricoKPI>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notifica> Notifiche => Set<Notifica>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applica tutte le classi IEntityTypeConfiguration<T> presenti in questo assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Query filter globale: le righe soft-deleted non compaiono mai nelle query di default.
        // Questo garantisce che l'import Excel, cancellando "logicamente" un ordine, non ne perda mai lo storico.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.Eliminato);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var vociDaTracciare = new List<(EntityState Stato, BaseEntity Entita)>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DataCreazione = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.DataModifica = now;
                    break;
            }

            // L'AuditLog non traccia se stesso, altrimenti ogni scrittura ne genererebbe un'altra all'infinito.
            if (entry.Entity is not AuditLog && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                vociDaTracciare.Add((entry.State, entry.Entity));
        }

        var risultato = await base.SaveChangesAsync(cancellationToken);

        // Le entità Added ottengono il loro Id (identity) solo dopo il salvataggio: l'AuditLog va
        // quindi scritto con una seconda SaveChangesAsync, a valle, quando gli Id sono già valorizzati.
        if (vociDaTracciare.Count > 0)
        {
            var utenteId = _currentUser.UtenteId;
            foreach (var (stato, entita) in vociDaTracciare)
            {
                AuditLogs.Add(new AuditLog
                {
                    UtenteId = utenteId,
                    NomeEntita = entita.GetType().Name,
                    EntitaId = entita.Id,
                    Azione = stato switch
                    {
                        EntityState.Added => "Creazione",
                        EntityState.Modified => "Modifica",
                        EntityState.Deleted => "Eliminazione",
                        _ => stato.ToString()
                    },
                    DataCreazione = now
                });
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        return risultato;
    }
}
