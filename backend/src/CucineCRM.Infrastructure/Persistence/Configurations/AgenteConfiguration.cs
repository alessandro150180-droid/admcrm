using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CucineCRM.Infrastructure.Persistence.Configurations;

public class AgenteConfiguration : IEntityTypeConfiguration<Agente>
{
    public void Configure(EntityTypeBuilder<Agente> builder)
    {
        builder.ToTable("Agenti");

        builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Cognome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Zona).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(256);

        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasIndex(a => a.Zona);

        // Auto-relazione: un Area Manager (Agente) gestisce N agenti
        builder.HasOne(a => a.AreaManager)
            .WithMany(a => a.AgentiGestiti)
            .HasForeignKey(a => a.AreaManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
