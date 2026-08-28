using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IDashboardService
{
    /// <summary>I 4 KPI principali della Home, con confronto anno su anno, per i mesi/anno indicati
    /// (uno o più mesi: se ne indichi più di uno, i valori sono la somma sull'insieme di mesi selezionato).</summary>
    Task<DashboardKpiDto> GetKpiPrincipaliAsync(IReadOnlyList<int> mesi, int anno, int? agenteId = null, CancellationToken ct = default);

    /// <summary>Andamento fatturato mese per mese, per il confronto 2025 vs 2026 (grafico a colonne).</summary>
    Task<IReadOnlyList<PuntoGraficoMensileDto>> GetFatturatoMensileAsync(int anno, int? agenteId = null, CancellationToken ct = default);

    /// <summary>Fatturato e provvigione per cliente (portafoglio di un agente, o singolo cliente) nei mesi/anno indicati.</summary>
    Task<IReadOnlyList<ProvvigioneClienteDto>> GetProvvigioniPerClienteAsync(
        IReadOnlyList<int> mesi, int anno, int? agenteId = null, int? clienteId = null, CancellationToken ct = default);
}
