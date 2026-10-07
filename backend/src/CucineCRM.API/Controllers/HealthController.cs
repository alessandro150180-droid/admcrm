using CucineCRM.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CucineCRM.API.Controllers;

/// <summary>
/// Endpoint pubblico (nessuna autenticazione) usato da un job schedulato esterno per tenere
/// "attivo" il database: i progetti Supabase sul piano gratuito vengono messi in pausa dopo
/// circa una settimana senza query. GetAllAsync forza una vera query al database, non basta
/// rispondere 200 senza toccarlo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public HealthController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var numeroFornitori = await _unitOfWork.Fornitori.CountAsync(ct: ct);
        return Ok(new { ok = true, timestampUtc = DateTime.UtcNow, numeroFornitori });
    }
}
