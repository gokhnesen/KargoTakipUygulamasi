using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderReceiverEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM CargoMovements");
            migrationBuilder.Sql("DELETE FROM Cargos");

            migrationBuilder.DropColumn(
                name: "ReceiverAddress",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "SenderAddress",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Cargos");

            migrationBuilder.CreateTable(
                name: "CargoReceivers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoReceivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CargoSenders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoSenders", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "ReceiverId",
                table: "Cargos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "Cargos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cargos_CargoReceivers_ReceiverId",
                table: "Cargos",
                column: "ReceiverId",
                principalTable: "CargoReceivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cargos_CargoSenders_SenderId",
                table: "Cargos",
                column: "SenderId",
                principalTable: "CargoSenders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cargos_CargoReceivers_ReceiverId",
                table: "Cargos");

            migrationBuilder.DropForeignKey(
                name: "FK_Cargos_CargoSenders_SenderId",
                table: "Cargos");

            migrationBuilder.DropTable(
                name: "CargoReceivers");

            migrationBuilder.DropTable(
                name: "CargoSenders");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_ReceiverId",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_SenderId",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Cargos");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverAddress",
                table: "Cargos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "Cargos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderAddress",
                table: "Cargos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Cargos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
