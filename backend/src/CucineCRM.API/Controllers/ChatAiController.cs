using CucineCRM.Application.DTOs;
using CucineCRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/chat-ai")]
[Authorize(Policy = "TuttiIRuoli")]
public class ChatAiController : ControllerBase
{
    private readonly IChatAiService _chatAiService;

    public ChatAiController(IChatAiService chatAiService)
    {
        _chatAiService = chatAiService;
    }

    /// <summary>Invia un messaggio all'assistente AI, con la cronologia della conversazione
    /// (mantenuta lato frontend, nessuna persistenza server-side).</summary>
    [HttpPost("messaggio")]
    public async Task<IActionResult> Messaggio([FromBody] ChatAiRichiestaDto request, CancellationToken ct)
    {
        var result = await _chatAiService.ChiediAsync(request, ct);
        return Ok(result);
    }
}
