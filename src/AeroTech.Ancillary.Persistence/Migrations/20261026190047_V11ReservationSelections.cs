using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V11ReservationSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentTimeWithOffset",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "CoveredFlightIds",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.AddColumn<string>(
                name: "ServiceDefinitionRef",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitAirportServiceSelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    TimeWithOffset = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GuestCount = table.Column<int>(type: "int", nullable: true),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitAirportServiceSelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitAirportServiceSelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitAssistedTravelSelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    AssistanceSsrCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitAssistedTravelSelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitAssistedTravelSelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitBaggageSelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    BagRef = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MeasuredWeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LengthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WidthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    EquipmentKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitBaggageSelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitBaggageSelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitConnectivitySelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitConnectivitySelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitConnectivitySelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitFlights",
                schema: "Ancillary",
                columns: table => new
                {
                    FlightId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitFlights", x => new { x.AncillaryReservationUnitId, x.FlightId });
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitFlights_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitMealSelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    MealCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    MenuItemRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitMealSelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitMealSelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnitUpgradeSelections",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryReservationUnitId = table.Column<long>(type: "bigint", nullable: false),
                    ToCabinId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnitUpgradeSelections", x => x.AncillaryReservationUnitId);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnitUpgradeSelections_AncillaryReservationUnits_AncillaryReservationUnitId",
                        column: x => x.AncillaryReservationUnitId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitAirportServiceSelections",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitAssistedTravelSelections",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitBaggageSelections",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitConnectivitySelections",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitFlights",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitMealSelections",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservationUnitUpgradeSelections",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "ServiceDefinitionRef",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                schema: "Ancillary",
                table: "AncillaryReservationUnits");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppointmentTimeWithOffset",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoveredFlightIds",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                type: "int",
                nullable: true);
        }
    }
}
