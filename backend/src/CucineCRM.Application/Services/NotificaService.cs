using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class NotificaService : INotificaService
{
    private const string TipoAttivitaScaduta = "AttivitaScaduta";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ICurrentUserService _currentUser;

    public NotificaService(IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificaDto>> GetPerUtenteCorrenteAsync(bool? soloNonLette, CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var query = _unitOfWork.Notifiche.Query().Where(n => n.UtenteId == utenteId);
        if (soloNonLette == true)
            query = query.Where(n => !n.Letta);

        return await _queryExecutor.ToListAsync(query
            .OrderByDescending(n => n.DataCreazione)
            .Select(n => new NotificaDto(
                n.Id, n.Tipo, n.Titolo, n.Messaggio, n.RiferimentoEntitaId, n.Letta, n.DataCreazione)), ct);
    }

    public async Task SegnaComeLettaAsync(int notificaId, CancellationToken ct = default)
    {
        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var notifica = await _unitOfWork.Notifiche.GetByIdAsync(notificaId, ct)
            ?? throw new NotFoundException(nameof(Notifica), notificaId);

        if (notifica.UtenteId != utenteId)
            throw new ForbiddenAccessException("Questa notifica non appartiene all'utente autenticato.");

        if (notifica.Letta)
            return;

        notifica.Letta = true;
        notifica.DataLettura = DateTime.UtcNow;
        _unitOfWork.Notifiche.Update(notifica);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> GeneraPerAttivitaScaduteAsync(CancellationToken ct = default)
    {
        var scadute = await _queryExecutor.ToListAsync(_unitOfWork.Attivita.Query()
            .Where(a => !a.Completata && a.DataScadenza < DateTime.UtcNow)
            .Select(a => new { a.Id, a.UtenteId, a.Titolo, a.DataScadenza }), ct);

        if (scadute.Count == 0)
            return 0;

        // Evita di generare due volte la notifica per la stessa attività già segnalata in precedenza.
        var idAttivitaGiaNotificate = new HashSet<int>(
            await _queryExecutor.ToListAsync(_unitOfWork.Notifiche.Query()
                .Where(n => n.Tipo == TipoAttivitaScaduta && n.RiferimentoEntitaId != null)
                .Select(n => n.RiferimentoEntitaId!.Value), ct));

        var nuove = 0;
        foreach (var a in scadute)
        {
            if (idAttivitaGiaNotificate.Contains(a.Id))
                continue;

            await _unitOfWork.Notifiche.AddAsync(new Notifica
            {
                UtenteId = a.UtenteId,
                Tipo = TipoAttivitaScaduta,
                Titolo = "Attività scaduta",
                Messaggio = $"\"{a.Titolo}\" era prevista per il {a.DataScadenza:dd/MM/yyyy}.",
                RiferimentoEntitaId = a.Id,
                Letta = false
            }, ct);
            nuove++;
        }

        if (nuove > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return nuove;
    }
}
