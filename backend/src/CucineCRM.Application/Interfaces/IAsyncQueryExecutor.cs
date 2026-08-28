namespace CucineCRM.Application.Interfaces;

/// <summary>
/// Esegue in modo realmente asincrono le IQueryable composte nei servizi applicativi (filtri,
/// paginazione, aggregazioni). Necessaria perché l'Application layer non referenzia EF Core
/// direttamente (vedi CucineCRM.Application.csproj) e quindi non può chiamare gli operatori
/// CountAsync/ToListAsync di Microsoft.EntityFrameworkCore sulle IQueryable&lt;T&gt; ottenute da
/// IRepository&lt;T&gt;.Query(): senza questa astrazione i servizi finirebbero per usare le
/// controparti sincrone (Count/ToList), bloccando un thread del pool per tutta la durata della
/// query invece di liberarlo durante l'I/O verso il database.
/// </summary>
public interface IAsyncQueryExecutor
{
    Task<int> CountAsync<TSource>(IQueryable<TSource> query, CancellationToken ct = default);

    Task<List<TSource>> ToListAsync<TSource>(IQueryable<TSource> query, CancellationToken ct = default);

    /// <summary>Somma lato database (SQL SUM), senza scaricare le righe per sommarle in memoria.</summary>
    Task<decimal> SumAsync(IQueryable<decimal> query, CancellationToken ct = default);
}
