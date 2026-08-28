using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/google-calendar")]
[Authorize(Policy = "TuttiIRuoli")]
public class GoogleCalendarController : ControllerBase
{
    private readonly IGoogleCalendarSyncService _syncService;

    public GoogleCalendarController(IGoogleCalendarSyncService syncService)
    {
        _syncService = syncService;
    }

    /// <summary>Restituisce l'URL a cui reindirizzare il browser per avviare il consenso OAuth di Google.</summary>
    [HttpGet("connetti")]
    public async Task<IActionResult> Connetti(CancellationToken ct)
    {
        var url = await _syncService.GetUrlConnessioneAsync(ct);
        return Ok(new { url });
    }

    /// <summary>Indica se l'utente autenticato ha già collegato il proprio account Google Calendar.</summary>
    [HttpGet("stato")]
    public async Task<IActionResult> Stato(CancellationToken ct)
    {
        var collegato = await _syncService.IsCollegatoAsync(ct);
        return Ok(new { collegato });
    }

    /// <summary>
    /// Endpoint di redirect di Google al termine del consenso OAuth: il browser dell'utente arriva
    /// qui senza il token JWT dell'app (è una navigazione, non una chiamata API autenticata), quindi
    /// l'identità dell'utente è recuperata dal parametro "state" firmato generato da /connetti.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, CancellationToken ct)
    {
        await _syncService.GestisciCallbackAsync(code, state, ct);
        return Content(
            "<html><body style=\"font-family: sans-serif; text-align: center; margin-top: 3rem;\">" +
            "<h3>Google Calendar collegato con successo.</h3><p>Puoi chiudere questa finestra e tornare al CRM.</p>" +
            "</body></html>",
            "text/html");
    }
}
