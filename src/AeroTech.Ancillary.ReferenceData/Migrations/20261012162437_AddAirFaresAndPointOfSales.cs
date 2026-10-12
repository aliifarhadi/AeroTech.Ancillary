using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.ReferenceData.Migrations
{
    /// <inheritdoc />
    public partial class AddAirFaresAndPointOfSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirFares",
                schema: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareNumber = table.Column<long>(type: "bigint", nullable: false),
                    FareFamilyId = table.Column<long>(type: "bigint", nullable: true),
                    FareFamilyTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CabinClassId = table.Column<int>(type: "int", nullable: true),
                    RbdId = table.Column<long>(type: "bigint", nullable: true),
                    FareBasis = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirFares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointOfSaleConditions",
                schema: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PointOfSaleId = table.Column<long>(type: "bigint", nullable: false),
                    CountryIsInclude = table.Column<bool>(type: "bit", nullable: true),
                    CountryIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AirlineOfficeIsInclude = table.Column<bool>(type: "bit", nullable: true),
                    AirlineOfficeIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TravelAgencyOfficeIsInclude = table.Column<bool>(type: "bit", nullable: true),
                    TravelAgencyOfficeIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelIsInclude = table.Column<bool>(type: "bit", nullable: true),
                    Channels = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointOfSaleConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointOfSales",
                schema: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointOfSales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirFares_FareFamilyId",
                schema: "ReferenceData",
                table: "AirFares",
                column: "FareFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_PointOfSaleConditions_PointOfSaleId",
                schema: "ReferenceData",
                table: "PointOfSaleConditions",
                column: "PointOfSaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirFares",
                schema: "ReferenceData");

            migrationBuilder.DropTable(
                name: "PointOfSaleConditions",
                schema: "ReferenceData");

            migrationBuilder.DropTable(
                name: "PointOfSales",
                schema: "ReferenceData");
        }
    }
}
