namespace CucineCRM.Application.DTOs;

public record ClienteDto(
    int Id,
    string RagioneSociale,
    string CodiceCliente,
    string? PartitaIVA,
    string? Indirizzo,
    string? Citta,
    string? Provincia,
    string? Regione,
    string? CAP,
    string? Telefono,
    string? Email,
    int AgenteId,
    string AgenteNomeCompleto,
    string AgenteEmail,
    DateTime DataInserimento,
    decimal PercentualeProvvigione
);

public record CreaClienteDto(
    string RagioneSociale,
    string CodiceCliente,
    string? PartitaIVA,
    string? Indirizzo,
    string? Citta,
    string? Provincia,
    string? Regione,
    string? CAP,
    string? Telefono,
    string? Email,
    int AgenteId,
    decimal PercentualeProvvigione = 0
);

public record ImpostaProvvigioneDto(decimal PercentualeProvvigione);

public record ClienteDettaglioDto(
    ClienteDto Anagrafica,
    int NumeroOrdiniTotali,
    decimal FatturatoTotale,
    int NumeroCucineAcquistate,
    int NumeroElettrodomesticiAcquistati,
    decimal OrdineMedio,
    DateTime? UltimoAcquisto
);

/// <summary>Parametri di paginazione/filtro comuni a più liste (Clienti, Ordini, Attivita...).</summary>
public record FiltriListaDto(
    int Pagina = 1,
    int Dimensione = 20,
    string? Regione = null,
    string? Provincia = null,
    int? AgenteId = null,
    int? Anno = null,
    int? Mese = null
);
