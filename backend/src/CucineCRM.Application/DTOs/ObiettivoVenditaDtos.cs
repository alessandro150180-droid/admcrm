namespace CucineCRM.Application.DTOs;

public record ObiettivoVenditaDto(
    int Id,
    int AgenteId,
    string AgenteNomeCompleto,
    int Mese,
    int Anno,
    decimal ObiettivoFatturato,
    int ObiettivoCucine,
    decimal FatturatoRealizzato,
    decimal PercentualeRaggiungimento
);

/// <summary>Imposta l'obiettivo per agente/mese/anno: se esiste già, viene aggiornato (upsert).</summary>
public record ImpostaObiettivoDto(
    int AgenteId,
    int Mese,
    int Anno,
    decimal ObiettivoFatturato,
    int ObiettivoCucine
);
