using CucineCRM.Domain.Common;

namespace CucineCRM.Domain.Entities;

public class NotaCliente : BaseEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int UtenteId { get; set; }
    public Utente Utente { get; set; } = null!;

    public string Testo { get; set; } = string.Empty;
    public DateTime DataInserimento { get; set; }
}
