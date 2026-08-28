using CucineCRM.Application.Common;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class GoogleCalendarSyncService : IGoogleCalendarSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;
    private readonly IGoogleOAuthClient _googleClient;
    private readonly ICurrentUserService _currentUser;

    public GoogleCalendarSyncService(
        IUnitOfWork unitOfWork, IDataScopingService scoping, IGoogleOAuthClient googleClient, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
        _googleClient = googleClient;
        _currentUser = currentUser;
    }

    public Task<string> GetUrlConnessioneAsync(CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var stato = _googleClient.GeneraStatoFirmato(utenteId);
        return Task.FromResult(_googleClient.CostruisciUrlAutorizzazione(stato));
    }

    public async Task GestisciCallbackAsync(string code, string stato, CancellationToken ct = default)
    {
        var utenteId = _googleClient.VerificaStatoFirmato(stato)
            ?? throw new AuthenticationException("Collegamento a Google Calendar non valido o scaduto: riprova.");

        var token = await _googleClient.ScambiaCodiceAsync(code, ct);

        var utente = await _unitOfWork.Utenti.GetByIdAsync(utenteId, ct)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        utente.GoogleAccessToken = token.AccessToken;
        // Google restituisce il refresh_token solo al primo consenso (prompt=consent lo forza comunque
        // anche sui successivi): se assente in questa risposta, manteniamo quello già salvato.
        if (!string.IsNullOrEmpty(token.RefreshToken))
            utente.GoogleRefreshToken = token.RefreshToken;
        utente.GoogleTokenScadenza = DateTime.UtcNow.AddSeconds(token.ScadenzaSecondi);

        _unitOfWork.Utenti.Update(utente);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<bool> IsCollegatoAsync(CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var utente = await _unitOfWork.Utenti.GetByIdAsync(utenteId, ct)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        return !string.IsNullOrEmpty(utente.GoogleRefreshToken);
    }

    public async Task<string> SincronizzaAttivitaAsync(int attivitaId, CancellationToken ct = default)
    {
        var attivita = await _unitOfWork.Attivita.GetByIdAsync(attivitaId, ct)
            ?? throw new NotFoundException(nameof(Attivita), attivitaId);

        var cliente = await _unitOfWork.Clienti.GetByIdAsync(attivita.ClienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), attivita.ClienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non hai accesso a questa attività.");

        var utente = await _unitOfWork.Utenti.GetByIdAsync(attivita.UtenteId, ct)
            ?? throw new NotFoundException(nameof(Utente), attivita.UtenteId);

        if (string.IsNullOrEmpty(utente.GoogleRefreshToken))
            throw new ValidationAppException(
                "L'utente responsabile di questa attività non ha collegato il proprio Google Calendar.");

        var accessToken = await AssicuraTokenValidoAsync(utente, ct);

        var eventoId = await _googleClient.CreaEventoCalendarioAsync(
            accessToken, attivita.Titolo, attivita.Descrizione, attivita.DataScadenza, attivita.DataScadenza.AddHours(1), ct);

        var calendario = (await _unitOfWork.Calendario.FindAsync(c => c.AttivitaId == attivitaId, ct)).FirstOrDefault();
        if (calendario is null)
        {
            calendario = new Calendario { ClienteId = attivita.ClienteId, AttivitaId = attivitaId };
            await _unitOfWork.Calendario.AddAsync(calendario, ct);
        }

        calendario.DataEvento = attivita.DataScadenza;
        calendario.GoogleEventId = eventoId;
        calendario.SincronizzatoConGoogle = true;
        calendario.UltimaSincronizzazione = DateTime.UtcNow;
        _unitOfWork.Calendario.Update(calendario);

        await _unitOfWork.SaveChangesAsync(ct);
        return eventoId;
    }

    private async Task<string> AssicuraTokenValidoAsync(Utente utente, CancellationToken ct)
    {
        if (utente.GoogleAccessToken is not null && utente.GoogleTokenScadenza > DateTime.UtcNow.AddMinutes(1))
            return utente.GoogleAccessToken;

        var token = await _googleClient.RinnovaAccessTokenAsync(utente.GoogleRefreshToken!, ct);
        utente.GoogleAccessToken = token.AccessToken;
        utente.GoogleTokenScadenza = DateTime.UtcNow.AddSeconds(token.ScadenzaSecondi);
        _unitOfWork.Utenti.Update(utente);
        // Il salvataggio avviene a fine SincronizzaAttivitaAsync insieme al resto: qui basta
        // aggiornare l'entità tracciata.
        return token.AccessToken;
    }
}
