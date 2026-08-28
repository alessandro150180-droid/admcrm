namespace CucineCRM.Application.Services;

public interface IGoogleCalendarSyncService
{
    /// <summary>URL a cui reindirizzare l'utente per avviare il consenso OAuth di Google.</summary>
    Task<string> GetUrlConnessioneAsync(CancellationToken ct = default);

    /// <summary>Gestisce il redirect di ritorno da Google: scambia il code e salva i token sull'utente.</summary>
    Task GestisciCallbackAsync(string code, string stato, CancellationToken ct = default);

    Task<bool> IsCollegatoAsync(CancellationToken ct = default);

    /// <summary>Crea/aggiorna l'evento Google Calendar collegato a un'Attività; ritorna l'Id evento Google.</summary>
    Task<string> SincronizzaAttivitaAsync(int attivitaId, CancellationToken ct = default);
}
