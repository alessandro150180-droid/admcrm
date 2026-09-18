namespace CucineCRM.Application.DTOs;

public record FornitoreDto(int Id, string Nome, bool Attivo);

public record CreaFornitoreDto(string Nome);
