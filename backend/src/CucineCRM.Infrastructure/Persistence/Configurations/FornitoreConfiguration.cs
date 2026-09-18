using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CucineCRM.Infrastructure.Persistence.Configurations;

public class FornitoreConfiguration : IEntityTypeConfiguration<Fornitore>
{
    public void Configure(EntityTypeBuilder<Fornitore> builder)
    {
        builder.ToTable("Fornitori");

        builder.Property(f => f.Nome).IsRequired().HasMaxLength(100);

        // Univoco solo tra i fornitori attivi (non eliminati): coerente con lo stesso pattern
        // già usato sull'indice di Ordine.RiferimentoEsterno.
        builder.HasIndex(f => f.Nome).IsUnique().HasFilter("\"Eliminato\" = false");
    }
}
