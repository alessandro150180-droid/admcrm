using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class FornitoreService : IFornitoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;

    public FornitoreService(IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor)
    {
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
    }

    // Nessuno scoping per ruolo: i fornitori sono un elenco globale (non legato ad agente/cliente),
    // visibile a tutti i ruoli autenticati allo stesso modo.
    public async Task<IReadOnlyList<FornitoreDto>> GetListaAsync(CancellationToken ct = default) =>
        await _queryExecutor.ToListAsync(_unitOfWork.Fornitori.Query()
            .OrderBy(f => f.Nome)
            .Select(f => new FornitoreDto(f.Id, f.Nome, f.Attivo)), ct);

    public async Task<FornitoreDto> CreaAsync(CreaFornitoreDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new ValidationAppException("Il nome del fornitore è obbligatorio.");

        var esistente = (await _unitOfWork.Fornitori.FindAsync(f => f.Nome == request.Nome.Trim(), ct)).Any();
        if (esistente)
            throw new ValidationAppException($"Esiste già un fornitore '{request.Nome}'.");

        var fornitore = new Fornitore { Nome = request.Nome.Trim(), Attivo = true };
        await _unitOfWork.Fornitori.AddAsync(fornitore, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new FornitoreDto(fornitore.Id, fornitore.Nome, fornitore.Attivo);
    }
}
