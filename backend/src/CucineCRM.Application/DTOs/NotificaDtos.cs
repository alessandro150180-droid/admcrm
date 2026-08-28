namespace CucineCRM.Application.DTOs;

public record NotificaDto(
    int Id,
    string Tipo,
    string Titolo,
    string? Messaggio,
    int? RiferimentoEntitaId,
    bool Letta,
    DateTime DataCreazione
);
