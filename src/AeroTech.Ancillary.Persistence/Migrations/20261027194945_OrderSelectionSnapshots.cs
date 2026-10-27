using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderSelectionSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OfferBinding",
                schema: "Ancillary",
                table: "SelectionSnapshotLines",
                newName: "SourceBinding");

            migrationBuilder.AlterColumn<string>(
                name: "OfferId",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<int>(
                name: "OrderCommercialVersion",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCommercialVersion",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.DropColumn(
                name: "OrderId",
                schema: "Ancillary",
                table: "SelectionSnapshots");

            migrationBuilder.RenameColumn(
                name: "SourceBinding",
                schema: "Ancillary",
                table: "SelectionSnapshotLines",
                newName: "OfferBinding");

            migrationBuilder.AlterColumn<string>(
                name: "OfferId",
                schema: "Ancillary",
                table: "SelectionSnapshots",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
