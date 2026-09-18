using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IChatAiService
{
    Task<ChatAiRispostaDto> ChiediAsync(ChatAiRichiestaDto richiesta, CancellationToken ct = default);
}
