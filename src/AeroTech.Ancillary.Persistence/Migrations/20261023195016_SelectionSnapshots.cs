using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SelectionSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelectionSnapshots",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(24)", maxLength: 256, nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    PricingRateId = table.Column<long>(type: "bigint", nullable: false),
                    CoverageScope = table.Column<short>(type: "smallint", nullable: false),
                    TravellerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CoverageRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfferBinding = table.Column<string>(type: "char(43)", maxLength: 256, nullable: false),
                    SalesBinding = table.Column<string>(type: "char(43)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SelectedJson = table.Column<string>(type: "nvarchar(max)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionSnapshots", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelectionSnapshots",
                schema: "Ancillary");
        }
    }
}
