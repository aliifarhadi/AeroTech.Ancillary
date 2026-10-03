using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceReservations",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReservationUnits",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ServiceReservationId = table.Column<long>(type: "bigint", nullable: false),
                    UnitReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    ProductRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductVersion = table.Column<int>(type: "int", nullable: false),
                    PriceRuleId = table.Column<long>(type: "bigint", nullable: false),
                    TravellerRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoundRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FlightRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoveredFlightIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InventoryControl = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReservationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceReservationUnits_ServiceReservations_ServiceReservationId",
                        column: x => x.ServiceReservationId,
                        principalSchema: "Ancillary",
                        principalTable: "ServiceReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReservations_IdempotencyKey",
                schema: "Ancillary",
                table: "ServiceReservations",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReservationUnits_ServiceReservationId_UnitReference",
                schema: "Ancillary",
                table: "ServiceReservationUnits",
                columns: new[] { "ServiceReservationId", "UnitReference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceReservationUnits",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ServiceReservations",
                schema: "Ancillary");
        }
    }
}
