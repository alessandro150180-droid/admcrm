using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IFornitoreService
{
    Task<IReadOnlyList<FornitoreDto>> GetListaAsync(CancellationToken ct = default);
    Task<FornitoreDto> CreaAsync(CreaFornitoreDto request, CancellationToken ct = default);
}
