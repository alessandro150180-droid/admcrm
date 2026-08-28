using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.DTOs;

public record AttivitaDto(
    int Id,
    int ClienteId,
    string ClienteRagioneSociale,
    int UtenteId,
    string UtenteNomeCompleto,
    TipoAttivita TipoAttivita,
    string Titolo,
    string? Descrizione,
    PrioritaAttivita Priorita,
    DateTime DataScadenza,
    bool Completata,
    StatoAttivita Stato
);

public record CreaAttivitaDto(
    int ClienteId,
    TipoAttivita TipoAttivita,
    string Titolo,
    string? Descrizione,
    PrioritaAttivita Priorita,
    DateTime DataScadenza
);

public record AggiornaStatoAttivitaDto(StatoAttivita NuovoStato);

/// <summary>Parametri di filtro per la lista Attività, oltre a quelli comuni in FiltriListaDto (Mese/Anno = scadenza).</summary>
public record FiltriAttivitaDto(
    int Pagina = 1,
    int Dimensione = 20,
    int? AgenteId = null,
    StatoAttivita? Stato = null,
    bool? SoloScadute = null
);
