using CucineCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CucineCRM.Infrastructure.Persistence.Configurations;

public class ImportazioneConfiguration : IEntityTypeConfiguration<Importazione>
{
    public void Configure(EntityTypeBuilder<Importazione> builder)
    {
        builder.ToTable("Importazioni");
        builder.Property(i => i.NomeFile).IsRequired().HasMaxLength(260);
        builder.Property(i => i.PeriodoCompetenza).IsRequired().HasMaxLength(7); // "yyyy-MM"

        builder.HasOne(i => i.UtenteImportazione)
            .WithMany(u => u.Importazioni)
            .HasForeignKey(i => i.UtenteImportazioneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.DataImportazione);
        builder.HasIndex(i => i.PeriodoCompetenza);
    }
}

public class AttivitaConfiguration : IEntityTypeConfiguration<Attivita>
{
    public void Configure(EntityTypeBuilder<Attivita> builder)
    {
        builder.ToTable("Attivita");
        builder.Property(a => a.Titolo).IsRequired().HasMaxLength(200);
        builder.Property(a => a.TipoAttivita).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Priorita).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Stato).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Cliente)
            .WithMany(c => c.Attivita)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Utente)
            .WithMany(u => u.Attivita)
            .HasForeignKey(a => a.UtenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.DataScadenza);
        builder.HasIndex(a => new { a.Completata, a.DataScadenza }); // per query "attività scadute"
    }
}

public class NotaClienteConfiguration : IEntityTypeConfiguration<NotaCliente>
{
    public void Configure(EntityTypeBuilder<NotaCliente> builder)
    {
        builder.ToTable("NoteCliente");
        builder.Property(n => n.Testo).IsRequired();

        builder.HasOne(n => n.Cliente)
            .WithMany(c => c.Note)
            .HasForeignKey(n => n.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Utente)
            .WithMany(u => u.Note)
            .HasForeignKey(n => n.UtenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CalendarioConfiguration : IEntityTypeConfiguration<Calendario>
{
    public void Configure(EntityTypeBuilder<Calendario> builder)
    {
        builder.ToTable("Calendario");
        builder.Property(c => c.GoogleEventId).HasMaxLength(200);

        builder.HasOne(c => c.Cliente)
            .WithMany(cl => cl.EventiCalendario)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Attivita)
            .WithOne(a => a.EventoCalendario)
            .HasForeignKey<Calendario>(c => c.AttivitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.DataEvento);
    }
}

public class ObiettivoVenditaConfiguration : IEntityTypeConfiguration<ObiettivoVendita>
{
    public void Configure(EntityTypeBuilder<ObiettivoVendita> builder)
    {
        builder.ToTable("ObiettiviVendita");
        builder.Property(o => o.ObiettivoFatturato).HasColumnType("decimal(14,2)");

        builder.HasOne(o => o.Agente)
            .WithMany(a => a.Obiettivi)
            .HasForeignKey(o => o.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un solo obiettivo per agente/mese/anno
        builder.HasIndex(o => new { o.AgenteId, o.Mese, o.Anno }).IsUnique();
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.Property(a => a.NomeEntita).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Azione).IsRequired().HasMaxLength(30);

        builder.HasOne(a => a.Utente)
            .WithMany()
            .HasForeignKey(a => a.UtenteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Query tipiche: storico di una specifica entità, oppure ultimi eventi in ordine cronologico.
        builder.HasIndex(a => new { a.NomeEntita, a.EntitaId });
        builder.HasIndex(a => a.DataCreazione);
    }
}

public class NotificaConfiguration : IEntityTypeConfiguration<Notifica>
{
    public void Configure(EntityTypeBuilder<Notifica> builder)
    {
        builder.ToTable("Notifiche");
        builder.Property(n => n.Tipo).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Titolo).IsRequired().HasMaxLength(200);

        builder.HasOne(n => n.Utente)
            .WithMany()
            .HasForeignKey(n => n.UtenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query tipica: notifiche non lette di un utente, più recenti per prime.
        builder.HasIndex(n => new { n.UtenteId, n.Letta });
    }
}

public class StoricoKPIConfiguration : IEntityTypeConfiguration<StoricoKPI>
{
    public void Configure(EntityTypeBuilder<StoricoKPI> builder)
    {
        builder.ToTable("StoricoKPI");
        builder.Property(s => s.Fatturato).HasColumnType("decimal(14,2)");
        builder.Property(s => s.OrdineMedio).HasColumnType("decimal(14,2)");

        builder.HasOne(s => s.Agente)
            .WithMany()
            .HasForeignKey(s => s.AgenteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Un solo aggregato per mese/anno (a livello azienda: AgenteId = null) oppure per mese/anno/agente
        builder.HasIndex(s => new { s.Mese, s.Anno, s.AgenteId }).IsUnique();
    }
}
