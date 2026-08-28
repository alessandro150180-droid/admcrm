using CucineCRM.Application.Interfaces;

namespace CucineCRM.Infrastructure.Auth;

public class BCryptPasswordHasher : IPasswordHasher
{
    // WorkFactor 12: buon compromesso sicurezza/prestazioni per un'app di produzione (2026).
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
