using System.Linq.Expressions;
using CucineCRM.Domain.Common;

namespace CucineCRM.Application.Interfaces;

/// <summary>
/// Repository generico (Repository Pattern). Le implementazioni concrete vivono in Infrastructure
/// e usano EF Core; l'Application layer dipende solo da questa astrazione.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Query composabile (IQueryable) per filtri complessi/paginazione lato Application.</summary>
    IQueryable<T> Query();

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void SoftDelete(T entity); // imposta Eliminato = true, non cancella mai lo storico

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}
