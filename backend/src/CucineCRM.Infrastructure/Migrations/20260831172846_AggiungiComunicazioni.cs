using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CucineCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AggiungiComunicazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comunicazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    TipoContenuto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DimensioneByte = table.Column<long>(type: "bigint", nullable: false),
                    Contenuto = table.Column<byte[]>(type: "bytea", nullable: false),
                    UtentePubblicazioneId = table.Column<int>(type: "integer", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunicazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comunicazioni_Utenti_UtentePubblicazioneId",
                        column: x => x.UtentePubblicazioneId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comunicazioni_DataCreazione",
                table: "Comunicazioni",
                column: "DataCreazione");

            migrationBuilder.CreateIndex(
                name: "IX_Comunicazioni_UtentePubblicazioneId",
                table: "Comunicazioni",
                column: "UtentePubblicazioneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comunicazioni");
        }
    }
}
