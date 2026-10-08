using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V12Phase1NormalizedProvisionAndPricingQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PricingUnit",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryPricingLines",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: true),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: true),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricingLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryPricings",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    PricingUnit = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    FeeApplicationUnit = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SuspendedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionAircraft",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionAircraft", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionAirFares",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirFareId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionAirFares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionAirFareTypes",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirFareType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionAirFareTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionBlackoutPeriods",
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
                    table.PrimaryKey("PK_AncillaryProvisionBlackoutPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionCabinClasses",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CabinClassId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionCabinClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionCustomers",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionCustomerTypes",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionCustomerTypes", x => x.Id);
                });

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
                name: "AncillaryProvisionDestinationAirports",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionDestinationAirports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionFareBases",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FareBasisCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionFareBases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionFareFamilies",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FareFamilyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionFareFamilies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionFlightNumbers",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionFlightNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionFlights",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FlightId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionFlights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionMarketingAirlines",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirlineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionMarketingAirlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionOperatingAirlines",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirlineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionOperatingAirlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionOriginAirports",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionOriginAirports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionPassengerTypes",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionPassengerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionPointsOfSale",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    PointOfSaleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionPointsOfSale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionRbds",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    RbdId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionRbds", x => x.Id);
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
                name: "AncillaryProvisionSeatCharacteristics",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CharacteristicCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionSeatCharacteristics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionSeatNumbers",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionSeatNumbers", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionViaAirports",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionViaAirports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricingLines_AncillaryPricingId",
                schema: "ReadModel",
                table: "AncillaryPricingLines",
                column: "AncillaryPricingId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricings_AncillaryProvisionId_Status",
                schema: "ReadModel",
                table: "AncillaryPricings",
                columns: new[] { "AncillaryProvisionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionAircraft_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionAircraft",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionAirFares_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionAirFares",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionAirFareTypes_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionAirFareTypes",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionBlackoutPeriods_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionBlackoutPeriods",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionCabinClasses_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionCabinClasses",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionCustomers_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionCustomers",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionCustomerTypes_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionCustomerTypes",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionDayTimeRestrictions_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionDayTimeRestrictions",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionDestinationAirports_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionDestinationAirports",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionFareBases_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionFareBases",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionFareFamilies_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionFareFamilies",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionFlightNumbers_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionFlightNumbers",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionFlights_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionFlights",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionMarketingAirlines_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionMarketingAirlines",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionOperatingAirlines_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionOperatingAirlines",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionOriginAirports_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionOriginAirports",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionPassengerTypes_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionPassengerTypes",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionPointsOfSale_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionPointsOfSale",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionRbds_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionRbds",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionSeasonalPeriods_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionSeasonalPeriods",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionSeatCharacteristics_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionSeatCharacteristics",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionSeatNumbers_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionSeatNumbers",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionTravelDates_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionTravelDates",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionViaAirports_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionViaAirports",
                column: "AncillaryProvisionId");

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionPassengerTypes] ([Id], [AncillaryProvisionId], [PassengerTypeCode])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[PassengerTypeCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionPointsOfSale] ([Id], [AncillaryProvisionId], [PointOfSaleId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[PointOfSaleIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionCustomers] ([Id], [AncillaryProvisionId], [CustomerId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CustomerIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionCustomerTypes] ([Id], [AncillaryProvisionId], [CustomerType])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CustomerTypes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionOriginAirports] ([Id], [AncillaryProvisionId], [AirportId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[OriginAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionDestinationAirports] ([Id], [AncillaryProvisionId], [AirportId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[DestinationAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionViaAirports] ([Id], [AncillaryProvisionId], [AirportId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[ViaAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionMarketingAirlines] ([Id], [AncillaryProvisionId], [AirlineId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[MarketingAirlineIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionOperatingAirlines] ([Id], [AncillaryProvisionId], [AirlineId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[OperatingAirlineIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionFlightNumbers] ([Id], [AncillaryProvisionId], [FlightNumber])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(16))
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FlightNumbers]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionFlights] ([Id], [AncillaryProvisionId], [FlightId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FlightIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionAircraft] ([Id], [AncillaryProvisionId], [AircraftId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AircraftIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionAirFares] ([Id], [AncillaryProvisionId], [AirFareId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AirFareIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionAirFareTypes] ([Id], [AncillaryProvisionId], [AirFareType])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AirFareTypes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionFareFamilies] ([Id], [AncillaryProvisionId], [FareFamilyId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FareFamilyIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionFareBases] ([Id], [AncillaryProvisionId], [FareBasisCode])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(64))
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FareBasisCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionCabinClasses] ([Id], [AncillaryProvisionId], [CabinClassId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CabinClassIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionRbds] ([Id], [AncillaryProvisionId], [RbdId])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint)
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[RbdIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionSeatNumbers] ([Id], [AncillaryProvisionId], [SeatNumber])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(16))
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[SeatNumbers]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionSeatCharacteristics] ([Id], [AncillaryProvisionId], [CharacteristicCode])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(25))
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[SeatCharacteristicCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionSeasonalPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id]), provision.[Id], provision.[TravelFrom], provision.[TravelTo]
                FROM [ReadModel].[AncillaryProvisions] AS provision
                WHERE provision.[TravelFrom] IS NOT NULL AND provision.[TravelTo] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryProvisionDayTimeRestrictions] ([Id], [AncillaryProvisionId], [DayOfWeek], [StartTime], [EndTime], [Effect])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], weekday.[DayOfWeek]), provision.[Id], weekday.[DayOfWeek], provision.[TimeFrom], provision.[TimeTo], 1
                FROM [ReadModel].[AncillaryProvisions] AS provision
                CROSS APPLY
                (
                    SELECT CAST(item.[value] AS int) AS [DayOfWeek]
                    FROM OPENJSON(provision.[DaysOfWeek]) AS item
                    UNION ALL
                    SELECT everyday.[DayOfWeek]
                    FROM (VALUES (0), (1), (2), (3), (4), (5), (6)) AS everyday([DayOfWeek])
                    WHERE provision.[DaysOfWeek] = N'[]' AND provision.[TimeFrom] IS NOT NULL
                ) AS weekday
                WHERE provision.[TimeFrom] IS NULL OR provision.[TimeFrom] <= provision.[TimeTo];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryPricings] ([Id], [AncillaryProvisionId], [PricingUnit], [Version], [CurrencyId], [FeeApplicationUnit], [Status], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime])
                SELECT provision.[Id], provision.[Id], NULL, 1, provision.[FeeCurrencyId], provision.[FeeApplicationUnit],
                       CASE provision.[Status] WHEN 1 THEN 1 WHEN 4 THEN 4 ELSE 2 END,
                       provision.[CreatedAt], provision.[ActivatedAt], NULL, provision.[RetiredAt], provision.[LastUpdateTime]
                FROM [ReadModel].[AncillaryProvisions] AS provision
                WHERE provision.[Disposition] = 1 AND provision.[FeeCurrencyId] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryPricingLines] ([Id], [AncillaryPricingId], [PassengerTypeCode], [AgeFromInclusive], [AgeToExclusive], [Category], [Code], [Name], [CountryId], [StationAirportId], [Amount])
                SELECT line.[Id], line.[AncillaryProvisionId], NULL, NULL, NULL, line.[Category], line.[Code], line.[Name], line.[CountryId], line.[StationAirportId], line.[UnitAmount]
                FROM [ReadModel].[AncillaryProvisionPriceLines] AS line
                WHERE EXISTS (SELECT 1 FROM [ReadModel].[AncillaryPricings] AS pricing WHERE pricing.[Id] = line.[AncillaryProvisionId]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryPricingLines",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryPricings",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionAircraft",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionAirFares",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionAirFareTypes",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionBlackoutPeriods",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionCabinClasses",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionCustomers",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionCustomerTypes",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionDayTimeRestrictions",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionDestinationAirports",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionFareBases",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionFareFamilies",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionFlightNumbers",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionFlights",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionMarketingAirlines",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionOperatingAirlines",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionOriginAirports",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionPassengerTypes",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionPointsOfSale",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionRbds",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionSeasonalPeriods",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionSeatCharacteristics",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionSeatNumbers",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionTravelDates",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisionViaAirports",
                schema: "ReadModel");

            migrationBuilder.DropColumn(
                name: "PricingUnit",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");
        }
    }
}
