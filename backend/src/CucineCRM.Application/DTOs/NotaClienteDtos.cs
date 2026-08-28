namespace CucineCRM.Application.DTOs;

public record NotaClienteDto(
    int Id,
    int ClienteId,
    int UtenteId,
    string UtenteNomeCompleto,
    string Testo,
    DateTime DataInserimento
);

public record CreaNotaClienteDto(
    int ClienteId,
    string Testo
);

/// <summary>Corpo della richiesta POST /api/clienti/{id}/note: il ClienteId arriva dalla route.</summary>
public record AggiungiNotaDto(string Testo);
