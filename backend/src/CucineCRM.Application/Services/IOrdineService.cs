using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IOrdineService
{
    Task<PagedResult<OrdineDto>> GetListaAsync(FiltriListaDto filtri, CancellationToken ct = default);
    Task<OrdineDto> GetDettaglioAsync(int ordineId, CancellationToken ct = default);
    Task<OrdineDto> CreaAsync(CreaOrdineDto request, CancellationToken ct = default);
    Task<OrdineDto> AggiornaStatoAsync(int ordineId, AggiornaStatoOrdineDto request, CancellationToken ct = default);
}
