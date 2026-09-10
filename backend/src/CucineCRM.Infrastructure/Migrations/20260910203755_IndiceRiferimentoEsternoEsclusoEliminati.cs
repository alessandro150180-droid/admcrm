using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CucineCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndiceRiferimentoEsternoEsclusoEliminati : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ordini_RiferimentoEsterno",
                table: "Ordini");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_RiferimentoEsterno",
                table: "Ordini",
                column: "RiferimentoEsterno",
                unique: true,
                filter: "\"RiferimentoEsterno\" IS NOT NULL AND \"Eliminato\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ordini_RiferimentoEsterno",
                table: "Ordini");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_RiferimentoEsterno",
                table: "Ordini",
                column: "RiferimentoEsterno",
                unique: true,
                filter: "\"RiferimentoEsterno\" IS NOT NULL");
        }
    }
}
