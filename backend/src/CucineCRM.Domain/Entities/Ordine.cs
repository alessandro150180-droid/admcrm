using CucineCRM.Domain.Common;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Domain.Entities;

public class Ordine : BaseEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime DataOrdine { get; set; }
    public decimal Importo { get; set; }
    public int NumeroCucine { get; set; }
    public int NumeroElettrodomestici { get; set; }
    public int NumeroComplementi { get; set; }
    public StatoOrdine StatoOrdine { get; set; }

    // Collega l'ordine all'importazione Excel che lo ha generato/aggiornato (nullable per ordini manuali)
    public int? ImportazioneId { get; set; }
    public Importazione? Importazione { get; set; }

    // Chiave naturale usata in fase di import per riconoscere i duplicati
    // (es. concatenazione CodiceCliente + numero ordine gestionale esterno)
    public string? RiferimentoEsterno { get; set; }
}
