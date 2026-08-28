using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CucineCRM.Application.Interfaces;
using CucineCRM.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace CucineCRM.Infrastructure.GoogleCalendar;

public class GoogleOAuthClient : IGoogleOAuthClient
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string CalendarEventsEndpoint = "https://www.googleapis.com/calendar/v3/calendars/primary/events";
    private const string Scope = "https://www.googleapis.com/auth/calendar.events";
    private const int StatoValiditaSecondi = 600; // 10 minuti: tempo massimo per completare il consenso su Google

    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthOptions _options;
    private readonly byte[] _chiaveFirmaStato;

    public GoogleOAuthClient(HttpClient httpClient, IOptions<GoogleOAuthOptions> options, IOptions<JwtOptions> jwtOptions)
    {
        _httpClient = httpClient;
        _options = options.Value;
        // Riusa il secret JWT già configurato per firmare lo "state" OAuth (HMAC anti-CSRF):
        // evita di dover configurare/gestire un secret aggiuntivo solo per questo scopo.
        _chiaveFirmaStato = Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey);
    }

    public string GeneraStatoFirmato(int utenteId)
    {
        var payload = $"{utenteId}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var firma = CalcolaFirma(payload);
        return CodificaBase64Url($"{payload}|{firma}");
    }

    public int? VerificaStatoFirmato(string stato)
    {
        try
        {
            var decodificato = DecodificaBase64Url(stato);
            var parti = decodificato.Split('|');
            if (parti.Length != 3)
                return null;

            var payload = $"{parti[0]}|{parti[1]}";
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(CalcolaFirma(payload)), Encoding.UTF8.GetBytes(parti[2])))
                return null;

            var timestamp = long.Parse(parti[1]);
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp > StatoValiditaSecondi)
                return null;

            return int.Parse(parti[0]);
        }
        catch
        {
            return null; // stato malformato/manomesso: trattato come non valido, non come errore
        }
    }

    public string CostruisciUrlAutorizzazione(string statoFirmato)
    {
        var query = string.Join('&', new[]
        {
            $"client_id={Uri.EscapeDataString(_options.ClientId)}",
            $"redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}",
            "response_type=code",
            $"scope={Uri.EscapeDataString(Scope)}",
            "access_type=offline", // necessario per ottenere un refresh_token
            "prompt=consent",      // forza il rilascio del refresh_token anche ai consensi successivi
            $"state={Uri.EscapeDataString(statoFirmato)}"
        });
        return $"{AuthorizationEndpoint}?{query}";
    }

    public async Task<GoogleTokenResult> ScambiaCodiceAsync(string code, CancellationToken ct = default)
    {
        var risposta = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        }), ct);

        return await LeggiRispostaTokenAsync(risposta, refreshTokenDiFallback: null, ct);
    }

    public async Task<GoogleTokenResult> RinnovaAccessTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var risposta = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "refresh_token"
        }), ct);

        // Il grant "refresh_token" non restituisce un nuovo refresh_token: si mantiene quello esistente.
        return await LeggiRispostaTokenAsync(risposta, refreshTokenDiFallback: refreshToken, ct);
    }

    public async Task<string> CreaEventoCalendarioAsync(
        string accessToken, string titolo, string? descrizione, DateTime inizio, DateTime fine, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, CalendarEventsEndpoint)
        {
            Content = JsonContent.Create(new
            {
                summary = titolo,
                description = descrizione,
                start = new { dateTime = inizio.ToString("O") },
                end = new { dateTime = fine.ToString("O") }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var risposta = await _httpClient.SendAsync(request, ct);
        var corpo = await risposta.Content.ReadAsStringAsync(ct);
        if (!risposta.IsSuccessStatusCode)
            throw new InvalidOperationException($"Errore nella creazione dell'evento su Google Calendar ({risposta.StatusCode}): {corpo}");

        using var json = JsonDocument.Parse(corpo);
        return json.RootElement.GetProperty("id").GetString()!;
    }

    private static async Task<GoogleTokenResult> LeggiRispostaTokenAsync(
        HttpResponseMessage risposta, string? refreshTokenDiFallback, CancellationToken ct)
    {
        var corpo = await risposta.Content.ReadAsStringAsync(ct);
        if (!risposta.IsSuccessStatusCode)
            throw new InvalidOperationException($"Errore nello scambio del token con Google ({risposta.StatusCode}): {corpo}");

        using var json = JsonDocument.Parse(corpo);
        var root = json.RootElement;

        var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshTokenDiFallback;

        return new GoogleTokenResult(
            root.GetProperty("access_token").GetString()!,
            refreshToken,
            root.GetProperty("expires_in").GetInt32());
    }

    private string CalcolaFirma(string payload) =>
        Convert.ToBase64String(new HMACSHA256(_chiaveFirmaStato).ComputeHash(Encoding.UTF8.GetBytes(payload)));

    private static string CodificaBase64Url(string testo) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(testo)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string DecodificaBase64Url(string testo)
    {
        var base64 = testo.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }
}
