using System.Security.Claims;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

/// <summary>
/// Circolari, PDF e file Excel pubblicati dalla direzione: visibili e scaricabili da tutta la
/// rete vendita, ma la pubblicazione (creazione/eliminazione) resta riservata alla direzione.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class ComunicazioniController : ControllerBase
{
    private readonly IComunicazioneService _comunicazioneService;

    public ComunicazioniController(IComunicazioneService comunicazioneService)
    {
        _comunicazioneService = comunicazioneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLista(CancellationToken ct)
    {
        var result = await _comunicazioneService.GetListaAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var (contenuto, tipoContenuto, nomeFile) = await _comunicazioneService.ScaricaAsync(id, ct);
        return File(contenuto, tipoContenuto, nomeFile);
    }

    [HttpPost]
    [Authorize(Policy = "SoloDirezione")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Crea(
        IFormFile file, [FromForm] string titolo, [FromForm] string? descrizione, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Il file è vuoto." });

        var utenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Claim utente mancante."));

        await using var stream = file.OpenReadStream();
        var result = await _comunicazioneService.CreaAsync(stream, file.FileName, file.ContentType, titolo, descrizione, utenteId, ct);
        return CreatedAtAction(nameof(GetLista), result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SoloDirezione")]
    public async Task<IActionResult> Elimina(int id, CancellationToken ct)
    {
        await _comunicazioneService.EliminaAsync(id, ct);
        return NoContent();
    }
}
