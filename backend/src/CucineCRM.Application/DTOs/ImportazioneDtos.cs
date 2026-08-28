namespace CucineCRM.Application.DTOs;

public record ImportazioneRisultatoDto(
    int Id,
    string NomeFile,
    DateTime DataImportazione,
    string PeriodoCompetenza,
    int RighePlesse,
    int RigheImportate,
    int RigheScartate,
    int RigheDuplicate,
    bool Completata,
    string? LogEsito
);

/// <summary>Esito dell'elaborazione di una singola riga del file importato (usato per LogEsito).</summary>
public record RigaImportLogDto(int NumeroRiga, string Esito, string? Messaggio);
