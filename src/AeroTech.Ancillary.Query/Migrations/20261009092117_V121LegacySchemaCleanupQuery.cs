using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V121LegacySchemaCleanupQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryProvisionDayTimeRestrictions",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionPriceLines",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionSeasonalPeriods",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionTravelDates",
                schema: "ReadModel");

            migrationBuilder.DropColumn(
                name: "AirFareIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirFareTypes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AircraftIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CabinClassIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CustomerIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CustomerTypes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "DestinationAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FareBasisCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FareFamilyIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FeeApplicationUnit",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FlightIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FlightNumbers",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "MarketingAirlineIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "OperatingAirlineIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "OriginAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PassengerTypeCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PointOfSaleIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "RbdIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SeatCharacteristicCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SeatNumbers",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TimeFrom",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TimeTo",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TravelFrom",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TravelTo",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "ViaAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AirFareIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "AirFareTypes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "AircraftIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CabinClassIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CustomerIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CustomerTypes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DestinationAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FareBasisCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FareFamilyIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "FeeApplicationUnit",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeeCurrencyId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumbers",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "MarketingAirlineIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "OperatingAirlineIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "OriginAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PassengerTypeCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PointOfSaleIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "RbdIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SeatCharacteristicCodes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SeatNumbers",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeFrom",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeTo",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TravelFrom",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TravelTo",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViaAirportIds",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionDayTimeRestrictions",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Effect = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionDayTimeRestrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionPriceLines",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionPriceLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionSeasonalPeriods",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionSeasonalPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionTravelDates",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    TravelDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionTravelDates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionDayTimeRestrictions_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionDayTimeRestrictions",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionPriceLines_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionSeasonalPeriods_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionSeasonalPeriods",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionTravelDates_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionTravelDates",
                column: "AncillaryProvisionId");
        }
    }
}
