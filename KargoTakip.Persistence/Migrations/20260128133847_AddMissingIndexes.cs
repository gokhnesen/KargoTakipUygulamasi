using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos",
                column: "ReceiverId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos",
                column: "SenderId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_Status",
                table: "Cargos",
                column: "Status",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_Status",
                table: "Cargos");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos",
                column: "SenderId");
        }
    }
}
