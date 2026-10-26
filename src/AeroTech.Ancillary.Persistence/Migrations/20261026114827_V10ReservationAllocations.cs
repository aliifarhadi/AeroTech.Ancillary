using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V10ReservationAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppendIdempotencyKey",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppointmentTimeWithOffset",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitAllocations",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    ResourceKind = table.Column<int>(type: "int", nullable: false),
                    InventorySourceId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitAllocations_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservationUnitAllocations_AncillaryReservationUnitId_ResourceKind_InventorySourceId",
                schema: "Ancillary",
                table: "AncillaryReservationUnitAllocations",
                columns: new[] { "AncillaryReservationUnitId", "ResourceKind", "InventorySourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservationUnitAllocations_ResourceKind_InventorySourceId",
                schema: "Ancillary",
                table: "AncillaryReservationUnitAllocations",
                columns: new[] { "ResourceKind", "InventorySourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitAllocations",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "AppendIdempotencyKey",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "AppointmentTimeWithOffset",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");
        }
    }
}
