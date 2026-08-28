using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Application.Services;
using CucineCRM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CucineCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TuttiIRuoli")]
public class AgentiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataScopingService _scoping;

    public AgentiController(IUnitOfWork unitOfWork, IDataScopingService scoping)
    {
        _unitOfWork = unitOfWork;
        _scoping = scoping;
    }

    [HttpGet]
    public async Task<IActionResult> GetAgenti(CancellationToken ct)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);

        var query = _unitOfWork.Agenti.Query();
        if (agentiVisibili is not null) // null = nessun filtro (Amministratore/Direttore)
            query = query.Where(a => agentiVisibili.Contains(a.Id));

        var risultato = await query
            .Select(a => new AgenteDto(a.Id, a.Nome, a.Cognome, a.Zona, a.Telefono, a.Email, a.AreaManagerId))
            .ToListAsync(ct);

        return Ok(risultato);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAgente(int id, CancellationToken ct)
    {
        if (!await _scoping.PuoAccedereAdAgenteAsync(id, ct))
            return Forbid();

        var agente = await _unitOfWork.Agenti.GetByIdAsync(id, ct);
        return agente is null ? NotFound() : Ok(new AgenteDto(agente.Id, agente.Nome, agente.Cognome, agente.Zona, agente.Telefono, agente.Email, agente.AreaManagerId));
    }

    /// <summary>Crea un nuovo agente della rete vendita.</summary>
    [HttpPost]
    [Authorize(Policy = "SoloDirezione")]
    public async Task<IActionResult> CreaAgente([FromBody] CreaAgenteDto request, CancellationToken ct)
    {
        var emailEsistente = (await _unitOfWork.Agenti.FindAsync(a => a.Email == request.Email, ct)).Any();
        if (emailEsistente)
            throw new ValidationAppException($"Esiste già un agente con email '{request.Email}'.");

        var agente = new Agente
        {
            Nome = request.Nome,
            Cognome = request.Cognome,
            Zona = request.Zona,
            Telefono = request.Telefono,
            Email = request.Email,
            AreaManagerId = request.AreaManagerId
        };

        await _unitOfWork.Agenti.AddAsync(agente, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new AgenteDto(agente.Id, agente.Nome, agente.Cognome, agente.Zona, agente.Telefono, agente.Email, agente.AreaManagerId);
        return CreatedAtAction(nameof(GetAgente), new { id = agente.Id }, dto);
    }

    /// <summary>
    /// Elimina (soft-delete) un agente e a cascata tutti i dati a lui collegati: i suoi clienti e,
    /// per ciascuno, ordini, attività, note e appuntamenti di calendario, oltre agli eventuali
    /// obiettivi di vendita dell'agente. I record restano nel database (Eliminato = true, storico e
    /// audit log preservati) ma non compaiono più in nessuna lista/dashboard.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SoloDirezione")]
    public async Task<IActionResult> EliminaAgente(int id, CancellationToken ct)
    {
        var agente = await _unitOfWork.Agenti.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Agente), id);

        var clienti = await _unitOfWork.Clienti.FindAsync(c => c.AgenteId == id, ct);
        foreach (var cliente in clienti)
        {
            foreach (var ordine in await _unitOfWork.Ordini.FindAsync(o => o.ClienteId == cliente.Id, ct))
                _unitOfWork.Ordini.SoftDelete(ordine);

            foreach (var attivita in await _unitOfWork.Attivita.FindAsync(a => a.ClienteId == cliente.Id, ct))
                _unitOfWork.Attivita.SoftDelete(attivita);

            foreach (var nota in await _unitOfWork.NoteCliente.FindAsync(n => n.ClienteId == cliente.Id, ct))
                _unitOfWork.NoteCliente.SoftDelete(nota);

            foreach (var evento in await _unitOfWork.Calendario.FindAsync(e => e.ClienteId == cliente.Id, ct))
                _unitOfWork.Calendario.SoftDelete(evento);

            _unitOfWork.Clienti.SoftDelete(cliente);
        }

        foreach (var obiettivo in await _unitOfWork.ObiettiviVendita.FindAsync(o => o.AgenteId == id, ct))
            _unitOfWork.ObiettiviVendita.SoftDelete(obiettivo);

        _unitOfWork.Agenti.SoftDelete(agente);
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
