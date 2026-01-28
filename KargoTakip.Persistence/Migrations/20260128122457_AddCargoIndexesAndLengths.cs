using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoIndexesAndLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CargoMovements_CargoId",
                table: "CargoMovements");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Cargos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "CargoReceivers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_CreatedDate_Id",
                table: "Cargos",
                columns: new[] { "CreatedDate", "Id" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_Status_CreatedDate_Id",
                table: "Cargos",
                columns: new[] { "Status", "CreatedDate", "Id" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_TrackingNumber",
                table: "Cargos",
                column: "TrackingNumber",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CargoReceivers_Email",
                table: "CargoReceivers",
                column: "Email",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CargoMovements_CargoId_CreatedDate_Id",
                table: "CargoMovements",
                columns: new[] { "CargoId", "CreatedDate", "Id" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cargos_CreatedDate_Id",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_Status_CreatedDate_Id",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_TrackingNumber",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_CargoReceivers_Email",
                table: "CargoReceivers");

            migrationBuilder.DropIndex(
                name: "IX_CargoMovements_CargoId_CreatedDate_Id",
                table: "CargoMovements");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Cargos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "CargoReceivers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_CargoMovements_CargoId",
                table: "CargoMovements",
                column: "CargoId");
        }
    }
}
