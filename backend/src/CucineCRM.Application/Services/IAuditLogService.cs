using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetListaAsync(FiltriAuditLogDto filtri, CancellationToken ct = default);
}
