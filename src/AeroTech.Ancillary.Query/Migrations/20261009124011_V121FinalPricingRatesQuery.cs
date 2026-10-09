using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V121FinalPricingRatesQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AncillaryPriceComponents",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingRateId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    FeeApplicationUnit = table.Column<int>(type: "int", nullable: true),
                    TaxIncludedInSource = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPriceComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryPricingRates",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPricingId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: true),
                    AgeFromInclusive = table.Column<int>(type: "int", nullable: true),
                    AgeToExclusive = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricingRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPriceComponents_AncillaryPricingId",
                schema: "ReadModel",
                table: "AncillaryPriceComponents",
                column: "AncillaryPricingId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPriceComponents_AncillaryPricingRateId",
                schema: "ReadModel",
                table: "AncillaryPriceComponents",
                column: "AncillaryPricingRateId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricingRates_AncillaryPricingId",
                schema: "ReadModel",
                table: "AncillaryPricingRates",
                column: "AncillaryPricingId");

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryPricingRates] ([Id], [AncillaryPricingId], [PassengerTypeCode], [AgeFromInclusive], [AgeToExclusive], [CurrencyId], [BaseAmount])
                SELECT line.[Id], line.[AncillaryPricingId], line.[PassengerTypeCode], line.[AgeFromInclusive], line.[AgeToExclusive], pricing.[CurrencyId], line.[Amount]
                FROM [ReadModel].[AncillaryPricingLines] AS line
                INNER JOIN [ReadModel].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId]
                WHERE line.[Category] = 1;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ReadModel].[AncillaryPriceComponents] ([Id], [AncillaryPricingId], [AncillaryPricingRateId], [Category], [Code], [Name], [CountryId], [StationAirportId], [Amount], [CurrencyId], [FeeApplicationUnit], [TaxIncludedInSource])
                SELECT line.[Id], line.[AncillaryPricingId], base.[Id], line.[Category], line.[Code], line.[Name], line.[CountryId], line.[StationAirportId], line.[Amount], pricing.[CurrencyId],
                       CASE WHEN line.[Category] = 3 THEN pricing.[FeeApplicationUnit] END, NULL
                FROM [ReadModel].[AncillaryPricingLines] AS line
                INNER JOIN [ReadModel].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId]
                INNER JOIN [ReadModel].[AncillaryPricingLines] AS base
                    ON base.[AncillaryPricingId] = line.[AncillaryPricingId]
                   AND base.[Category] = 1
                   AND ISNULL(base.[PassengerTypeCode], -1) = ISNULL(line.[PassengerTypeCode], -1)
                   AND ISNULL(base.[AgeFromInclusive], -1) = ISNULL(line.[AgeFromInclusive], -1)
                   AND ISNULL(base.[AgeToExclusive], -1) = ISNULL(line.[AgeToExclusive], -1)
                WHERE line.[Category] IN (2, 3);
                """);

            migrationBuilder.DropTable(
                name: "AncillaryPricingLines",
                schema: "ReadModel");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "ReadModel",
                table: "AncillaryPricings");

            migrationBuilder.DropColumn(
                name: "FeeApplicationUnit",
                schema: "ReadModel",
                table: "AncillaryPricings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryPriceComponents",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryPricingRates",
                schema: "ReadModel");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "ReadModel",
                table: "AncillaryPricings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeeApplicationUnit",
                schema: "ReadModel",
                table: "AncillaryPricings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AncillaryPricingLines",
                schema: "ReadModel",
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
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: true),
                    StationAirportId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPricingLines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPricingLines_AncillaryPricingId",
                schema: "ReadModel",
                table: "AncillaryPricingLines",
                column: "AncillaryPricingId");
        }
    }
}
