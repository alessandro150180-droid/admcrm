using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CucineCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AggiungiFornitori : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fornitori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataModifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eliminato = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornitori", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fornitori_Nome",
                table: "Fornitori",
                column: "Nome",
                unique: true,
                filter: "\"Eliminato\" = false");

            // Elenco iniziale: "Nobilia" è il fornitore di tutto il fatturato già presente in
            // Ordini prima di questa migration (finora l'unico gestito dal CRM), gli altri sono
            // i nuovi fornitori richiesti. L'Id è auto-generato (identity), non specificato qui:
            // il backfill sotto lo recupera per nome, non ipotizza un valore fisso.
            migrationBuilder.InsertData(
                table: "Fornitori",
                columns: new[] { "Nome", "Attivo", "Eliminato", "DataCreazione" },
                values: new object[,]
                {
                    { "Nobilia", true, false, DateTime.UtcNow },
                    { "NobiSmart", true, false, DateTime.UtcNow },
                    { "NobiDirect", true, false, DateTime.UtcNow },
                    { "Comma", true, false, DateTime.UtcNow },
                    { "GierreDue", true, false, DateTime.UtcNow },
                });

            migrationBuilder.AddColumn<int>(
                name: "FornitoreId",
                table: "Ordini",
                type: "integer",
                nullable: true);

            // Backfill: tutti gli ordini esistenti sono fatturato Nobilia (unico fornitore gestito
            // finora dal CRM).
            migrationBuilder.Sql(
                "UPDATE \"Ordini\" SET \"FornitoreId\" = (SELECT \"Id\" FROM \"Fornitori\" WHERE \"Nome\" = 'Nobilia') " +
                "WHERE \"FornitoreId\" IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "FornitoreId",
                table: "Ordini",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_FornitoreId",
                table: "Ordini",
                column: "FornitoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ordini_Fornitori_FornitoreId",
                table: "Ordini",
                column: "FornitoreId",
                principalTable: "Fornitori",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ordini_Fornitori_FornitoreId",
                table: "Ordini");

            migrationBuilder.DropIndex(
                name: "IX_Ordini_FornitoreId",
                table: "Ordini");

            migrationBuilder.DropColumn(
                name: "FornitoreId",
                table: "Ordini");

            migrationBuilder.DropTable(
                name: "Fornitori");
        }
    }
}
