using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.Services;

public class AttivitaService : IAttivitaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ICurrentUserService _currentUser;

    public AttivitaService(
        IUnitOfWork unitOfWork, IDataScopingService scoping, IAsyncQueryExecutor queryExecutor, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
        _queryExecutor = queryExecutor;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AttivitaDto>> GetListaAsync(FiltriAttivitaDto filtri, CancellationToken ct = default)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);

        var query = _unitOfWork.Attivita.Query();

        // Stesso pattern di scoping di Clienti/Ordini: la visibilità segue il cliente collegato all'attività.
        if (agentiVisibili is not null)
            query = query.Where(a => agentiVisibili.Contains(a.Cliente.AgenteId));

        if (filtri.AgenteId.HasValue)
            query = query.Where(a => a.Cliente.AgenteId == filtri.AgenteId.Value);
        if (filtri.Stato.HasValue)
            query = query.Where(a => a.Stato == filtri.Stato.Value);
        if (filtri.SoloScadute == true)
            query = query.Where(a => !a.Completata && a.DataScadenza < DateTime.UtcNow);

        var totale = await _queryExecutor.CountAsync(query, ct);

        var elementi = await _queryExecutor.ToListAsync(query
            .OrderBy(a => a.DataScadenza)
            .Skip((filtri.Pagina - 1) * filtri.Dimensione)
            .Take(filtri.Dimensione)
            .Select(a => new AttivitaDto(
                a.Id, a.ClienteId, a.Cliente.RagioneSociale, a.UtenteId,
                a.Utente.Nome + " " + a.Utente.Cognome, a.TipoAttivita, a.Titolo, a.Descrizione,
                a.Priorita, a.DataScadenza, a.Completata, a.Stato)), ct);

        return new PagedResult<AttivitaDto>
        {
            Elementi = elementi,
            Pagina = filtri.Pagina,
            Dimensione = filtri.Dimensione,
            TotaleElementi = totale
        };
    }

    public async Task<AttivitaDto> GetDettaglioAsync(int attivitaId, CancellationToken ct = default)
    {
        var (attivita, cliente) = await CaricaAttivitaEClienteAsync(attivitaId, ct);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non hai accesso a questa attività.");

        return await MapToDtoAsync(attivita, cliente.RagioneSociale, ct);
    }

    public async Task<AttivitaDto> CreaAsync(CreaAttivitaDto request, CancellationToken ct = default)
    {
        var cliente = await _unitOfWork.Clienti.GetByIdAsync(request.ClienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), request.ClienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non puoi creare attività per un cliente che non gestisci.");

        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var attivita = new Attivita
        {
            ClienteId = request.ClienteId,
            UtenteId = utenteId,
            TipoAttivita = request.TipoAttivita,
            Titolo = request.Titolo,
            Descrizione = request.Descrizione,
            Priorita = request.Priorita,
            DataScadenza = request.DataScadenza,
            Stato = StatoAttivita.DaFare,
            Completata = false
        };

        await _unitOfWork.Attivita.AddAsync(attivita, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await MapToDtoAsync(attivita, cliente.RagioneSociale, ct);
    }

    public async Task<AttivitaDto> AggiornaStatoAsync(int attivitaId, AggiornaStatoAttivitaDto request, CancellationToken ct = default)
    {
        var (attivita, cliente) = await CaricaAttivitaEClienteAsync(attivitaId, ct);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non hai accesso a questa attività.");

        attivita.Stato = request.NuovoStato;
        attivita.Completata = request.NuovoStato == StatoAttivita.Completata;
        _unitOfWork.Attivita.Update(attivita);
        await _unitOfWork.SaveChangesAsync(ct);

        return await MapToDtoAsync(attivita, cliente.RagioneSociale, ct);
    }

    private async Task<(Attivita Attivita, Cliente Cliente)> CaricaAttivitaEClienteAsync(int attivitaId, CancellationToken ct)
    {
        var attivita = await _unitOfWork.Attivita.GetByIdAsync(attivitaId, ct)
            ?? throw new NotFoundException(nameof(Attivita), attivitaId);

        var cliente = await _unitOfWork.Clienti.GetByIdAsync(attivita.ClienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), attivita.ClienteId);

        return (attivita, cliente);
    }

    private async Task<AttivitaDto> MapToDtoAsync(Attivita a, string clienteRagioneSociale, CancellationToken ct)
    {
        var utente = await _unitOfWork.Utenti.GetByIdAsync(a.UtenteId, ct)
            ?? throw new NotFoundException(nameof(Utente), a.UtenteId);

        return new AttivitaDto(
            a.Id, a.ClienteId, clienteRagioneSociale, a.UtenteId, $"{utente.Nome} {utente.Cognome}",
            a.TipoAttivita, a.Titolo, a.Descrizione, a.Priorita, a.DataScadenza, a.Completata, a.Stato);
    }
}
