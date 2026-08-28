using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CucineCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditNotificheGoogleCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleAccessToken",
                table: "Utenti",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleRefreshToken",
                table: "Utenti",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GoogleTokenScadenza",
                table: "Utenti",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtenteId = table.Column<int>(type: "integer", nullable: true),
                    NomeEntita = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntitaId = table.Column<int>(type: "integer", nullable: false),
                    Azione = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtenteId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Messaggio = table.Column<string>(type: "text", nullable: true),
                    RiferimentoEntitaId = table.Column<int>(type: "integer", nullable: true),
                    Letta = table.Column<bool>(type: "boolean", nullable: false),
                    DataLettura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifiche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifiche_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_DataCreazione",
                table: "AuditLog",
                column: "DataCreazione");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_NomeEntita_EntitaId",
                table: "AuditLog",
                columns: new[] { "NomeEntita", "EntitaId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UtenteId",
                table: "AuditLog",
                column: "UtenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_UtenteId_Letta",
                table: "Notifiche",
                columns: new[] { "UtenteId", "Letta" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "Notifiche");

            migrationBuilder.DropColumn(
                name: "GoogleAccessToken",
                table: "Utenti");

            migrationBuilder.DropColumn(
                name: "GoogleRefreshToken",
                table: "Utenti");

            migrationBuilder.DropColumn(
                name: "GoogleTokenScadenza",
                table: "Utenti");
        }
    }
}
