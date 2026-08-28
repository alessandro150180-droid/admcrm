using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class ObiettivoVenditaService : IObiettivoVenditaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;
    private readonly IAsyncQueryExecutor _queryExecutor;

    public ObiettivoVenditaService(IUnitOfWork unitOfWork, IDataScopingService scoping, IAsyncQueryExecutor queryExecutor)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
        _queryExecutor = queryExecutor;
    }

    public async Task<IReadOnlyList<ObiettivoVenditaDto>> GetListaAsync(int anno, int? agenteId, CancellationToken ct = default)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);

        var query = _unitOfWork.ObiettiviVendita.Query().Where(o => o.Anno == anno);
        if (agentiVisibili is not null)
            query = query.Where(o => agentiVisibili.Contains(o.AgenteId));
        if (agenteId.HasValue)
            query = query.Where(o => o.AgenteId == agenteId.Value);

        var obiettivi = await _queryExecutor.ToListAsync(query
            .OrderBy(o => o.AgenteId).ThenBy(o => o.Mese)
            .Select(o => new
            {
                o.Id, o.AgenteId, AgenteNomeCompleto = o.Agente.Nome + " " + o.Agente.Cognome,
                o.Mese, o.Anno, o.ObiettivoFatturato, o.ObiettivoCucine
            }), ct);

        if (obiettivi.Count == 0)
            return Array.Empty<ObiettivoVenditaDto>();

        var agentiCoinvolti = obiettivi.Select(o => o.AgenteId).Distinct().ToList();

        // Un'unica query aggregata per il fatturato realizzato di tutti gli agenti/mesi coinvolti,
        // invece di interrogare il DB una volta per ogni riga di obiettivo.
        var fatturatiPerAgenteMese = await _queryExecutor.ToListAsync(_unitOfWork.Ordini.Query()
            .Where(o => o.DataOrdine.Year == anno && agentiCoinvolti.Contains(o.Cliente.AgenteId))
            .GroupBy(o => new { o.Cliente.AgenteId, o.DataOrdine.Month })
            .Select(g => new { g.Key.AgenteId, Mese = g.Key.Month, Totale = g.Sum(x => x.Importo) }), ct);

        var fatturatiLookup = fatturatiPerAgenteMese.ToDictionary(f => (f.AgenteId, f.Mese), f => f.Totale);

        return obiettivi.Select(o =>
        {
            var fatturatoRealizzato = fatturatiLookup.GetValueOrDefault((o.AgenteId, o.Mese), 0m);
            var percentuale = o.ObiettivoFatturato == 0 ? 0 : Math.Round(fatturatoRealizzato / o.ObiettivoFatturato * 100, 1);
            return new ObiettivoVenditaDto(
                o.Id, o.AgenteId, o.AgenteNomeCompleto, o.Mese, o.Anno,
                o.ObiettivoFatturato, o.ObiettivoCucine, fatturatoRealizzato, percentuale);
        }).ToList();
    }

    public async Task<ObiettivoVenditaDto> ImpostaAsync(ImpostaObiettivoDto request, CancellationToken ct = default)
    {
        if (request.Mese is < 1 or > 12)
            throw new ValidationAppException("Il mese deve essere compreso tra 1 e 12.");

        var agente = await _unitOfWork.Agenti.GetByIdAsync(request.AgenteId, ct)
            ?? throw new NotFoundException(nameof(Agente), request.AgenteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(request.AgenteId, ct))
            throw new ForbiddenAccessException("Non puoi impostare obiettivi per un agente che non gestisci.");

        var esistente = (await _unitOfWork.ObiettiviVendita.FindAsync(
            o => o.AgenteId == request.AgenteId && o.Mese == request.Mese && o.Anno == request.Anno, ct)).FirstOrDefault();

        ObiettivoVendita obiettivo;
        if (esistente is not null)
        {
            esistente.ObiettivoFatturato = request.ObiettivoFatturato;
            esistente.ObiettivoCucine = request.ObiettivoCucine;
            _unitOfWork.ObiettiviVendita.Update(esistente);
            obiettivo = esistente;
        }
        else
        {
            obiettivo = new ObiettivoVendita
            {
                AgenteId = request.AgenteId,
                Mese = request.Mese,
                Anno = request.Anno,
                ObiettivoFatturato = request.ObiettivoFatturato,
                ObiettivoCucine = request.ObiettivoCucine
            };
            await _unitOfWork.ObiettiviVendita.AddAsync(obiettivo, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var fatturatoRealizzato = await _queryExecutor.SumAsync(_unitOfWork.Ordini.Query()
            .Where(o => o.Cliente.AgenteId == request.AgenteId && o.DataOrdine.Month == request.Mese && o.DataOrdine.Year == request.Anno)
            .Select(o => o.Importo), ct);

        var percentuale = obiettivo.ObiettivoFatturato == 0 ? 0 : Math.Round(fatturatoRealizzato / obiettivo.ObiettivoFatturato * 100, 1);

        return new ObiettivoVenditaDto(
            obiettivo.Id, obiettivo.AgenteId, $"{agente.Nome} {agente.Cognome}", obiettivo.Mese, obiettivo.Anno,
            obiettivo.ObiettivoFatturato, obiettivo.ObiettivoCucine, fatturatoRealizzato, percentuale);
    }
}
