using CucineCRM.Application.Interfaces;
using CucineCRM.Application.Common;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.Services;

public class DataScopingService : IDataScopingService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DataScopingService(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<int>?> GetAgentiVisibiliAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            throw new AuthenticationException("Utente non autenticato.");

        switch (_currentUser.Ruolo)
        {
            case RuoloUtente.Amministratore:
            case RuoloUtente.DirettoreCommerciale:
                return null; // nessun filtro: vede tutto

            case RuoloUtente.AreaManager:
                {
                    var managerAgenteId = _currentUser.AgenteId
                        ?? throw new ValidationAppException(
                            "L'utente Area Manager non è collegato a nessun record Agente.");

                    var agentiGestiti = await _unitOfWork.Agenti.FindAsync(
                        a => a.AreaManagerId == managerAgenteId, ct);

                    return agentiGestiti.Select(a => a.Id).ToList();
                }

            case RuoloUtente.Agente:
                {
                    var proprioAgenteId = _currentUser.AgenteId
                        ?? throw new ValidationAppException(
                            "L'utente Agente non è collegato a nessun record Agente.");

                    return new List<int> { proprioAgenteId };
                }

            default:
                return new List<int>(); // ruolo sconosciuto: nessuna visibilità per sicurezza
        }
    }

    public async Task<bool> PuoAccedereAdAgenteAsync(int agenteId, CancellationToken ct = default)
    {
        var visibili = await GetAgentiVisibiliAsync(ct);
        return visibili is null || visibili.Contains(agenteId);
    }
}
