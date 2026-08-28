namespace CucineCRM.Application.Interfaces;

public record GoogleTokenResult(string AccessToken, string? RefreshToken, int ScadenzaSecondi);

/// <summary>
/// Comunicazione a basso livello con gli endpoint OAuth/Calendar di Google (implementata in
/// Infrastructure con HttpClient). L'Application layer non conosce i dettagli del protocollo,
/// solo queste operazioni.
/// </summary>
public interface IGoogleOAuthClient
{
    /// <summary>Genera un token di stato firmato (anti-CSRF) che incapsula l'Id dell'utente che avvia il collegamento.</summary>
    string GeneraStatoFirmato(int utenteId);

    /// <summary>Verifica firma e scadenza dello stato; ritorna l'UtenteId se valido, altrimenti null.</summary>
    int? VerificaStatoFirmato(string stato);

    string CostruisciUrlAutorizzazione(string statoFirmato);

    Task<GoogleTokenResult> ScambiaCodiceAsync(string code, CancellationToken ct = default);
    Task<GoogleTokenResult> RinnovaAccessTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Crea un evento sul calendario primario dell'utente proprietario dell'access token; ritorna l'Id evento Google.</summary>
    Task<string> CreaEventoCalendarioAsync(
        string accessToken, string titolo, string? descrizione, DateTime inizio, DateTime fine, CancellationToken ct = default);
}
