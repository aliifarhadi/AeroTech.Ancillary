using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V121LegacySchemaCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisionDayTimeRestrictions",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPriceLines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSeasonalPeriods",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionTravelDates",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "AdvancePurchasePeriod",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchaseUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirFareIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirFareTypes",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AircraftIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageFirstExcessPiece",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageFreePieces",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageLastExcessPiece",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggagePurchaseApplication",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageRuleDeference",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageTravelApplication",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageWeight",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageWeightUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CabinClassIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CustomerIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "CustomerTypes",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "DestinationAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FareBasisCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FareFamilyIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FeeApplicationUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FlightIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FlightNumbers",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "MarketingAirlineIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "OperatingAirlineIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "OriginAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PassengerTypeCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PointOfSaleIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "RbdIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SalesDiscontinueAt",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SalesEffectiveFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SeatCharacteristicCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SeatNumbers",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TimeFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TimeTo",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TravelFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TravelTo",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "ViaAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvancePurchasePeriod",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvancePurchaseUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AirFareIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "AirFareTypes",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "AircraftIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "BaggageFirstExcessPiece",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageFreePieces",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageLastExcessPiece",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggagePurchaseApplication",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageRuleDeference",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageTravelApplication",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaggageWeight",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageWeightUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CabinClassIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CustomerIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CustomerTypes",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DestinationAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FareBasisCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FareFamilyIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "FeeApplicationUnit",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeeCurrencyId",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumbers",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "MarketingAirlineIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "OperatingAirlineIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "OriginAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PassengerTypeCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PointOfSaleIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "RbdIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SalesDiscontinueAt",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SalesEffectiveFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatCharacteristicCodes",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatNumbers",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeTo",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TravelFrom",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TravelTo",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViaAirportIds",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "ProvisionDayTimeRestrictions",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionDayTimeRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionDayTimeRestrictions_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPriceLines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPriceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPriceLines_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionSeasonalPeriods",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionSeasonalPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionSeasonalPeriods_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionTravelDates",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    TravelDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionTravelDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionTravelDates_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDayTimeRestrictions_AncillaryProvisionId_DayOfWeek_StartTime_EndTime_Effect",
                schema: "Ancillary",
                table: "ProvisionDayTimeRestrictions",
                columns: new[] { "AncillaryProvisionId", "DayOfWeek", "StartTime", "EndTime", "Effect" },
                unique: true,
                filter: "([StartTime] IS NOT NULL AND [EndTime] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPriceLines_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionPriceLines",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeasonalPeriods_AncillaryProvisionId_StartDate_EndDate",
                schema: "Ancillary",
                table: "ProvisionSeasonalPeriods",
                columns: new[] { "AncillaryProvisionId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionTravelDates_AncillaryProvisionId_TravelDate",
                schema: "Ancillary",
                table: "ProvisionTravelDates",
                columns: new[] { "AncillaryProvisionId", "TravelDate" },
                unique: true);
        }
    }
}
