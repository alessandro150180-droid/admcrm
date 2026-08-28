namespace CucineCRM.Application.DTOs;

public record AuditLogDto(
    int Id,
    int? UtenteId,
    string? UtenteNomeCompleto,
    string NomeEntita,
    int EntitaId,
    string Azione,
    DateTime DataCreazione
);

public record FiltriAuditLogDto(
    int Pagina = 1,
    int Dimensione = 50,
    string? NomeEntita = null,
    int? EntitaId = null,
    int? UtenteId = null
);
