using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CucineCRM.Infrastructure.Persistence.Configurations;

public class OrdineConfiguration : IEntityTypeConfiguration<Ordine>
{
    public void Configure(EntityTypeBuilder<Ordine> builder)
    {
        builder.ToTable("Ordini");

        builder.Property(o => o.Importo).HasColumnType("decimal(14,2)");
        builder.Property(o => o.StatoOrdine).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.RiferimentoEsterno).HasMaxLength(100);

        // Fondamentale per l'import Excel: evita duplicati sullo stesso riferimento esterno
        builder.HasIndex(o => o.RiferimentoEsterno).IsUnique().HasFilter("\"RiferimentoEsterno\" IS NOT NULL");

        // Indici usati dalle dashboard per aggregazioni per data/cliente
        builder.HasIndex(o => o.DataOrdine);
        builder.HasIndex(o => o.ClienteId);

        builder.HasOne(o => o.Cliente)
            .WithMany(c => c.Ordini)
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Importazione)
            .WithMany(i => i.Ordini)
            .HasForeignKey(o => o.ImportazioneId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
