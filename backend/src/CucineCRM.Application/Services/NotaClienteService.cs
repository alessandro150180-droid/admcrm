using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class NotaClienteService : INotaClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ICurrentUserService _currentUser;

    public NotaClienteService(
        IUnitOfWork unitOfWork, IDataScopingService scoping, IAsyncQueryExecutor queryExecutor, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
        _queryExecutor = queryExecutor;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotaClienteDto>> GetPerClienteAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _unitOfWork.Clienti.GetByIdAsync(clienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), clienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non hai accesso alle note di questo cliente.");

        var note = await _queryExecutor.ToListAsync(_unitOfWork.NoteCliente.Query()
            .Where(n => n.ClienteId == clienteId)
            .OrderByDescending(n => n.DataInserimento)
            .Select(n => new NotaClienteDto(
                n.Id, n.ClienteId, n.UtenteId, n.Utente.Nome + " " + n.Utente.Cognome, n.Testo, n.DataInserimento)), ct);

        return note;
    }

    public async Task<NotaClienteDto> CreaAsync(CreaNotaClienteDto request, CancellationToken ct = default)
    {
        var cliente = await _unitOfWork.Clienti.GetByIdAsync(request.ClienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), request.ClienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non puoi aggiungere note a un cliente che non gestisci.");

        if (string.IsNullOrWhiteSpace(request.Testo))
            throw new ValidationAppException("Il testo della nota non può essere vuoto.");

        var utenteId = _currentUser.UtenteId
            ?? throw new AuthenticationException("Utente non autenticato.");

        var nota = new NotaCliente
        {
            ClienteId = request.ClienteId,
            UtenteId = utenteId,
            Testo = request.Testo,
            DataInserimento = DateTime.UtcNow
        };

        await _unitOfWork.NoteCliente.AddAsync(nota, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var utente = await _unitOfWork.Utenti.GetByIdAsync(utenteId, ct)
            ?? throw new NotFoundException(nameof(Utente), utenteId);

        return new NotaClienteDto(nota.Id, nota.ClienteId, nota.UtenteId, $"{utente.Nome} {utente.Cognome}", nota.Testo, nota.DataInserimento);
    }
}
