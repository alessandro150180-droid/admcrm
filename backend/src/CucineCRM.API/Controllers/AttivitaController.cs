using CucineCRM.Application.DTOs;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class AttivitaController : ControllerBase
{
    private readonly IAttivitaService _attivitaService;
    private readonly IGoogleCalendarSyncService _googleCalendarSyncService;

    public AttivitaController(IAttivitaService attivitaService, IGoogleCalendarSyncService googleCalendarSyncService)
    {
        _attivitaService = attivitaService;
        _googleCalendarSyncService = googleCalendarSyncService;
    }

    /// <summary>Elenco attività (telefonate, visite, follow-up...) paginato e filtrabile. Già ristretto allo scope dell'utente.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLista([FromQuery] FiltriAttivitaDto filtri, CancellationToken ct)
    {
        var result = await _attivitaService.GetListaAsync(filtri, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDettaglio(int id, CancellationToken ct)
    {
        var result = await _attivitaService.GetDettaglioAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Crea una nuova attività, assegnata all'utente autenticato.</summary>
    [HttpPost]
    public async Task<IActionResult> Crea([FromBody] CreaAttivitaDto request, CancellationToken ct)
    {
        var result = await _attivitaService.CreaAsync(request, ct);
        return CreatedAtAction(nameof(GetDettaglio), new { id = result.Id }, result);
    }

    /// <summary>Aggiorna lo stato dell'attività (DaFare, InCorso, Completata, Annullata).</summary>
    [HttpPatch("{id:int}/stato")]
    public async Task<IActionResult> AggiornaStato(int id, [FromBody] AggiornaStatoAttivitaDto request, CancellationToken ct)
    {
        var result = await _attivitaService.AggiornaStatoAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Crea/aggiorna l'evento Google Calendar collegato a questa attività (richiede che
    /// l'utente responsabile abbia già collegato il proprio account tramite /api/google-calendar/connetti).</summary>
    [HttpPost("{id:int}/sincronizza-calendario")]
    public async Task<IActionResult> SincronizzaCalendario(int id, CancellationToken ct)
    {
        var googleEventId = await _googleCalendarSyncService.SincronizzaAttivitaAsync(id, ct);
        return Ok(new { googleEventId });
    }
}
