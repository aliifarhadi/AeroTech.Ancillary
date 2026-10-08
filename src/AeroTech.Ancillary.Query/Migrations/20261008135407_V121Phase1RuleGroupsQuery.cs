using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V121Phase1RuleGroupsQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceDateBasis",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AdvancePurchaseRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdvancePurchaseSameTimeAsTicketed",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "BaggageApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DayTimeApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FareApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FlightApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GeographyRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PassengerEligibilityRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SalesRestrictionsRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SeatApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TravelDateRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionCoverageCountries",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionCoverageCountries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionDayTimeWindows",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    DaysOfWeekMask = table.Column<byte>(type: "tinyint", nullable: false),
                    StartLocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndLocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Effect = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionDayTimeWindows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionEligibleAgeBands",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: false),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionEligibleAgeBands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionPermittedTravelPeriods",
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
                    table.PrimaryKey("PK_AncillaryProvisionPermittedTravelPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionServiceLocations",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionServiceLocations", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                WITH [dated] AS
                (
                    SELECT travelDate.[Id], travelDate.[AncillaryProvisionId], travelDate.[TravelDate]
                    FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate
                    WHERE NOT EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId])
                       OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate])
                ),
                [datedIslands] AS
                (
                    SELECT MIN(numbered.[Id]) AS [Id], numbered.[AncillaryProvisionId], MIN(numbered.[TravelDate]) AS [StartDate], MAX(numbered.[TravelDate]) AS [EndDate]
                    FROM
                    (
                        SELECT [Id], [AncillaryProvisionId], [TravelDate],
                               DATEDIFF(DAY, '2000-01-01', [TravelDate]) - ROW_NUMBER() OVER (PARTITION BY [AncillaryProvisionId] ORDER BY [TravelDate]) AS [Island]
                        FROM [dated]
                    ) AS numbered
                    GROUP BY numbered.[AncillaryProvisionId], numbered.[Island]
                ),
                [seasonal] AS
                (
                    SELECT season.[Id], season.[AncillaryProvisionId], season.[StartDate], season.[EndDate],
                           MAX(season.[EndDate]) OVER (PARTITION BY season.[AncillaryProvisionId] ORDER BY season.[StartDate], season.[EndDate] ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS [PreviousEnd]
                    FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season
                    WHERE NOT EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = season.[AncillaryProvisionId])
                ),
                [seasonalIslands] AS
                (
                    SELECT MIN(grouped.[Id]) AS [Id], grouped.[AncillaryProvisionId], MIN(grouped.[StartDate]) AS [StartDate], MAX(grouped.[EndDate]) AS [EndDate]
                    FROM
                    (
                        SELECT [Id], [AncillaryProvisionId], [StartDate], [EndDate],
                               SUM(CASE WHEN [PreviousEnd] IS NULL OR ([PreviousEnd] < '9999-12-31' AND [StartDate] > DATEADD(DAY, 1, [PreviousEnd])) THEN 1 ELSE 0 END)
                                   OVER (PARTITION BY [AncillaryProvisionId] ORDER BY [StartDate], [EndDate] ROWS UNBOUNDED PRECEDING) AS [Island]
                        FROM [seasonal]
                    ) AS grouped
                    GROUP BY grouped.[AncillaryProvisionId], grouped.[Island]
                )
                INSERT INTO [ReadModel].[AncillaryProvisionPermittedTravelPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate])
                SELECT permitted.[Id], permitted.[AncillaryProvisionId], permitted.[StartDate], permitted.[EndDate]
                FROM (SELECT * FROM [datedIslands] UNION ALL SELECT * FROM [seasonalIslands]) AS permitted
                JOIN [ReadModel].[AncillaryProvisions] AS provision ON provision.[Id] = permitted.[AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionBlackoutPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate])
                SELECT provision.[Id], provision.[Id], '0001-01-01', '9999-12-31'
                FROM [ReadModel].[AncillaryProvisions] AS provision
                WHERE (
                        EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = provision.[Id])
                        AND EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = provision.[Id])
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate
                            JOIN [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season ON season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate]
                            WHERE travelDate.[AncillaryProvisionId] = provision.[Id]
                        )
                      )
                   OR EXISTS
                      (
                        SELECT 1
                        FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions] AS restriction
                        WHERE restriction.[AncillaryProvisionId] = provision.[Id]
                          AND NOT ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime])
                      );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionDayTimeWindows] ([Id], [AncillaryProvisionId], [DaysOfWeekMask], [StartLocalTime], [EndLocalTime], [Effect])
                SELECT restriction.[Id], restriction.[AncillaryProvisionId], CASE restriction.[DayOfWeek] WHEN 0 THEN 64 ELSE POWER(2, restriction.[DayOfWeek] - 1) END, restriction.[StartTime], restriction.[EndTime], restriction.[Effect]
                FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions] AS restriction
                WHERE (restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime];
                """);

            migrationBuilder.Sql(
                """
                UPDATE provision
                SET [PassengerEligibilityRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionPassengerTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionEligibleAgeBands] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                        THEN provision.[Id] END,
                    [SalesRestrictionsRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionPointsOfSale] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionCustomers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionCustomerTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR provision.[SalesEffectiveFrom] IS NOT NULL OR provision.[SalesDiscontinueAt] IS NOT NULL
                        THEN provision.[Id] END,
                    [GeographyRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionOriginAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionDestinationAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionViaAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionCoverageCountries] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionRoutePairs] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionServiceLocations] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                        THEN provision.[Id] END,
                    [FlightApplicationRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionMarketingAirlines] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionOperatingAirlines] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionFlightNumbers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionFlights] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionAircraft] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                        THEN provision.[Id] END,
                    [FareApplicationRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionAirFares] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionAirFareTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionFareFamilies] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionFareBases] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionCabinClasses] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionRbds] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                        THEN provision.[Id] END,
                    [TravelDateRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionTravelDates] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionBlackoutPeriods] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR ((
                        EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = provision.[Id])
                        AND EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = provision.[Id])
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM [ReadModel].[AncillaryProvisionTravelDates] AS travelDate
                            JOIN [ReadModel].[AncillaryProvisionSeasonalPeriods] AS season ON season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate]
                            WHERE travelDate.[AncillaryProvisionId] = provision.[Id]
                        )
                      ) OR EXISTS
                      (
                        SELECT 1
                        FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions] AS restriction
                        WHERE restriction.[AncillaryProvisionId] = provision.[Id]
                          AND NOT ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime])
                      ))
                        THEN provision.[Id] END,
                    [DayTimeApplicationRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions] AS restriction WHERE restriction.[AncillaryProvisionId] = provision.[Id] AND ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime]))
                        THEN provision.[Id] END,
                    [AdvancePurchaseRuleId] = CASE WHEN (provision.[AdvancePurchasePeriod] IS NOT NULL AND provision.[AdvancePurchaseUnit] IS NOT NULL)
                        THEN provision.[Id] END,
                    [BaggageApplicationRuleId] = CASE WHEN (provision.[ApplicationType] = 2 AND provision.[BaggageWeightUnit] IS NOT NULL AND provision.[BaggagePurchaseApplication] IS NOT NULL)
                        THEN provision.[Id] END,
                    [SeatApplicationRuleId] = CASE WHEN EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeatNumbers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]) OR EXISTS (SELECT 1 FROM [ReadModel].[AncillaryProvisionSeatCharacteristics] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                        THEN provision.[Id] END
                FROM [ReadModel].[AncillaryProvisions] AS provision;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionCoverageCountries_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionCoverageCountries",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionDayTimeWindows_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionDayTimeWindows",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionEligibleAgeBands_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionEligibleAgeBands",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionPermittedTravelPeriods_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionPermittedTravelPeriods",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionServiceLocations_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionServiceLocations",
                column: "AncillaryProvisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE marker
                FROM [ReadModel].[AncillaryProvisionBlackoutPeriods] AS marker
                WHERE marker.[Id] = marker.[AncillaryProvisionId] AND marker.[StartDate] = '0001-01-01' AND marker.[EndDate] = '9999-12-31';
                """);

            migrationBuilder.DropTable(
                name: "AncillaryProvisionCoverageCountries",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionDayTimeWindows",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionEligibleAgeBands",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionPermittedTravelPeriods",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionServiceLocations",
                schema: "ReadModel");

            migrationBuilder.DropColumn(
                name: "ServiceDateBasis",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchaseRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchaseSameTimeAsTicketed",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "DayTimeApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FareApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "FlightApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "GeographyRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PassengerEligibilityRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SalesRestrictionsRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "SeatApplicationRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TravelDateRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");
        }
    }
}
