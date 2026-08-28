using System.Linq.Expressions;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Common;
using CucineCRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CucineCRM.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await DbSet.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    // AsNoTracking: Query() è usata solo per filtri/paginazione in lettura (liste, dashboard),
    // mai per poi richiamare Update/SaveChanges sugli stessi risultati — tracciarli sarebbe
    // solo overhead di memoria/CPU sul ChangeTracker senza alcun beneficio.
    public IQueryable<T> Query() => DbSet.AsNoTracking();

    public async Task AddAsync(T entity, CancellationToken ct = default) => await DbSet.AddAsync(entity, ct);

    public void Update(T entity) => DbSet.Update(entity);

    public void SoftDelete(T entity)
    {
        entity.Eliminato = true;
        DbSet.Update(entity);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default) =>
        predicate is null ? await DbSet.CountAsync(ct) : await DbSet.CountAsync(predicate, ct);
}
