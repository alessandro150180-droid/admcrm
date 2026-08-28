using CucineCRM.Application.DTOs;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloDirezione")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>Storico di tutte le operazioni di scrittura (Creazione/Modifica/Eliminazione) sulle entità, tracciato automaticamente.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLista([FromQuery] FiltriAuditLogDto filtri, CancellationToken ct)
    {
        var result = await _auditLogService.GetListaAsync(filtri, ct);
        return Ok(result);
    }
}
