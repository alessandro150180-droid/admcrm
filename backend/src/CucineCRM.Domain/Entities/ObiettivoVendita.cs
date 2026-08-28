using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class ObiettivoVendita : BaseEntity
{
    public int AgenteId { get; set; }
    public Agente Agente { get; set; } = null!;

    public int Mese { get; set; } // 1-12
    public int Anno { get; set; }
    public decimal ObiettivoFatturato { get; set; }
    public int ObiettivoCucine { get; set; }
}
