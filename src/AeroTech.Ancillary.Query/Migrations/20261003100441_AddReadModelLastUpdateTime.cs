using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class AddReadModelLastUpdateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdateTime",
                schema: "ReadModel",
                table: "AncillaryProducts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdateTime",
                schema: "ReadModel",
                table: "AncillaryPriceRules",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql("UPDATE [ReadModel].[AncillaryProducts] SET [LastUpdateTime] = [CreatedAt];");

            migrationBuilder.Sql("UPDATE [ReadModel].[AncillaryPriceRules] SET [LastUpdateTime] = [CreatedAt];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                schema: "ReadModel",
                table: "AncillaryProducts");

            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                schema: "ReadModel",
                table: "AncillaryPriceRules");
        }
    }
}
