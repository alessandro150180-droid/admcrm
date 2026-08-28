namespace CucineCRM.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Elementi { get; init; } = Array.Empty<T>();
    public int Pagina { get; init; }
    public int Dimensione { get; init; }
    public int TotaleElementi { get; init; }
    public int TotalePagine => Dimensione == 0 ? 0 : (int)Math.Ceiling(TotaleElementi / (double)Dimensione);
}
