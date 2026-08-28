using CucineCRM.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CucineCRM.Infrastructure.Repositories;

public class EfAsyncQueryExecutor : IAsyncQueryExecutor
{
    public Task<int> CountAsync<TSource>(IQueryable<TSource> query, CancellationToken ct = default) =>
        query.CountAsync(ct);

    public Task<List<TSource>> ToListAsync<TSource>(IQueryable<TSource> query, CancellationToken ct = default) =>
        query.ToListAsync(ct);

    public Task<decimal> SumAsync(IQueryable<decimal> query, CancellationToken ct = default) =>
        query.SumAsync(ct);
}
