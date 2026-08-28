namespace CucineCRM.Infrastructure.GoogleCalendar;

/// <summary>Mappa la sezione "GoogleOAuth" di appsettings.json. Vedi README per come ottenere queste credenziali.</summary>
public class GoogleOAuthOptions
{
    public const string SectionName = "GoogleOAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
