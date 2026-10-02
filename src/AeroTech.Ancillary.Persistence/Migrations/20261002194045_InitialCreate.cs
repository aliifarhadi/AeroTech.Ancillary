using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Ancillary");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AncillaryPriceRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    ProductRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    SalesFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SalesTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TravelFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    TravelTo = table.Column<DateOnly>(type: "date", nullable: true),
                    PassengerTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginAirportIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationAirportIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryPriceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryProducts",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    ProductRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SalesScope = table.Column<int>(type: "int", nullable: false),
                    QuantityUnit = table.Column<int>(type: "int", nullable: false),
                    QuantityMin = table.Column<int>(type: "int", nullable: false),
                    QuantityMax = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Rfisc = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Rfic = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ServiceTypeCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SubGroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description1Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description2Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Refundable = table.Column<bool>(type: "bit", nullable: false),
                    Commissionable = table.Column<bool>(type: "bit", nullable: true),
                    Reusable = table.Column<bool>(type: "bit", nullable: true),
                    FormOfRefundCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InterlineSettlementAllowed = table.Column<bool>(type: "bit", nullable: true),
                    InventoryControl = table.Column<int>(type: "int", nullable: false),
                    BaggagePieces = table.Column<int>(type: "int", nullable: true),
                    BaggageWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BaggageWeightUnit = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "dbo",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Consumer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReceivedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => new { x.MessageId, x.Consumer });
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", maxLength: 256, nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Rfic = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SubGroupCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description1Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Description2Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CommercialName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryPriceRuleId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceLines_AncillaryPriceRules_AncillaryPriceRuleId",
                        column: x => x.AncillaryPriceRuleId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryPriceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPriceRules_CurrencyId_Status",
                schema: "Ancillary",
                table: "AncillaryPriceRules",
                columns: new[] { "CurrencyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryPriceRules_OwnerAirlineId_ProductRef_CurrencyId_Priority",
                schema: "Ancillary",
                table: "AncillaryPriceRules",
                columns: new[] { "OwnerAirlineId", "ProductRef", "CurrencyId", "Priority" },
                unique: true,
                filter: "[Status] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProducts_OwnerAirlineId_ProductRef_Draft",
                schema: "Ancillary",
                table: "AncillaryProducts",
                columns: new[] { "OwnerAirlineId", "ProductRef" },
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProducts_OwnerAirlineId_ProductRef_Offered",
                schema: "Ancillary",
                table: "AncillaryProducts",
                columns: new[] { "OwnerAirlineId", "ProductRef" },
                unique: true,
                filter: "[Status] IN (2, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProducts_OwnerAirlineId_ProductRef_Version",
                schema: "Ancillary",
                table: "AncillaryProducts",
                columns: new[] { "OwnerAirlineId", "ProductRef", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProducts_Status",
                schema: "Ancillary",
                table: "AncillaryProducts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_ReceivedOn",
                schema: "dbo",
                table: "InboxMessages",
                column: "ReceivedOn");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn",
                schema: "dbo",
                table: "OutboxMessages",
                column: "ProcessedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLines_AncillaryPriceRuleId",
                schema: "Ancillary",
                table: "PriceLines",
                column: "AncillaryPriceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSubCodes_OwnerAirlineId_Code",
                schema: "Ancillary",
                table: "ServiceSubCodes",
                columns: new[] { "OwnerAirlineId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryProducts",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PriceLines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ServiceSubCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryPriceRules",
                schema: "Ancillary");
        }
    }
}
