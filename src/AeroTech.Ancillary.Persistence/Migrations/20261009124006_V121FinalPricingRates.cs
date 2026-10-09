using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V121FinalPricingRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AncillaryPricingRates",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: true),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: true),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricingRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryPricingRates_AncillaryPricings_AncillaryPricingId",
                        column: x => x.AncillaryPricingId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryPricings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryPriceComponents",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingRateId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    FeeApplicationUnit = table.Column<int>(type: "int", nullable: true),
                    TaxIncludedInSource = table.Column<bool>(type: "bit", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPriceComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryPriceComponents_AncillaryPricingRates_AncillaryPricingRateId",
                        column: x => x.AncillaryPricingRateId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryPricingRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPriceComponents_ComponentKey",
                schema: "Ancillary",
                table: "AncillaryPriceComponents",
                columns: new[] { "AncillaryPricingRateId", "Category", "Code", "CountryId", "StationAirportId", "FeeApplicationUnit" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricingRates_RateKey",
                schema: "Ancillary",
                table: "AncillaryPricingRates",
                columns: new[] { "AncillaryPricingId", "CurrencyId", "PassengerTypeCode", "AgeFromInclusive", "AgeToExclusive" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[AncillaryPricingRates] ([Id], [AncillaryPricingId], [PassengerTypeCode], [AgeFromInclusive], [AgeToExclusive], [CurrencyId], [BaseAmount], [LastUpdateTime], [LastUpdatedBy])
                SELECT line.[Id], line.[AncillaryPricingId], line.[PassengerTypeCode], line.[AgeFromInclusive], line.[AgeToExclusive], pricing.[CurrencyId], line.[Amount], line.[LastUpdateTime], line.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryPricingLines] AS line
                INNER JOIN [Ancillary].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId]
                WHERE line.[Category] = 1;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [Ancillary].[AncillaryPriceComponents] ([Id], [AncillaryPricingRateId], [Category], [Code], [Name], [CountryId], [StationAirportId], [Amount], [CurrencyId], [FeeApplicationUnit], [TaxIncludedInSource], [LastUpdateTime], [LastUpdatedBy])
                SELECT line.[Id], base.[Id], line.[Category], line.[Code], line.[Name], line.[CountryId], line.[StationAirportId], line.[Amount], pricing.[CurrencyId],
                       CASE WHEN line.[Category] = 3 THEN pricing.[FeeApplicationUnit] END, NULL, line.[LastUpdateTime], line.[LastUpdatedBy]
                FROM [Ancillary].[AncillaryPricingLines] AS line
                INNER JOIN [Ancillary].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId]
                INNER JOIN [Ancillary].[AncillaryPricingLines] AS base
                    ON base.[AncillaryPricingId] = line.[AncillaryPricingId]
                   AND base.[Category] = 1
                   AND ISNULL(base.[PassengerTypeCode], -1) = ISNULL(line.[PassengerTypeCode], -1)
                   AND ISNULL(base.[AgeFromInclusive], -1) = ISNULL(line.[AgeFromInclusive], -1)
                   AND ISNULL(base.[AgeToExclusive], -1) = ISNULL(line.[AgeToExclusive], -1)
                WHERE line.[Category] IN (2, 3);
                """);

            migrationBuilder.DropTable(
                name: "AncillaryPricingLines",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Ancillary",
                table: "AncillaryPricings");

            migrationBuilder.DropColumn(
                name: "FeeApplicationUnit",
                schema: "Ancillary",
                table: "AncillaryPricings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryPriceComponents",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryPricingRates",
                schema: "Ancillary");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Ancillary",
                table: "AncillaryPricings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeeApplicationUnit",
                schema: "Ancillary",
                table: "AncillaryPricings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryPricingLines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: true),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AncillaryPricingId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_AncillaryPricingLines_AncillaryPricingId",
                schema: "Ancillary",
                table: "AncillaryPricingLines",
                column: "AncillaryPricingId");
        }
    }
}
