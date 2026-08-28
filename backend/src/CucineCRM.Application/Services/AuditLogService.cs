using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;

namespace CucineCRM.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;

    public AuditLogService(IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor)
    {
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
    }

    public async Task<PagedResult<AuditLogDto>> GetListaAsync(FiltriAuditLogDto filtri, CancellationToken ct = default)
    {
        var query = _unitOfWork.AuditLogs.Query();

        if (!string.IsNullOrWhiteSpace(filtri.NomeEntita))
            query = query.Where(a => a.NomeEntita == filtri.NomeEntita);
        if (filtri.EntitaId.HasValue)
            query = query.Where(a => a.EntitaId == filtri.EntitaId.Value);
        if (filtri.UtenteId.HasValue)
            query = query.Where(a => a.UtenteId == filtri.UtenteId.Value);

        var totale = await _queryExecutor.CountAsync(query, ct);

        var elementi = await _queryExecutor.ToListAsync(query
            .OrderByDescending(a => a.DataCreazione)
            .Skip((filtri.Pagina - 1) * filtri.Dimensione)
            .Take(filtri.Dimensione)
            .Select(a => new AuditLogDto(
                a.Id, a.UtenteId,
                a.Utente == null ? null : a.Utente.Nome + " " + a.Utente.Cognome,
                a.NomeEntita, a.EntitaId, a.Azione, a.DataCreazione)), ct);

        return new PagedResult<AuditLogDto>
        {
            Elementi = elementi,
            Pagina = filtri.Pagina,
            Dimensione = filtri.Dimensione,
            TotaleElementi = totale
        };
    }
}
