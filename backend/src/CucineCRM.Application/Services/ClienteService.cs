using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Entities;

namespace CucineCRM.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;
    private readonly IAsyncQueryExecutor _queryExecutor;

    public ClienteService(IUnitOfWork unitOfWork, IDataScopingService scoping, IAsyncQueryExecutor queryExecutor)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
        _queryExecutor = queryExecutor;
    }

    public async Task<PagedResult<ClienteDto>> GetListaAsync(FiltriListaDto filtri, CancellationToken ct = default)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);

        var query = _unitOfWork.Clienti.Query();

        // Scoping per ruolo: applicato SEMPRE prima di qualunque altro filtro utente,
        // così un Agente non può in nessun modo forzare la visibilità di clienti altrui via query string.
        if (agentiVisibili is not null)
            query = query.Where(c => agentiVisibili.Contains(c.AgenteId));

        if (filtri.AgenteId.HasValue)
            query = query.Where(c => c.AgenteId == filtri.AgenteId.Value);
        if (!string.IsNullOrWhiteSpace(filtri.Regione))
            query = query.Where(c => c.Regione == filtri.Regione);
        if (!string.IsNullOrWhiteSpace(filtri.Provincia))
            query = query.Where(c => c.Provincia == filtri.Provincia);

        var totale = await _queryExecutor.CountAsync(query, ct);

        var elementi = await _queryExecutor.ToListAsync(query
            .OrderBy(c => c.RagioneSociale)
            .Skip((filtri.Pagina - 1) * filtri.Dimensione)
            .Take(filtri.Dimensione)
            .Select(c => new ClienteDto(
                c.Id, c.RagioneSociale, c.CodiceCliente, c.PartitaIVA, c.Indirizzo, c.Citta,
                c.Provincia, c.Regione, c.CAP, c.Telefono, c.Email, c.NominativoTitolare, c.AgenteId,
                c.Agente.Nome + " " + c.Agente.Cognome, c.Agente.Email,
                c.DataInserimento, c.PercentualeProvvigione)), ct);

        return new PagedResult<ClienteDto>
        {
            Elementi = elementi,
            Pagina = filtri.Pagina,
            Dimensione = filtri.Dimensione,
            TotaleElementi = totale
        };
    }

    public async Task<ClienteDettaglioDto> GetDettaglioAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _unitOfWork.Clienti.GetByIdAsync(clienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), clienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non hai accesso ai dati di questo cliente.");

        var ordini = await _unitOfWork.Ordini.FindAsync(o => o.ClienteId == clienteId, ct);
        var ordiniList = ordini.ToList();

        var fatturatoTotale = ordiniList.Sum(o => o.Importo);
        var numeroOrdini = ordiniList.Count;

        var anagrafica = await MappaConAgenteAsync(cliente, ct);

        return new ClienteDettaglioDto(
            Anagrafica: anagrafica,
            NumeroOrdiniTotali: numeroOrdini,
            FatturatoTotale: fatturatoTotale,
            NumeroCucineAcquistate: ordiniList.Sum(o => o.NumeroCucine),
            NumeroElettrodomesticiAcquistati: ordiniList.Sum(o => o.NumeroElettrodomestici),
            OrdineMedio: numeroOrdini == 0 ? 0 : fatturatoTotale / numeroOrdini,
            UltimoAcquisto: ordiniList.Count == 0 ? null : ordiniList.Max(o => o.DataOrdine)
        );
    }

    public async Task<ClienteDto> CreaAsync(CreaClienteDto request, CancellationToken ct = default)
    {
        if (!await _scoping.PuoAccedereAdAgenteAsync(request.AgenteId, ct))
            throw new ForbiddenAccessException("Non puoi creare clienti per un agente che non gestisci.");

        var codiceEsistente = (await _unitOfWork.Clienti.FindAsync(c => c.CodiceCliente == request.CodiceCliente, ct)).Any();
        if (codiceEsistente)
            throw new ValidationAppException($"Codice cliente '{request.CodiceCliente}' già esistente.");

        var cliente = new Cliente
        {
            RagioneSociale = request.RagioneSociale,
            CodiceCliente = request.CodiceCliente,
            PartitaIVA = request.PartitaIVA,
            Indirizzo = request.Indirizzo,
            Citta = request.Citta,
            Provincia = request.Provincia,
            Regione = request.Regione,
            CAP = request.CAP,
            Telefono = request.Telefono,
            Email = request.Email,
            NominativoTitolare = request.NominativoTitolare,
            AgenteId = request.AgenteId,
            DataInserimento = DateTime.UtcNow,
            PercentualeProvvigione = request.PercentualeProvvigione
        };

        await _unitOfWork.Clienti.AddAsync(cliente, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await MappaConAgenteAsync(cliente, ct);
    }

    public async Task<ClienteDto> ImpostaProvvigioneAsync(int clienteId, ImpostaProvvigioneDto request, CancellationToken ct = default)
    {
        if (request.PercentualeProvvigione < 0 || request.PercentualeProvvigione > 100)
            throw new ValidationAppException("La percentuale di provvigione deve essere compresa tra 0 e 100.");

        var cliente = await _unitOfWork.Clienti.GetByIdAsync(clienteId, ct)
            ?? throw new NotFoundException(nameof(Cliente), clienteId);

        if (!await _scoping.PuoAccedereAdAgenteAsync(cliente.AgenteId, ct))
            throw new ForbiddenAccessException("Non puoi modificare la provvigione di un cliente che non gestisci.");

        cliente.PercentualeProvvigione = request.PercentualeProvvigione;
        _unitOfWork.Clienti.Update(cliente);
        await _unitOfWork.SaveChangesAsync(ct);

        return await MappaConAgenteAsync(cliente, ct);
    }

    /// <summary>
    /// Mappa un Cliente già caricato in DTO risolvendo nome ed email dell'agente. Sulle liste la
    /// proiezione LINQ fa lo stesso lavoro lato database; qui l'agente va invece caricato a parte
    /// perché il repository restituisce l'entità senza la navigazione Agente popolata.
    /// </summary>
    private async Task<ClienteDto> MappaConAgenteAsync(Cliente cliente, CancellationToken ct)
    {
        var agente = await _unitOfWork.Agenti.GetByIdAsync(cliente.AgenteId, ct);

        return new ClienteDto(
            cliente.Id, cliente.RagioneSociale, cliente.CodiceCliente, cliente.PartitaIVA,
            cliente.Indirizzo, cliente.Citta, cliente.Provincia, cliente.Regione, cliente.CAP,
            cliente.Telefono, cliente.Email, cliente.NominativoTitolare, cliente.AgenteId,
            agente is null ? string.Empty : $"{agente.Nome} {agente.Cognome}",
            agente?.Email ?? string.Empty,
            cliente.DataInserimento, cliente.PercentualeProvvigione);
    }
}
