using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V121Phase1RuleGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceDateBasis",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProvisionAdvancePurchaseRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    MinimumPeriod = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    SameTimeAsTicketed = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAdvancePurchaseRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAdvancePurchaseRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionBaggageApplicationRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FreePieces = table.Column<int>(type: "int", nullable: true),
                    FirstExcessPiece = table.Column<int>(type: "int", nullable: true),
                    LastExcessPiece = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    WeightUnit = table.Column<int>(type: "int", nullable: false),
                    TravelApplication = table.Column<int>(type: "int", nullable: true),
                    PurchaseApplication = table.Column<int>(type: "int", nullable: false),
                    RuleDeference = table.Column<int>(type: "int", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionBaggageApplicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionBaggageApplicationRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionDayTimeApplicationRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionDayTimeApplicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionDayTimeApplicationRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFareApplicationRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFareApplicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFareApplicationRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFlightApplicationRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFlightApplicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFlightApplicationRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionGeographyRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionGeographyRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionGeographyRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPassengerEligibilityRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPassengerEligibilityRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPassengerEligibilityRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionSalesRestrictionsRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    SalesEffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SalesDiscontinueAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionSalesRestrictionsRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionSalesRestrictionsRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionSeatApplicationRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionSeatApplicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionSeatApplicationRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionTravelDateRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionTravelDateRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionTravelDateRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionDayTimeWindows",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionDayTimeApplicationRuleId = table.Column<long>(type: "bigint", nullable: false),
                    DaysOfWeekMask = table.Column<byte>(type: "tinyint", nullable: false),
                    StartLocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndLocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionDayTimeWindows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionDayTimeWindows_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionDayTimeWindows_ProvisionDayTimeApplicationRules_ProvisionDayTimeApplicationRuleId",
                        column: x => x.ProvisionDayTimeApplicationRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "ProvisionDayTimeApplicationRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProvisionCoverageCountries",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionGeographyRuleId = table.Column<long>(type: "bigint", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionCoverageCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionCoverageCountries_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionCoverageCountries_ProvisionGeographyRules_ProvisionGeographyRuleId",
                        column: x => x.ProvisionGeographyRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "ProvisionGeographyRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProvisionServiceLocations",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionGeographyRuleId = table.Column<long>(type: "bigint", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionServiceLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionServiceLocations_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionServiceLocations_ProvisionGeographyRules_ProvisionGeographyRuleId",
                        column: x => x.ProvisionGeographyRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "ProvisionGeographyRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProvisionEligibleAgeBands",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionPassengerEligibilityRuleId = table.Column<long>(type: "bigint", nullable: false),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: false),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionEligibleAgeBands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionEligibleAgeBands_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionEligibleAgeBands_ProvisionPassengerEligibilityRules_ProvisionPassengerEligibilityRuleId",
                        column: x => x.ProvisionPassengerEligibilityRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "ProvisionPassengerEligibilityRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPermittedTravelPeriods",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionTravelDateRuleId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPermittedTravelPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPermittedTravelPeriods_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionPermittedTravelPeriods_ProvisionTravelDateRules_ProvisionTravelDateRuleId",
                        column: x => x.ProvisionTravelDateRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "ProvisionTravelDateRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProvisionRuleMigrationAudit",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTable = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceRowId = table.Column<long>(type: "bigint", nullable: true),
                    TargetTable = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TargetRowId = table.Column<long>(type: "bigint", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionRuleMigrationAudit", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionPassengerEligibilityRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionPassengerTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionEligibleAgeBands] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]);
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionPassengerTypes] SET [ProvisionPassengerEligibilityRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSalesRestrictionsRules] ([Id], [AncillaryProvisionId], [SalesEffectiveFrom], [SalesDiscontinueAt], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[SalesEffectiveFrom], provision.[SalesDiscontinueAt], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionPointsOfSale] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionCustomers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionCustomerTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR provision.[SalesEffectiveFrom] IS NOT NULL
                   OR provision.[SalesDiscontinueAt] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionPointsOfSale] SET [ProvisionSalesRestrictionsRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionCustomers] SET [ProvisionSalesRestrictionsRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionCustomerTypes] SET [ProvisionSalesRestrictionsRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionGeographyRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionOriginAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionDestinationAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionViaAirports] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionCoverageCountries] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionRoutePairs] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionServiceLocations] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]);
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionOriginAirports] SET [ProvisionGeographyRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionDestinationAirports] SET [ProvisionGeographyRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionViaAirports] SET [ProvisionGeographyRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionRoutePairs] SET [ProvisionGeographyRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFlightApplicationRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionMarketingAirlines] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionOperatingAirlines] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionFlightNumbers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionFlights] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionAircraft] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]);
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionMarketingAirlines] SET [ProvisionFlightApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionOperatingAirlines] SET [ProvisionFlightApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionFlightNumbers] SET [ProvisionFlightApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionFlights] SET [ProvisionFlightApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionAircraft] SET [ProvisionFlightApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFareApplicationRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionAirFares] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionAirFareTypes] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionFareFamilies] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionFareBases] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionCabinClasses] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionRbds] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]);
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionAirFares] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionAirFareTypes] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionFareFamilies] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionFareBases] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionCabinClasses] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionRbds] SET [ProvisionFareApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionTravelDateRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionBlackoutPeriods] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR ((
                        EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = provision.[Id])
                        AND EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = provision.[Id])
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM [Ancillary].[ProvisionTravelDates] AS travelDate
                            JOIN [Ancillary].[ProvisionSeasonalPeriods] AS season ON season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate]
                            WHERE travelDate.[AncillaryProvisionId] = provision.[Id]
                        )
                      )
                   OR EXISTS
                      (
                        SELECT 1
                        FROM [Ancillary].[ProvisionDayTimeRestrictions] AS restriction
                        WHERE restriction.[AncillaryProvisionId] = provision.[Id]
                          AND NOT ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime])
                      ));
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionBlackoutPeriods] SET [ProvisionTravelDateRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionDayTimeApplicationRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionDayTimeRestrictions] AS restriction WHERE restriction.[AncillaryProvisionId] = provision.[Id] AND ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime]));
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionAdvancePurchaseRules] ([Id], [AncillaryProvisionId], [MinimumPeriod], [Unit], [SameTimeAsTicketed], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[AdvancePurchasePeriod], provision.[AdvancePurchaseUnit], 0, provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE (provision.[AdvancePurchasePeriod] IS NOT NULL AND provision.[AdvancePurchaseUnit] IS NOT NULL);
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionBaggageApplicationRules] ([Id], [AncillaryProvisionId], [FreePieces], [FirstExcessPiece], [LastExcessPiece], [Weight], [WeightUnit], [TravelApplication], [PurchaseApplication], [RuleDeference], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[BaggageFreePieces], provision.[BaggageFirstExcessPiece], provision.[BaggageLastExcessPiece], provision.[BaggageWeight], provision.[BaggageWeightUnit], provision.[BaggageTravelApplication], provision.[BaggagePurchaseApplication], provision.[BaggageRuleDeference], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE (provision.[ApplicationType] = 2 AND provision.[BaggageWeightUnit] IS NOT NULL AND provision.[BaggagePurchaseApplication] IS NOT NULL);
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSeatApplicationRules] ([Id], [AncillaryProvisionId], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeatNumbers] AS child WHERE child.[AncillaryProvisionId] = provision.[Id])
                   OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeatCharacteristics] AS child WHERE child.[AncillaryProvisionId] = provision.[Id]);
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionSeatNumbers] SET [ProvisionSeatApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Ancillary].[ProvisionSeatCharacteristics] SET [ProvisionSeatApplicationRuleId] = [AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                WITH [dated] AS
                (
                    SELECT travelDate.[Id], travelDate.[AncillaryProvisionId], travelDate.[TravelDate]
                    FROM [Ancillary].[ProvisionTravelDates] AS travelDate
                    WHERE NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId])
                       OR EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate])
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
                    FROM [Ancillary].[ProvisionSeasonalPeriods] AS season
                    WHERE NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = season.[AncillaryProvisionId])
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
                INSERT INTO [Ancillary].[ProvisionPermittedTravelPeriods] ([Id], [AncillaryProvisionId], [ProvisionTravelDateRuleId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
                SELECT permitted.[Id], permitted.[AncillaryProvisionId], permitted.[AncillaryProvisionId], permitted.[StartDate], permitted.[EndDate], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM (SELECT * FROM [datedIslands] UNION ALL SELECT * FROM [seasonalIslands]) AS permitted
                JOIN [Ancillary].[AncillaryProvisions] AS provision ON provision.[Id] = permitted.[AncillaryProvisionId];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionBlackoutPeriods] ([Id], [AncillaryProvisionId], [ProvisionTravelDateRuleId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], provision.[Id], '0001-01-01', '9999-12-31', provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE (
                        EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = provision.[Id])
                        AND EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS season WHERE season.[AncillaryProvisionId] = provision.[Id])
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM [Ancillary].[ProvisionTravelDates] AS travelDate
                            JOIN [Ancillary].[ProvisionSeasonalPeriods] AS season ON season.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN season.[StartDate] AND season.[EndDate]
                            WHERE travelDate.[AncillaryProvisionId] = provision.[Id]
                        )
                      )
                   OR EXISTS
                      (
                        SELECT 1
                        FROM [Ancillary].[ProvisionDayTimeRestrictions] AS restriction
                        WHERE restriction.[AncillaryProvisionId] = provision.[Id]
                          AND NOT ((restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime])
                      );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionDayTimeWindows] ([Id], [AncillaryProvisionId], [ProvisionDayTimeApplicationRuleId], [DaysOfWeekMask], [StartLocalTime], [EndLocalTime], [Effect], [LastUpdateTime], [LastUpdatedBy])
                SELECT restriction.[Id], restriction.[AncillaryProvisionId], restriction.[AncillaryProvisionId], CASE restriction.[DayOfWeek] WHEN 0 THEN 64 ELSE POWER(2, restriction.[DayOfWeek] - 1) END, restriction.[StartTime], restriction.[EndTime], restriction.[Effect], restriction.[LastUpdateTime], restriction.[LastUpdatedBy]
                FROM [Ancillary].[ProvisionDayTimeRestrictions] AS restriction
                WHERE (restriction.[StartTime] IS NULL AND restriction.[EndTime] IS NULL) OR restriction.[StartTime] < restriction.[EndTime];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionRuleMigrationAudit] ([AncillaryProvisionId], [SourceTable], [SourceRowId], [TargetTable], [TargetRowId], [Outcome])
                SELECT travelDate.[AncillaryProvisionId], N'ProvisionTravelDates', travelDate.[Id], CASE WHEN permitted.[Id] IS NULL THEN NULL ELSE N'ProvisionPermittedTravelPeriods' END, permitted.[Id],
                       CASE WHEN permitted.[Id] IS NULL THEN N'ExcludedByIntersection' ELSE N'MergedIntoPermittedPeriod' END
                FROM [Ancillary].[ProvisionTravelDates] AS travelDate
                LEFT JOIN [Ancillary].[ProvisionPermittedTravelPeriods] AS permitted
                    ON permitted.[AncillaryProvisionId] = travelDate.[AncillaryProvisionId] AND travelDate.[TravelDate] BETWEEN permitted.[StartDate] AND permitted.[EndDate]
                UNION ALL
                SELECT season.[AncillaryProvisionId], N'ProvisionSeasonalPeriods', season.[Id], CASE WHEN permitted.[Id] IS NULL THEN NULL ELSE N'ProvisionPermittedTravelPeriods' END, permitted.[Id],
                       CASE WHEN permitted.[Id] IS NULL THEN N'IntersectedWithTravelDates' ELSE N'MergedIntoPermittedPeriod' END
                FROM [Ancillary].[ProvisionSeasonalPeriods] AS season
                LEFT JOIN [Ancillary].[ProvisionPermittedTravelPeriods] AS permitted
                    ON permitted.[AncillaryProvisionId] = season.[AncillaryProvisionId] AND season.[StartDate] >= permitted.[StartDate] AND season.[EndDate] <= permitted.[EndDate]
                    AND NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS travelDate WHERE travelDate.[AncillaryProvisionId] = season.[AncillaryProvisionId])
                UNION ALL
                SELECT restriction.[AncillaryProvisionId], N'ProvisionDayTimeRestrictions', restriction.[Id], CASE WHEN dayTimeWindow.[Id] IS NULL THEN NULL ELSE N'ProvisionDayTimeWindows' END, dayTimeWindow.[Id],
                       CASE WHEN dayTimeWindow.[Id] IS NULL THEN N'ManualMappingRequired' ELSE N'ConvertedToDayTimeWindow' END
                FROM [Ancillary].[ProvisionDayTimeRestrictions] AS restriction
                LEFT JOIN [Ancillary].[ProvisionDayTimeWindows] AS dayTimeWindow ON dayTimeWindow.[Id] = restriction.[Id]
                UNION ALL
                SELECT marker.[AncillaryProvisionId], N'AncillaryProvisions', marker.[AncillaryProvisionId], N'ProvisionBlackoutPeriods', marker.[Id], N'QuarantinedAsUnsaleable'
                FROM [Ancillary].[ProvisionBlackoutPeriods] AS marker
                WHERE marker.[Id] = marker.[AncillaryProvisionId] AND marker.[StartDate] = '0001-01-01' AND marker.[EndDate] = '9999-12-31';
                """);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionViaAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeatNumbers_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers",
                column: "ProvisionSeatApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeatCharacteristics_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics",
                column: "ProvisionSeatApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionRoutePairs_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionRbds_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPointsOfSale_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale",
                column: "ProvisionSalesRestrictionsRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPassengerTypes_ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes",
                column: "ProvisionPassengerEligibilityRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionOriginAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionOperatingAirlines_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines",
                column: "ProvisionFlightApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionMarketingAirlines_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines",
                column: "ProvisionFlightApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFlights_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights",
                column: "ProvisionFlightApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFlightNumbers_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers",
                column: "ProvisionFlightApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFareFamilies_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFareBases_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDestinationAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCustomerTypes_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes",
                column: "ProvisionSalesRestrictionsRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCustomers_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers",
                column: "ProvisionSalesRestrictionsRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCabinClasses_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionBlackoutPeriods_ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods",
                column: "ProvisionTravelDateRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAirFareTypes_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAirFares_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares",
                column: "ProvisionFareApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAircraft_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft",
                column: "ProvisionFlightApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAdvancePurchaseRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionAdvancePurchaseRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionBaggageApplicationRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionBaggageApplicationRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCoverageCountries_AncillaryProvisionId_CountryId",
                schema: "Ancillary",
                table: "ProvisionCoverageCountries",
                columns: new[] { "AncillaryProvisionId", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCoverageCountries_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionCoverageCountries",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDayTimeApplicationRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionDayTimeApplicationRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDayTimeWindows_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionDayTimeWindows",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDayTimeWindows_ProvisionDayTimeApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionDayTimeWindows",
                column: "ProvisionDayTimeApplicationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionEligibleAgeBands_AncillaryProvisionId_AgeFromInclusive",
                schema: "Ancillary",
                table: "ProvisionEligibleAgeBands",
                columns: new[] { "AncillaryProvisionId", "AgeFromInclusive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionEligibleAgeBands_ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionEligibleAgeBands",
                column: "ProvisionPassengerEligibilityRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFareApplicationRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionFareApplicationRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFlightApplicationRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionFlightApplicationRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionGeographyRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionGeographyRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPassengerEligibilityRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionPassengerEligibilityRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPermittedTravelPeriods_AncillaryProvisionId_StartDate_EndDate",
                schema: "Ancillary",
                table: "ProvisionPermittedTravelPeriods",
                columns: new[] { "AncillaryProvisionId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPermittedTravelPeriods_ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionPermittedTravelPeriods",
                column: "ProvisionTravelDateRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSalesRestrictionsRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionSalesRestrictionsRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeatApplicationRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionSeatApplicationRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionServiceLocations_AncillaryProvisionId_LocationType_LocationId",
                schema: "Ancillary",
                table: "ProvisionServiceLocations",
                columns: new[] { "AncillaryProvisionId", "LocationType", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionServiceLocations_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionServiceLocations",
                column: "ProvisionGeographyRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionTravelDateRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionTravelDateRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionAircraft_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft",
                column: "ProvisionFlightApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFlightApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionAirFares_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionAirFareTypes_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionBlackoutPeriods_ProvisionTravelDateRules_ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods",
                column: "ProvisionTravelDateRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionTravelDateRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionCabinClasses_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionCustomers_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers",
                column: "ProvisionSalesRestrictionsRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionSalesRestrictionsRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionCustomerTypes_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes",
                column: "ProvisionSalesRestrictionsRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionSalesRestrictionsRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionDestinationAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports",
                column: "ProvisionGeographyRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionGeographyRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionFareBases_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionFareFamilies_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionFlightNumbers_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers",
                column: "ProvisionFlightApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFlightApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionFlights_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights",
                column: "ProvisionFlightApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFlightApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionMarketingAirlines_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines",
                column: "ProvisionFlightApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFlightApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionOperatingAirlines_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines",
                column: "ProvisionFlightApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFlightApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionOriginAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports",
                column: "ProvisionGeographyRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionGeographyRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionPassengerTypes_ProvisionPassengerEligibilityRules_ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes",
                column: "ProvisionPassengerEligibilityRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionPassengerEligibilityRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionPointsOfSale_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale",
                column: "ProvisionSalesRestrictionsRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionSalesRestrictionsRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionRbds_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds",
                column: "ProvisionFareApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionFareApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionRoutePairs_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                column: "ProvisionGeographyRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionGeographyRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionSeatCharacteristics_ProvisionSeatApplicationRules_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics",
                column: "ProvisionSeatApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionSeatApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionSeatNumbers_ProvisionSeatApplicationRules_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers",
                column: "ProvisionSeatApplicationRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionSeatApplicationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionViaAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports",
                column: "ProvisionGeographyRuleId",
                principalSchema: "Ancillary",
                principalTable: "ProvisionGeographyRules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE provision
                SET [SalesEffectiveFrom] = ruleRow.[SalesEffectiveFrom], [SalesDiscontinueAt] = ruleRow.[SalesDiscontinueAt]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                JOIN [Ancillary].[ProvisionSalesRestrictionsRules] AS ruleRow ON ruleRow.[AncillaryProvisionId] = provision.[Id];
                """);

            migrationBuilder.Sql(
                """
                UPDATE provision
                SET [AdvancePurchasePeriod] = ruleRow.[MinimumPeriod], [AdvancePurchaseUnit] = ruleRow.[Unit]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                JOIN [Ancillary].[ProvisionAdvancePurchaseRules] AS ruleRow ON ruleRow.[AncillaryProvisionId] = provision.[Id];
                """);

            migrationBuilder.Sql(
                """
                UPDATE provision
                SET [BaggageFreePieces] = ruleRow.[FreePieces], [BaggageFirstExcessPiece] = ruleRow.[FirstExcessPiece], [BaggageLastExcessPiece] = ruleRow.[LastExcessPiece],
                    [BaggageWeight] = ruleRow.[Weight], [BaggageWeightUnit] = ruleRow.[WeightUnit], [BaggageTravelApplication] = ruleRow.[TravelApplication],
                    [BaggagePurchaseApplication] = ruleRow.[PurchaseApplication], [BaggageRuleDeference] = ruleRow.[RuleDeference]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                JOIN [Ancillary].[ProvisionBaggageApplicationRules] AS ruleRow ON ruleRow.[AncillaryProvisionId] = provision.[Id];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSeasonalPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
                SELECT permitted.[Id], permitted.[AncillaryProvisionId], permitted.[StartDate], permitted.[EndDate], permitted.[LastUpdateTime], permitted.[LastUpdatedBy]
                FROM [Ancillary].[ProvisionPermittedTravelPeriods] AS permitted
                WHERE NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionRuleMigrationAudit] AS audit WHERE audit.[AncillaryProvisionId] = permitted.[AncillaryProvisionId])
                  AND NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS season WHERE season.[Id] = permitted.[Id]);
                """);

            migrationBuilder.Sql(
                """
                DELETE marker
                FROM [Ancillary].[ProvisionBlackoutPeriods] AS marker
                WHERE marker.[Id] = marker.[AncillaryProvisionId] AND marker.[StartDate] = '0001-01-01' AND marker.[EndDate] = '9999-12-31';
                """);

            migrationBuilder.DropTable(
                name: "ProvisionRuleMigrationAudit",
                schema: "Ancillary");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionAircraft_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionAirFares_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionAirFareTypes_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionBlackoutPeriods_ProvisionTravelDateRules_ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionCabinClasses_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionCustomers_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionCustomerTypes_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionDestinationAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionFareBases_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionFareFamilies_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionFlightNumbers_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionFlights_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionMarketingAirlines_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionOperatingAirlines_ProvisionFlightApplicationRules_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionOriginAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionPassengerTypes_ProvisionPassengerEligibilityRules_ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionPointsOfSale_ProvisionSalesRestrictionsRules_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionRbds_ProvisionFareApplicationRules_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionRoutePairs_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionSeatCharacteristics_ProvisionSeatApplicationRules_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionSeatNumbers_ProvisionSeatApplicationRules_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionViaAirports_ProvisionGeographyRules_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports");

            migrationBuilder.DropTable(
                name: "ProvisionAdvancePurchaseRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionBaggageApplicationRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionCoverageCountries",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionDayTimeWindows",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionEligibleAgeBands",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFareApplicationRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFlightApplicationRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPermittedTravelPeriods",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSalesRestrictionsRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSeatApplicationRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionServiceLocations",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionDayTimeApplicationRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPassengerEligibilityRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionTravelDateRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionGeographyRules",
                schema: "Ancillary");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionViaAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionSeatNumbers_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionSeatCharacteristics_ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionRoutePairs_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionRbds_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionPointsOfSale_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionPassengerTypes_ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionOriginAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionOperatingAirlines_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionMarketingAirlines_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionFlights_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionFlightNumbers_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionFareFamilies_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionFareBases_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionDestinationAirports_ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionCustomerTypes_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionCustomers_ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionCabinClasses_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionBlackoutPeriods_ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionAirFareTypes_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionAirFares_ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionAircraft_ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft");

            migrationBuilder.DropColumn(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionViaAirports");

            migrationBuilder.DropColumn(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers");

            migrationBuilder.DropColumn(
                name: "ProvisionSeatApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics");

            migrationBuilder.DropColumn(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionRbds");

            migrationBuilder.DropColumn(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale");

            migrationBuilder.DropColumn(
                name: "ProvisionPassengerEligibilityRuleId",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes");

            migrationBuilder.DropColumn(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports");

            migrationBuilder.DropColumn(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines");

            migrationBuilder.DropColumn(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines");

            migrationBuilder.DropColumn(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlights");

            migrationBuilder.DropColumn(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionFareBases");

            migrationBuilder.DropColumn(
                name: "ProvisionGeographyRuleId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports");

            migrationBuilder.DropColumn(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes");

            migrationBuilder.DropColumn(
                name: "ProvisionSalesRestrictionsRuleId",
                schema: "Ancillary",
                table: "ProvisionCustomers");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses");

            migrationBuilder.DropColumn(
                name: "ProvisionTravelDateRuleId",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes");

            migrationBuilder.DropColumn(
                name: "ProvisionFareApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAirFares");

            migrationBuilder.DropColumn(
                name: "ProvisionFlightApplicationRuleId",
                schema: "Ancillary",
                table: "ProvisionAircraft");

            migrationBuilder.DropColumn(
                name: "ServiceDateBasis",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");
        }
    }
}
