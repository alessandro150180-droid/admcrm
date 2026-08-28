using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CucineCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Zona = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AreaManagerId = table.Column<int>(type: "integer", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agenti_Agenti_AreaManagerId",
                        column: x => x.AreaManagerId,
                        principalTable: "Agenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RagioneSociale = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CodiceCliente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartitaIVA = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Indirizzo = table.Column<string>(type: "text", nullable: true),
                    Citta = table.Column<string>(type: "text", nullable: true),
                    Provincia = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Regione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CAP = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    AgenteId = table.Column<int>(type: "integer", nullable: false),
                    DataInserimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clienti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clienti_Agenti_AgenteId",
                        column: x => x.AgenteId,
                        principalTable: "Agenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ObiettiviVendita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgenteId = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    ObiettivoFatturato = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    ObiettivoCucine = table.Column<int>(type: "integer", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObiettiviVendita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObiettiviVendita_Agenti_AgenteId",
                        column: x => x.AgenteId,
                        principalTable: "Agenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoricoKPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mese = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Fatturato = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    CucineVendute = table.Column<int>(type: "integer", nullable: false),
                    OrdineMedio = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    NuoviClienti = table.Column<int>(type: "integer", nullable: false),
                    AgenteId = table.Column<int>(type: "integer", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoricoKPI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoricoKPI_Agenti_AgenteId",
                        column: x => x.AgenteId,
                        principalTable: "Agenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Utenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Ruolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    AgenteId = table.Column<int>(type: "integer", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utenti_Agenti_AgenteId",
                        column: x => x.AgenteId,
                        principalTable: "Agenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Attivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    UtenteId = table.Column<int>(type: "integer", nullable: false),
                    TipoAttivita = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "text", nullable: true),
                    Priorita = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Completata = table.Column<bool>(type: "boolean", nullable: false),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PromemoriaMinutiPrima = table.Column<string>(type: "text", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attivita_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attivita_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Importazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    DataImportazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UtenteImportazioneId = table.Column<int>(type: "integer", nullable: false),
                    PeriodoCompetenza = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    RighePlesse = table.Column<int>(type: "integer", nullable: false),
                    RigheImportate = table.Column<int>(type: "integer", nullable: false),
                    RigheScartate = table.Column<int>(type: "integer", nullable: false),
                    RigheDuplicate = table.Column<int>(type: "integer", nullable: false),
                    LogEsito = table.Column<string>(type: "text", nullable: true),
                    Completata = table.Column<bool>(type: "boolean", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Importazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Importazioni_Utenti_UtenteImportazioneId",
                        column: x => x.UtenteImportazioneId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NoteCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    UtenteId = table.Column<int>(type: "integer", nullable: false),
                    Testo = table.Column<string>(type: "text", nullable: false),
                    DataInserimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteCliente_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoteCliente_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calendario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    AttivitaId = table.Column<int>(type: "integer", nullable: false),
                    GoogleEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimaSincronizzazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SincronizzatoConGoogle = table.Column<bool>(type: "boolean", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendario_Attivita_AttivitaId",
                        column: x => x.AttivitaId,
                        principalTable: "Attivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calendario_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ordini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    DataOrdine = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Importo = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    NumeroCucine = table.Column<int>(type: "integer", nullable: false),
                    NumeroElettrodomestici = table.Column<int>(type: "integer", nullable: false),
                    NumeroComplementi = table.Column<int>(type: "integer", nullable: false),
                    StatoOrdine = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ImportazioneId = table.Column<int>(type: "integer", nullable: true),
                    RiferimentoEsterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ordini_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ordini_Importazioni_ImportazioneId",
                        column: x => x.ImportazioneId,
                        principalTable: "Importazioni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agenti_AreaManagerId",
                table: "Agenti",
                column: "AreaManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Agenti_Email",
                table: "Agenti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agenti_Zona",
                table: "Agenti",
                column: "Zona");

            migrationBuilder.CreateIndex(
                name: "IX_Attivita_ClienteId",
                table: "Attivita",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Attivita_Completata_DataScadenza",
                table: "Attivita",
                columns: new[] { "Completata", "DataScadenza" });

            migrationBuilder.CreateIndex(
                name: "IX_Attivita_DataScadenza",
                table: "Attivita",
                column: "DataScadenza");

            migrationBuilder.CreateIndex(
                name: "IX_Attivita_UtenteId",
                table: "Attivita",
                column: "UtenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendario_AttivitaId",
                table: "Calendario",
                column: "AttivitaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendario_ClienteId",
                table: "Calendario",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendario_DataEvento",
                table: "Calendario",
                column: "DataEvento");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_AgenteId",
                table: "Clienti",
                column: "AgenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_CodiceCliente",
                table: "Clienti",
                column: "CodiceCliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Provincia",
                table: "Clienti",
                column: "Provincia");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Regione",
                table: "Clienti",
                column: "Regione");

            migrationBuilder.CreateIndex(
                name: "IX_Importazioni_DataImportazione",
                table: "Importazioni",
                column: "DataImportazione");

            migrationBuilder.CreateIndex(
                name: "IX_Importazioni_PeriodoCompetenza",
                table: "Importazioni",
                column: "PeriodoCompetenza");

            migrationBuilder.CreateIndex(
                name: "IX_Importazioni_UtenteImportazioneId",
                table: "Importazioni",
                column: "UtenteImportazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteCliente_ClienteId",
                table: "NoteCliente",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteCliente_UtenteId",
                table: "NoteCliente",
                column: "UtenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ObiettiviVendita_AgenteId_Mese_Anno",
                table: "ObiettiviVendita",
                columns: new[] { "AgenteId", "Mese", "Anno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_ClienteId",
                table: "Ordini",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_DataOrdine",
                table: "Ordini",
                column: "DataOrdine");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_ImportazioneId",
                table: "Ordini",
                column: "ImportazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_RiferimentoEsterno",
                table: "Ordini",
                column: "RiferimentoEsterno",
                unique: true,
                filter: "\"RiferimentoEsterno\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StoricoKPI_AgenteId",
                table: "StoricoKPI",
                column: "AgenteId");

            migrationBuilder.CreateIndex(
                name: "IX_StoricoKPI_Mese_Anno_AgenteId",
                table: "StoricoKPI",
                columns: new[] { "Mese", "Anno", "AgenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_AgenteId",
                table: "Utenti",
                column: "AgenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Email",
                table: "Utenti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Ruolo",
                table: "Utenti",
                column: "Ruolo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calendario");

            migrationBuilder.DropTable(
                name: "NoteCliente");

            migrationBuilder.DropTable(
                name: "ObiettiviVendita");

            migrationBuilder.DropTable(
                name: "Ordini");

            migrationBuilder.DropTable(
                name: "StoricoKPI");

            migrationBuilder.DropTable(
                name: "Attivita");

            migrationBuilder.DropTable(
                name: "Importazioni");

            migrationBuilder.DropTable(
                name: "Clienti");

            migrationBuilder.DropTable(
                name: "Utenti");

            migrationBuilder.DropTable(
                name: "Agenti");
        }
    }
}
