using CucineCRM.Application.DTOs;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class ObiettiviVenditaController : ControllerBase
{
    private readonly IObiettivoVenditaService _obiettivoService;

    public ObiettiviVenditaController(IObiettivoVenditaService obiettivoService)
    {
        _obiettivoService = obiettivoService;
    }

    /// <summary>Obiettivi dell'anno (per agente o per tutti gli agenti visibili), con confronto vs il fatturato realizzato.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLista([FromQuery] int anno, [FromQuery] int? agenteId, CancellationToken ct)
    {
        var result = await _obiettivoService.GetListaAsync(anno, agenteId, ct);
        return Ok(result);
    }

    /// <summary>Imposta l'obiettivo di un agente per un mese/anno (crea o aggiorna se già esistente).</summary>
    [HttpPut]
    [Authorize(Policy = "DirezioneOAreaManager")]
    public async Task<IActionResult> Imposta([FromBody] ImpostaObiettivoDto request, CancellationToken ct)
    {
        var result = await _obiettivoService.ImpostaAsync(request, ct);
        return Ok(result);
    }
}
