using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface INotificaService
{
    Task<IReadOnlyList<NotificaDto>> GetPerUtenteCorrenteAsync(bool? soloNonLette, CancellationToken ct = default);
    Task SegnaComeLettaAsync(int notificaId, CancellationToken ct = default);

    /// <summary>Scansiona le attività scadute e non completate e genera una notifica per ciascun
    /// responsabile, evitando duplicati. Ritorna il numero di notifiche create.</summary>
    Task<int> GeneraPerAttivitaScaduteAsync(CancellationToken ct = default);
}
