using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V12Phase1NormalizedProvisionAndPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProvisionRoutePairs_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs");

            migrationBuilder.AddColumn<int>(
                name: "PricingUnit",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryPricings",
                schema: "Ancillary",
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
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryPricings_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionAircraft",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAircraft", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAircraft_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionAirFares",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirFareId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAirFares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAirFares_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionAirFareTypes",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirFareType = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAirFareTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAirFareTypes_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionBlackoutPeriods",
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
                    table.PrimaryKey("PK_ProvisionBlackoutPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionBlackoutPeriods_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionCabinClasses",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CabinClassId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionCabinClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionCabinClasses_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionCustomers",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionCustomers_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionCustomerTypes",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionCustomerTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionCustomerTypes_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "ProvisionDestinationAirports",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionDestinationAirports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionDestinationAirports_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFareBases",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FareBasisCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFareBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFareBases_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFareFamilies",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FareFamilyId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFareFamilies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFareFamilies_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFlightNumbers",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFlightNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFlightNumbers_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionFlights",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    FlightId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionFlights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionFlights_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionMarketingAirlines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirlineId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionMarketingAirlines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionMarketingAirlines_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionOperatingAirlines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirlineId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionOperatingAirlines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionOperatingAirlines_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionOriginAirports",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionOriginAirports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionOriginAirports_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPassengerTypes",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPassengerTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPassengerTypes_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPointsOfSale",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    PointOfSaleId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPointsOfSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPointsOfSale_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionRbds",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    RbdId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionRbds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionRbds_AncillaryProvisions_AncillaryProvisionId",
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
                name: "ProvisionSeatCharacteristics",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CharacteristicCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionSeatCharacteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionSeatCharacteristics_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionSeatNumbers",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionSeatNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionSeatNumbers_AncillaryProvisions_AncillaryProvisionId",
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

            migrationBuilder.CreateTable(
                name: "ProvisionViaAirports",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionViaAirports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionViaAirports_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryPricingLines",
                schema: "Ancillary",
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
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricingLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryPricingLines_AncillaryPricings_AncillaryPricingId",
                        column: x => x.AncillaryPricingId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryPricings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionRoutePairs_AncillaryProvisionId_OriginAirportId_DestinationAirportId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                columns: new[] { "AncillaryProvisionId", "OriginAirportId", "DestinationAirportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricingLines_AncillaryPricingId",
                schema: "Ancillary",
                table: "AncillaryPricingLines",
                column: "AncillaryPricingId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricings_AncillaryProvisionId_Version",
                schema: "Ancillary",
                table: "AncillaryPricings",
                columns: new[] { "AncillaryProvisionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricings_OneActivePerProvision",
                schema: "Ancillary",
                table: "AncillaryPricings",
                columns: new[] { "AncillaryProvisionId", "Status" },
                unique: true,
                filter: "[Status] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricings_Status",
                schema: "Ancillary",
                table: "AncillaryPricings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAircraft_AncillaryProvisionId_AircraftId",
                schema: "Ancillary",
                table: "ProvisionAircraft",
                columns: new[] { "AncillaryProvisionId", "AircraftId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAirFares_AncillaryProvisionId_AirFareId",
                schema: "Ancillary",
                table: "ProvisionAirFares",
                columns: new[] { "AncillaryProvisionId", "AirFareId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAirFareTypes_AncillaryProvisionId_AirFareType",
                schema: "Ancillary",
                table: "ProvisionAirFareTypes",
                columns: new[] { "AncillaryProvisionId", "AirFareType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionBlackoutPeriods_AncillaryProvisionId_StartDate_EndDate",
                schema: "Ancillary",
                table: "ProvisionBlackoutPeriods",
                columns: new[] { "AncillaryProvisionId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCabinClasses_AncillaryProvisionId_CabinClassId",
                schema: "Ancillary",
                table: "ProvisionCabinClasses",
                columns: new[] { "AncillaryProvisionId", "CabinClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCustomers_AncillaryProvisionId_CustomerId",
                schema: "Ancillary",
                table: "ProvisionCustomers",
                columns: new[] { "AncillaryProvisionId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionCustomerTypes_AncillaryProvisionId_CustomerType",
                schema: "Ancillary",
                table: "ProvisionCustomerTypes",
                columns: new[] { "AncillaryProvisionId", "CustomerType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDayTimeRestrictions_AncillaryProvisionId_DayOfWeek_StartTime_EndTime_Effect",
                schema: "Ancillary",
                table: "ProvisionDayTimeRestrictions",
                columns: new[] { "AncillaryProvisionId", "DayOfWeek", "StartTime", "EndTime", "Effect" },
                unique: true,
                filter: "[StartTime] IS NOT NULL AND [EndTime] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionDestinationAirports_AncillaryProvisionId_AirportId",
                schema: "Ancillary",
                table: "ProvisionDestinationAirports",
                columns: new[] { "AncillaryProvisionId", "AirportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFareBases_AncillaryProvisionId_FareBasisCode",
                schema: "Ancillary",
                table: "ProvisionFareBases",
                columns: new[] { "AncillaryProvisionId", "FareBasisCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFareFamilies_AncillaryProvisionId_FareFamilyId",
                schema: "Ancillary",
                table: "ProvisionFareFamilies",
                columns: new[] { "AncillaryProvisionId", "FareFamilyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFlightNumbers_AncillaryProvisionId_FlightNumber",
                schema: "Ancillary",
                table: "ProvisionFlightNumbers",
                columns: new[] { "AncillaryProvisionId", "FlightNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionFlights_AncillaryProvisionId_FlightId",
                schema: "Ancillary",
                table: "ProvisionFlights",
                columns: new[] { "AncillaryProvisionId", "FlightId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionMarketingAirlines_AncillaryProvisionId_AirlineId",
                schema: "Ancillary",
                table: "ProvisionMarketingAirlines",
                columns: new[] { "AncillaryProvisionId", "AirlineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionOperatingAirlines_AncillaryProvisionId_AirlineId",
                schema: "Ancillary",
                table: "ProvisionOperatingAirlines",
                columns: new[] { "AncillaryProvisionId", "AirlineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionOriginAirports_AncillaryProvisionId_AirportId",
                schema: "Ancillary",
                table: "ProvisionOriginAirports",
                columns: new[] { "AncillaryProvisionId", "AirportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPassengerTypes_AncillaryProvisionId_PassengerTypeCode",
                schema: "Ancillary",
                table: "ProvisionPassengerTypes",
                columns: new[] { "AncillaryProvisionId", "PassengerTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPointsOfSale_AncillaryProvisionId_PointOfSaleId",
                schema: "Ancillary",
                table: "ProvisionPointsOfSale",
                columns: new[] { "AncillaryProvisionId", "PointOfSaleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionRbds_AncillaryProvisionId_RbdId",
                schema: "Ancillary",
                table: "ProvisionRbds",
                columns: new[] { "AncillaryProvisionId", "RbdId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeasonalPeriods_AncillaryProvisionId_StartDate_EndDate",
                schema: "Ancillary",
                table: "ProvisionSeasonalPeriods",
                columns: new[] { "AncillaryProvisionId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeatCharacteristics_AncillaryProvisionId_CharacteristicCode",
                schema: "Ancillary",
                table: "ProvisionSeatCharacteristics",
                columns: new[] { "AncillaryProvisionId", "CharacteristicCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionSeatNumbers_AncillaryProvisionId_SeatNumber",
                schema: "Ancillary",
                table: "ProvisionSeatNumbers",
                columns: new[] { "AncillaryProvisionId", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionTravelDates_AncillaryProvisionId_TravelDate",
                schema: "Ancillary",
                table: "ProvisionTravelDates",
                columns: new[] { "AncillaryProvisionId", "TravelDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionViaAirports_AncillaryProvisionId_AirportId",
                schema: "Ancillary",
                table: "ProvisionViaAirports",
                columns: new[] { "AncillaryProvisionId", "AirportId" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionPassengerTypes] ([Id], [AncillaryProvisionId], [PassengerTypeCode], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[PassengerTypeCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionPointsOfSale] ([Id], [AncillaryProvisionId], [PointOfSaleId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[PointOfSaleIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionCustomers] ([Id], [AncillaryProvisionId], [CustomerId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CustomerIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionCustomerTypes] ([Id], [AncillaryProvisionId], [CustomerType], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CustomerTypes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionOriginAirports] ([Id], [AncillaryProvisionId], [AirportId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[OriginAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionDestinationAirports] ([Id], [AncillaryProvisionId], [AirportId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[DestinationAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionViaAirports] ([Id], [AncillaryProvisionId], [AirportId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[ViaAirportIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionMarketingAirlines] ([Id], [AncillaryProvisionId], [AirlineId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[MarketingAirlineIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionOperatingAirlines] ([Id], [AncillaryProvisionId], [AirlineId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[OperatingAirlineIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFlightNumbers] ([Id], [AncillaryProvisionId], [FlightNumber], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(16)), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FlightNumbers]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFlights] ([Id], [AncillaryProvisionId], [FlightId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FlightIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionAircraft] ([Id], [AncillaryProvisionId], [AircraftId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AircraftIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionAirFares] ([Id], [AncillaryProvisionId], [AirFareId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AirFareIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionAirFareTypes] ([Id], [AncillaryProvisionId], [AirFareType], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[AirFareTypes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFareFamilies] ([Id], [AncillaryProvisionId], [FareFamilyId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FareFamilyIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionFareBases] ([Id], [AncillaryProvisionId], [FareBasisCode], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(64)), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[FareBasisCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionCabinClasses] ([Id], [AncillaryProvisionId], [CabinClassId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS int), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[CabinClassIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionRbds] ([Id], [AncillaryProvisionId], [RbdId], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(item.[value] AS bigint), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[RbdIds]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSeatNumbers] ([Id], [AncillaryProvisionId], [SeatNumber], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(16)), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[SeatNumbers]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSeatCharacteristics] ([Id], [AncillaryProvisionId], [CharacteristicCode], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], CAST(item.[key] AS int)), provision.[Id], CAST(UPPER(LTRIM(RTRIM(item.[value]))) AS nvarchar(25)), provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                CROSS APPLY OPENJSON(provision.[SeatCharacteristicCodes]) AS item;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionSeasonalPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id]), provision.[Id], provision.[TravelFrom], provision.[TravelTo], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE provision.[TravelFrom] IS NOT NULL AND provision.[TravelTo] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[ProvisionDayTimeRestrictions] ([Id], [AncillaryProvisionId], [DayOfWeek], [StartTime], [EndTime], [Effect], [LastUpdateTime], [LastUpdatedBy])
                SELECT ROW_NUMBER() OVER (ORDER BY provision.[Id], weekday.[DayOfWeek]), provision.[Id], weekday.[DayOfWeek], provision.[TimeFrom], provision.[TimeTo], 1, provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
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
                INSERT INTO [Ancillary].[AncillaryPricings] ([Id], [AncillaryProvisionId], [PricingUnit], [Version], [CurrencyId], [FeeApplicationUnit], [Status], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime], [LastUpdatedBy])
                SELECT provision.[Id], provision.[Id], NULL, 1, provision.[FeeCurrencyId], provision.[FeeApplicationUnit],
                       CASE provision.[Status] WHEN 1 THEN 1 WHEN 4 THEN 4 ELSE 2 END,
                       provision.[CreatedAt], provision.[ActivatedAt], NULL, provision.[RetiredAt], provision.[LastUpdateTime], provision.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryProvisions] AS provision
                WHERE provision.[Disposition] = 1 AND provision.[FeeCurrencyId] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[AncillaryPricingLines] ([Id], [AncillaryPricingId], [PassengerTypeCode], [AgeFromInclusive], [AgeToExclusive], [Category], [Code], [Name], [CountryId], [StationAirportId], [Amount], [LastUpdateTime], [LastUpdatedBy])
                SELECT line.[Id], line.[AncillaryProvisionId], NULL, NULL, NULL, line.[Category], line.[Code], line.[Name], line.[CountryId], line.[StationAirportId], line.[UnitAmount], line.[LastUpdateTime], line.[LastUpdatedBy]
                FROM [Ancillary].[ProvisionPriceLines] AS line
                WHERE EXISTS (SELECT 1 FROM [Ancillary].[AncillaryPricings] AS pricing WHERE pricing.[Id] = line.[AncillaryProvisionId]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryPricingLines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionAircraft",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionAirFares",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionAirFareTypes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionBlackoutPeriods",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionCabinClasses",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionCustomers",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionCustomerTypes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionDayTimeRestrictions",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionDestinationAirports",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFareBases",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFareFamilies",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFlightNumbers",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionFlights",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionMarketingAirlines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionOperatingAirlines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionOriginAirports",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPassengerTypes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPointsOfSale",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionRbds",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSeasonalPeriods",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSeatCharacteristics",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionSeatNumbers",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionTravelDates",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionViaAirports",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryPricings",
                schema: "Ancillary");

            migrationBuilder.DropIndex(
                name: "IX_ProvisionRoutePairs_AncillaryProvisionId_OriginAirportId_DestinationAirportId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs");

            migrationBuilder.DropColumn(
                name: "PricingUnit",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionRoutePairs_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionRoutePairs",
                column: "AncillaryProvisionId");
        }
    }
}
