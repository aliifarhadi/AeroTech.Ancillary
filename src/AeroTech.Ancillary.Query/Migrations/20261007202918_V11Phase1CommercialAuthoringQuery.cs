using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V11Phase1CommercialAuthoringQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetiredAt",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SuspendedAt",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvancePurchasePeriod",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvancePurchaseUnit",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "BaggageFirstExcessPiece",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageFreePieces",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageLastExcessPiece",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggagePurchaseApplication",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageRuleDeference",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageTravelApplication",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaggageWeight",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageWeightUnit",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetiredAt",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "datetimeoffset",
                nullable: true);

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SuspendedAt",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "datetimeoffset",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationAirportId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionRoutePairs",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    OriginAirportId = table.Column<int>(type: "int", nullable: false),
                    DestinationAirportId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionRoutePairs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionRoutePairs_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionRoutePairs",
                column: "AncillaryProvisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryProvisionRoutePairs",
                schema: "ReadModel");

            migrationBuilder.DropColumn(
                name: "RetiredAt",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchasePeriod",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchaseUnit",
                schema: "ReadModel",
                table: "AncillaryProvisions");

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
                name: "BaggageFirstExcessPiece",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageFreePieces",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageLastExcessPiece",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggagePurchaseApplication",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageRuleDeference",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageTravelApplication",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageWeight",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageWeightUnit",
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
                name: "RetiredAt",
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
                name: "SuspendedAt",
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

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines");

            migrationBuilder.DropColumn(
                name: "StationAirportId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines");
        }
    }
}
