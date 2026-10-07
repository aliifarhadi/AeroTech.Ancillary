using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class InitialAncillaryQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ReadModel");

            migrationBuilder.CreateTable(
                name: "AncillaryProvisionPriceLines",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionPriceLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProvisions",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SalesEffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SalesDiscontinueAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoverageScope = table.Column<int>(type: "int", nullable: false),
                    QuantityUnit = table.Column<int>(type: "int", nullable: false),
                    MinQuantity = table.Column<int>(type: "int", nullable: false),
                    MaxQuantity = table.Column<int>(type: "int", nullable: false),
                    ApplicationType = table.Column<int>(type: "int", nullable: false),
                    Disposition = table.Column<int>(type: "int", nullable: false),
                    DocumentRequired = table.Column<bool>(type: "bit", nullable: false),
                    BookingRequired = table.Column<bool>(type: "bit", nullable: false),
                    FeeCurrencyId = table.Column<int>(type: "int", nullable: true),
                    FeeApplicationUnit = table.Column<int>(type: "int", nullable: true),
                    ReissueRefund = table.Column<int>(type: "int", nullable: false),
                    FormOfRefund = table.Column<int>(type: "int", nullable: true),
                    Commissionable = table.Column<bool>(type: "bit", nullable: false),
                    InterlineSettlement = table.Column<bool>(type: "bit", nullable: false),
                    MustCheckAvailability = table.Column<bool>(type: "bit", nullable: false),
                    FulfillmentProviderKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryServiceDefinitions",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceDefinitionRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    ServiceSubCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SubCodeSource = table.Column<int>(type: "int", nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SubGroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description1Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description2Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CommercialName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentRfic = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DocumentRfisc = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    BookingMethod = table.Column<int>(type: "int", nullable: false),
                    BookingSsrCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    BookingSsimCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    SalesEffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    SalesDiscontinueOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryServiceDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FulfillmentKind = table.Column<int>(type: "int", nullable: false),
                    FulfillmentProviderKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionPriceLines_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionPriceLines",
                column: "AncillaryProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisions_ServiceDefinitionId_Status",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                columns: new[] { "ServiceDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_OwnerAirlineId_Status",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                columns: new[] { "OwnerAirlineId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_SupplierId",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryProvisionPriceLines",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryProvisions",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryServiceDefinitions",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "ReadModel");
        }
    }
}
