namespace CucineCRM.Application.DTOs;

public record KpiDto(
    decimal ValoreCorrente,
    decimal ValoreAnnoPrecedente,
    decimal DifferenzaPercentuale,
    bool TrendPositivo
);

public record DashboardKpiDto(
    KpiDto FatturatoMensile,
    KpiDto NuoviClienti,
    KpiDto OrdineMedio,
    KpiDto CucineVendute
);

public record PuntoGraficoMensileDto(int Mese, int Anno, decimal Valore);

/// <summary>Fatturato e provvigione di un cliente per un dato mese/anno (vista "portafoglio agente").</summary>
public record ProvvigioneClienteDto(
    int ClienteId,
    string RagioneSociale,
    int AgenteId,
    string AgenteNomeCompleto,
    decimal Fatturato,
    decimal PercentualeProvvigione,
    decimal ImportoProvvigione
);
