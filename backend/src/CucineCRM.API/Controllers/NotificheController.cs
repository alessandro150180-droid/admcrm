using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class NotificheController : ControllerBase
{
    private readonly INotificaService _notificaService;

    public NotificheController(INotificaService notificaService)
    {
        _notificaService = notificaService;
    }

    /// <summary>Notifiche dell'utente autenticato, più recenti per prime.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLista([FromQuery] bool? soloNonLette, CancellationToken ct)
    {
        var result = await _notificaService.GetPerUtenteCorrenteAsync(soloNonLette, ct);
        return Ok(result);
    }

    [HttpPatch("{id:int}/letta")]
    public async Task<IActionResult> SegnaComeLetta(int id, CancellationToken ct)
    {
        await _notificaService.SegnaComeLettaAsync(id, ct);
        return NoContent();
    }

    /// <summary>Scansiona le attività scadute e genera le notifiche mancanti (nessuno scheduler in questa fase: da richiamare periodicamente, es. all'apertura della dashboard).</summary>
    [HttpPost("genera-scadute")]
    [Authorize(Policy = "SoloDirezione")]
    public async Task<IActionResult> GeneraScadute(CancellationToken ct)
    {
        var generate = await _notificaService.GeneraPerAttivitaScaduteAsync(ct);
        return Ok(new { notificheGenerate = generate });
    }
}
