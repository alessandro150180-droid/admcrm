using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface INotaClienteService
{
    Task<IReadOnlyList<NotaClienteDto>> GetPerClienteAsync(int clienteId, CancellationToken ct = default);
    Task<NotaClienteDto> CreaAsync(CreaNotaClienteDto request, CancellationToken ct = default);
}
