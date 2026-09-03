using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IDashboardService
{
    /// <summary>I 4 KPI principali della Home, con confronto anno su anno, per i mesi/anno indicati
    /// (uno o più mesi: se ne indichi più di uno, i valori sono la somma sull'insieme di mesi selezionato).
    /// Se <paramref name="clienteId"/> è specificato, i valori sono ristretti al solo fatturato di quel cliente.</summary>
    Task<DashboardKpiDto> GetKpiPrincipaliAsync(IReadOnlyList<int> mesi, int anno, int? agenteId = null, int? clienteId = null, CancellationToken ct = default);

    /// <summary>Andamento fatturato mese per mese per l'anno indicato e i due precedenti, per il
    /// confronto anno su anno nel grafico a colonne. Se <paramref name="clienteId"/> è specificato,
    /// la serie riporta solo il fatturato di quel cliente.</summary>
    Task<IReadOnlyList<PuntoGraficoMensileDto>> GetFatturatoMensileAsync(int anno, int? agenteId = null, int? clienteId = null, CancellationToken ct = default);

    /// <summary>Fatturato e provvigione per cliente (portafoglio di un agente, o singolo cliente) nei mesi/anno indicati.</summary>
    Task<IReadOnlyList<ProvvigioneClienteDto>> GetProvvigioniPerClienteAsync(
        IReadOnlyList<int> mesi, int anno, int? agenteId = null, int? clienteId = null, CancellationToken ct = default);
}
