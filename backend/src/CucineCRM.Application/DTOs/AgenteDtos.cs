namespace CucineCRM.Application.DTOs;

public record AgenteDto(int Id, string Nome, string Cognome, string Zona, string? Telefono, string Email, int? AreaManagerId);

public record CreaAgenteDto(string Nome, string Cognome, string Zona, string? Telefono, string Email, int? AreaManagerId);
