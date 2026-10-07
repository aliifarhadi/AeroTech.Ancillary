using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAncillary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Ancillary");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AncillaryProvisions",
                schema: "Ancillary",
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
                    ActivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SuspendedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservations",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RequestedExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryServiceDefinitions",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
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
                    SuspendedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryServiceDefinitions", x => x.Id);
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
                name: "Suppliers",
                schema: "Ancillary",
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
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPriceLines",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPriceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPriceLines_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryReservationUnits",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryReservationId = table.Column<long>(type: "bigint", nullable: false),
                    OrderServiceId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    TravellerId = table.Column<long>(type: "bigint", nullable: true),
                    CoverageScope = table.Column<int>(type: "int", nullable: false),
                    CoveredFlightIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockPoolId = table.Column<long>(type: "bigint", nullable: true),
                    ProviderUnitRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CancellationReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryReservationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryReservationUnits_AncillaryReservations_AncillaryReservationId",
                        column: x => x.AncillaryReservationId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisions_ServiceDefinitionId",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                column: "ServiceDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisions_ServiceDefinitionId_Sequence_Active",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                columns: new[] { "ServiceDefinitionId", "Sequence" },
                unique: true,
                filter: "[Status] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisions_Status",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservations_IdempotencyKey",
                schema: "Ancillary",
                table: "AncillaryReservations",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservations_OrderId",
                schema: "Ancillary",
                table: "AncillaryReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservationUnits_AncillaryReservationId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                column: "AncillaryReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryReservationUnits_OrderServiceId",
                schema: "Ancillary",
                table: "AncillaryReservationUnits",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_OwnerAirlineId_Ref_Active",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                columns: new[] { "OwnerAirlineId", "ServiceDefinitionRef" },
                unique: true,
                filter: "[Status] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_OwnerAirlineId_ServiceDefinitionRef_Version",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                columns: new[] { "OwnerAirlineId", "ServiceDefinitionRef", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_Status",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryServiceDefinitions_SupplierId",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                column: "SupplierId");

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
                name: "IX_ProvisionPriceLines_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionPriceLines",
                column: "AncillaryProvisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryReservationUnits",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryServiceDefinitions",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProvisionPriceLines",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryReservations",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryProvisions",
                schema: "Ancillary");
        }
    }
}
