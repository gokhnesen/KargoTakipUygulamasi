using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoLastLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastLocation",
                table: "Cargos",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE c SET c.LastLocation = m.Location
                FROM Cargos c
                INNER JOIN (
                    SELECT CargoId, Location,
                        ROW_NUMBER() OVER (PARTITION BY CargoId ORDER BY CreatedDate DESC) AS rn
                    FROM CargoMovements
                    WHERE IsDeleted = 0
                ) m ON c.Id = m.CargoId AND m.rn = 1
                WHERE c.IsDeleted = 0
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLocation",
                table: "Cargos");
        }
    }
}
