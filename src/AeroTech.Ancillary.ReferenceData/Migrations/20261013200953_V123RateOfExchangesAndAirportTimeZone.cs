using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.ReferenceData.Migrations
{
    /// <inheritdoc />
    public partial class V123RateOfExchangesAndAirportTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "ReferenceData",
                table: "Airports",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RateOfExchanges",
                schema: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    RateOfExchangeTableId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TableCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Application = table.Column<int>(type: "int", nullable: false),
                    FromCurrencyId = table.Column<int>(type: "int", nullable: false),
                    ToCurrencyId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,10)", precision: 18, scale: 10, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateOfExchanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateOfExchanges_ToCurrencyId_Application",
                schema: "ReferenceData",
                table: "RateOfExchanges",
                columns: new[] { "ToCurrencyId", "Application" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateOfExchanges",
                schema: "ReferenceData");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                schema: "ReferenceData",
                table: "Airports");
        }
    }
}
