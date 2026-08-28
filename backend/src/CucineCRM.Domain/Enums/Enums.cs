namespace CucineCRM.Domain.Enums;

public enum RuoloUtente
{
    Amministratore = 0,
    DirettoreCommerciale = 1,
    AreaManager = 2,
    Agente = 3
}

public enum StatoOrdine
{
    InAttesa = 0,
    Confermato = 1,
    InProduzione = 2,
    Spedito = 3,
    Consegnato = 4,
    Annullato = 5
}

public enum TipoAttivita
{
    Telefonata = 0,
    Visita = 1,
    Preventivo = 2,
    FollowUp = 3,
    Reclamo = 4,
    Campionario = 5,
    Email = 6,
    Assistenza = 7
}

public enum PrioritaAttivita
{
    Bassa = 0,
    Media = 1,
    Alta = 2,
    Urgente = 3
}

public enum StatoAttivita
{
    DaFare = 0,
    InCorso = 1,
    Completata = 2,
    Annullata = 3
}
