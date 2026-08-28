namespace CucineCRM.Infrastructure.Auth;

/// <summary>Mappa la sezione "Jwt" di appsettings.json.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty; // in produzione: da secret manager / variabile d'ambiente
    public int ExpirationHours { get; set; } = 8;
}
