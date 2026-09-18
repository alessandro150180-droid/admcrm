using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.DTOs;

public record OrdineDto(
    int Id,
    int ClienteId,
    string ClienteRagioneSociale,
    int FornitoreId,
    string FornitoreNome,
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
    int FornitoreId,
    DateTime DataOrdine,
    decimal Importo,
    int NumeroCucine,
    int NumeroElettrodomestici,
    int NumeroComplementi,
    string? RiferimentoEsterno
);

public record AggiornaStatoOrdineDto(StatoOrdine NuovoStato);
