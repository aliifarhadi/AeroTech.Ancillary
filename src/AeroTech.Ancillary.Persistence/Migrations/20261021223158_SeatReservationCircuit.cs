using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeatReservationCircuit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PricingId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatMapSelectionId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatNumber",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricingId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "SeatMapSelectionId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");
        }
    }
}
