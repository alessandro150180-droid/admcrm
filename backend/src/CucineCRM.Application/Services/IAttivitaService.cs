using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IAttivitaService
{
    Task<PagedResult<AttivitaDto>> GetListaAsync(FiltriAttivitaDto filtri, CancellationToken ct = default);
    Task<AttivitaDto> GetDettaglioAsync(int attivitaId, CancellationToken ct = default);
    Task<AttivitaDto> CreaAsync(CreaAttivitaDto request, CancellationToken ct = default);
    Task<AttivitaDto> AggiornaStatoAsync(int attivitaId, AggiornaStatoAttivitaDto request, CancellationToken ct = default);
}
