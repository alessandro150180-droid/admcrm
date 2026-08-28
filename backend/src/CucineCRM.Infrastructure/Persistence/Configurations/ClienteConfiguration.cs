using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CucineCRM.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clienti");

        builder.Property(c => c.RagioneSociale).IsRequired().HasMaxLength(250);
        builder.Property(c => c.CodiceCliente).IsRequired().HasMaxLength(50);
        builder.Property(c => c.PartitaIVA).HasMaxLength(20);
        builder.Property(c => c.Provincia).HasMaxLength(50);
        builder.Property(c => c.Regione).HasMaxLength(50);
        builder.Property(c => c.CAP).HasMaxLength(10);
        builder.Property(c => c.PercentualeProvvigione).HasColumnType("decimal(5,2)").HasDefaultValue(0m);

        builder.HasIndex(c => c.CodiceCliente).IsUnique();
        // Indici usati intensamente dai filtri dashboard (regione/provincia/agente)
        builder.HasIndex(c => c.Regione);
        builder.HasIndex(c => c.Provincia);
        builder.HasIndex(c => c.AgenteId);

        builder.HasOne(c => c.Agente)
            .WithMany(a => a.Clienti)
            .HasForeignKey(c => c.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
