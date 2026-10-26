using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SelectionSnapshotLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverageRef",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "CoverageScope",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "OfferBinding",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "PricingRateId",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "SalesBinding",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "SelectedJson",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "TravellerRef",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.AddColumn<string>(
                name: "OfferId",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SelectionSnapshotLines",
                schema: "Ancillary",
                columns: table => new
                {
                    SelectionSnapshotId = table.Column<string>(type: "varchar(24)", maxLength: 256, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    OfferItemId = table.Column<string>(type: "varchar(1024)", maxLength: 256, nullable: false),
                    PricingRateId = table.Column<long>(type: "bigint", nullable: false),
                    CoverageScope = table.Column<short>(type: "smallint", nullable: false),
                    TravellerRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CoverageRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfferBinding = table.Column<string>(type: "char(43)", maxLength: 256, nullable: false),
                    SalesBinding = table.Column<string>(type: "char(43)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SelectedJson = table.Column<string>(type: "nvarchar(max)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionSnapshotLines", x => new { x.SelectionSnapshotId, x.Sequence });
                    table.ForeignKey(
                        name: "FK_SelectionSnapshotLines_SelectionSnapshots_SelectionSnapshotId",
                        column: x => x.SelectionSnapshotId,
                        principalSchema: "Ancillary",
                        principalTable: "SelectionSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelectionSnapshotLines",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "OfferId",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.AddColumn<string>(
                name: "CoverageRef",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "CoverageScope",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "OfferBinding",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "char(43)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PricingRateId",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SalesBinding",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "char(43)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedJson",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(max)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TravellerRef",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
