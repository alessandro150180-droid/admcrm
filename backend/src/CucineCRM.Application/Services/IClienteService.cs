using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IClienteService
{
    Task<PagedResult<ClienteDto>> GetListaAsync(FiltriListaDto filtri, CancellationToken ct = default);
    Task<ClienteDettaglioDto> GetDettaglioAsync(int clienteId, CancellationToken ct = default);
    Task<ClienteDto> CreaAsync(CreaClienteDto request, CancellationToken ct = default);
    Task<ClienteDto> ImpostaProvvigioneAsync(int clienteId, ImpostaProvvigioneDto request, CancellationToken ct = default);
}
