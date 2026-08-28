using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IObiettivoVenditaService
{
    Task<IReadOnlyList<ObiettivoVenditaDto>> GetListaAsync(int anno, int? agenteId, CancellationToken ct = default);
    Task<ObiettivoVenditaDto> ImpostaAsync(ImpostaObiettivoDto request, CancellationToken ct = default);
}
