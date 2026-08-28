using CucineCRM.API.Export;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class ClientiController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly INotaClienteService _notaClienteService;

    public ClientiController(IClienteService clienteService, INotaClienteService notaClienteService)
    {
        _clienteService = clienteService;
        _notaClienteService = notaClienteService;
    }

    /// <summary>Elenco clienti paginato e filtrabile. I risultati sono già ristretti allo scope dell'utente.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLista([FromQuery] FiltriListaDto filtri, CancellationToken ct)
    {
        var result = await _clienteService.GetListaAsync(filtri, ct);
        return Ok(result);
    }

    /// <summary>Scheda cliente completa: anagrafica, storico ordini aggregato, KPI.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDettaglio(int id, CancellationToken ct)
    {
        var result = await _clienteService.GetDettaglioAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "DirezioneOAreaManager")]
    public async Task<IActionResult> Crea([FromBody] CreaClienteDto request, CancellationToken ct)
    {
        var result = await _clienteService.CreaAsync(request, ct);
        return CreatedAtAction(nameof(GetDettaglio), new { id = result.Id }, result);
    }

    /// <summary>Note commerciali sul cliente (storico interazioni), più recenti per prime.</summary>
    [HttpGet("{id:int}/note")]
    public async Task<IActionResult> GetNote(int id, CancellationToken ct)
    {
        var result = await _notaClienteService.GetPerClienteAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Aggiunge una nota al cliente, attribuita all'utente autenticato.</summary>
    [HttpPost("{id:int}/note")]
    public async Task<IActionResult> CreaNota(int id, [FromBody] AggiungiNotaDto request, CancellationToken ct)
    {
        var result = await _notaClienteService.CreaAsync(new CreaNotaClienteDto(id, request.Testo), ct);
        return CreatedAtAction(nameof(GetNote), new { id }, result);
    }

    /// <summary>Imposta la percentuale di provvigione riconosciuta all'agente per questo cliente.</summary>
    [HttpPut("{id:int}/provvigione")]
    [Authorize(Policy = "DirezioneOAreaManager")]
    public async Task<IActionResult> ImpostaProvvigione(int id, [FromBody] ImpostaProvvigioneDto request, CancellationToken ct)
    {
        var result = await _clienteService.ImpostaProvvigioneAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Esporta in CSV l'elenco clienti (stessi filtri della lista, senza paginazione).</summary>
    [HttpGet("export/csv")]
    public async Task<IActionResult> EsportaCsv([FromQuery] FiltriListaDto filtri, CancellationToken ct)
    {
        var tutti = await _clienteService.GetListaAsync(filtri with { Pagina = 1, Dimensione = 100_000 }, ct);
        var csv = CsvExporter.EsportaClienti(tutti.Elementi);
        return File(csv, "text/csv", "clienti.csv");
    }

    /// <summary>Esporta in PDF la scheda completa del cliente (anagrafica, KPI, note).</summary>
    [HttpGet("{id:int}/export/pdf")]
    public async Task<IActionResult> EsportaPdf(int id, CancellationToken ct)
    {
        var dettaglio = await _clienteService.GetDettaglioAsync(id, ct);
        var note = await _notaClienteService.GetPerClienteAsync(id, ct);
        var pdf = PdfExporter.EsportaSchedaCliente(dettaglio, note);
        return File(pdf, "application/pdf", $"scheda-cliente-{dettaglio.Anagrafica.CodiceCliente}.pdf");
    }
}
