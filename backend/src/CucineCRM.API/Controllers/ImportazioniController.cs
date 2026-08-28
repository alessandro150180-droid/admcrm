using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloDirezione")]
public class ImportazioniController : ControllerBase
{
    private readonly IImportazioneOrdiniService _importazioneOrdiniService;
    private readonly IImportazioneClientiService _importazioneClientiService;
    private readonly IImportazioneFatturatoMensileService _importazioneFatturatoMensileService;

    public ImportazioniController(
        IImportazioneOrdiniService importazioneOrdiniService,
        IImportazioneClientiService importazioneClientiService,
        IImportazioneFatturatoMensileService importazioneFatturatoMensileService)
    {
        _importazioneOrdiniService = importazioneOrdiniService;
        _importazioneClientiService = importazioneClientiService;
        _importazioneFatturatoMensileService = importazioneFatturatoMensileService;
    }

    /// <summary>
    /// Importa ordini da un file Excel (.xlsx). Colonne attese in prima riga: CodiceCliente,
    /// DataOrdine, Importo, NumeroCucine, NumeroElettrodomestici, NumeroComplementi,
    /// RiferimentoEsterno (opzionale, usato per riconoscere i duplicati).
    /// </summary>
    [HttpPost("ordini")]
    [RequestSizeLimit(20_000_000)] // 20 MB: sufficiente per un file Excel di import, evita upload abnormi
    public async Task<IActionResult> ImportaOrdini(IFormFile file, [FromForm] string periodoCompetenza, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Il file è vuoto." });

        await using var stream = file.OpenReadStream();
        var result = await _importazioneOrdiniService.ImportaOrdiniAsync(stream, file.FileName, periodoCompetenza, ct);
        return Ok(result);
    }

    /// <summary>
    /// Importa anagrafiche clienti da un file Excel (.xlsx). Colonne attese in prima riga:
    /// RagioneSociale, CodiceCliente, PartitaIVA, Indirizzo, Citta, Provincia, Regione, CAP,
    /// Telefono, Email, EmailAgente (deve corrispondere a un agente già esistente). I clienti con
    /// CodiceCliente già presente in anagrafica sono considerati duplicati e vengono scartati.
    /// </summary>
    [HttpPost("clienti")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ImportaClienti(IFormFile file, [FromForm] string periodoCompetenza, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Il file è vuoto." });

        await using var stream = file.OpenReadStream();
        var result = await _importazioneClientiService.ImportaClientiAsync(stream, file.FileName, periodoCompetenza, ct);
        return Ok(result);
    }

    /// <summary>
    /// Importa fatturato mensile da un file "a pivot": una riga per cliente (colonna CodiceCliente),
    /// con una colonna per ogni mese (es. "Aprile 2026") contenente il fatturato di quel mese.
    /// Genera un Ordine sintetico per ogni cella valorizzata; se presente, aggiorna anche la
    /// percentuale di provvigione del cliente (colonna "Provvigione").
    /// </summary>
    [HttpPost("fatturato-mensile")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ImportaFatturatoMensile(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Il file è vuoto." });

        await using var stream = file.OpenReadStream();
        var result = await _importazioneFatturatoMensileService.ImportaFatturatoMensileAsync(stream, file.FileName, ct);
        return Ok(result);
    }
}
