using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.DTOs;

public record OrdineDto(
    int Id,
    int ClienteId,
    string ClienteRagioneSociale,
    DateTime DataOrdine,
    decimal Importo,
    int NumeroCucine,
    int NumeroElettrodomestici,
    int NumeroComplementi,
    StatoOrdine StatoOrdine,
    string? RiferimentoEsterno
);

public record CreaOrdineDto(
    int ClienteId,
    DateTime DataOrdine,
    decimal Importo,
    int NumeroCucine,
    int NumeroElettrodomestici,
    int NumeroComplementi,
    string? RiferimentoEsterno
);

public record AggiornaStatoOrdineDto(StatoOrdine NuovoStato);
