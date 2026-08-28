using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<UtenteDto> CreaUtenteAsync(CreaUtenteDto request, CancellationToken ct = default);
    Task CambiaPasswordAsync(int utenteId, CambiaPasswordDto request, CancellationToken ct = default);
}
