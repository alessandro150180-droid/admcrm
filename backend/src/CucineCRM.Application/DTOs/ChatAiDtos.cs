namespace CucineCRM.Application.DTOs;

/// <summary>Un turno della conversazione così come lo mantiene il frontend (stateless lato server:
/// nessuna Entity/tabella dedicata, la cronologia viaggia per intero ad ogni richiesta).</summary>
public record ChatAiMessaggioDto(string Ruolo, string Testo); // Ruolo: "utente" | "assistente"

public record ChatAiRichiestaDto(string Testo, IReadOnlyList<ChatAiMessaggioDto> Cronologia);

public record ChatAiRispostaDto(string Testo);
