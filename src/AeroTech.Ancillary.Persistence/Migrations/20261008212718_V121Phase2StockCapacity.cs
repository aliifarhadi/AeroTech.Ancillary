using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V121Phase2StockCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirportSlotInventories",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<long>(type: "bigint", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CapacityPersons = table.Column<int>(type: "int", nullable: false),
                    ClosedForSale = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportSlotInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryInventoryPolicies",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    ServiceDefinitionRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Authority = table.Column<int>(type: "int", nullable: false),
                    LocalPattern = table.Column<int>(type: "int", nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountResourceId = table.Column<long>(type: "bigint", nullable: true),
                    CountPerAcceptedUnit = table.Column<int>(type: "int", nullable: true),
                    CountUnit = table.Column<int>(type: "int", nullable: true),
                    WeightResourceId = table.Column<long>(type: "bigint", nullable: true),
                    WeightConsumptionMode = table.Column<int>(type: "int", nullable: true),
                    WeightFixedKgPerUnit = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    SlotFacilityId = table.Column<long>(type: "bigint", nullable: true),
                    SlotOccupancyMinutes = table.Column<int>(type: "int", nullable: true),
                    SlotPeoplePerAcceptedUnit = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SuspendedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryInventoryPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AncillaryInventoryPolicies_AncillaryServiceDefinitions_ServiceDefinitionId",
                        column: x => x.ServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightCountInventories",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    FlightId = table.Column<long>(type: "bigint", nullable: false),
                    ResourceId = table.Column<long>(type: "bigint", nullable: false),
                    CountUnit = table.Column<int>(type: "int", nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    ClosedForSale = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightCountInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightWeightInventories",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<int>(type: "int", nullable: false),
                    FlightId = table.Column<long>(type: "bigint", nullable: false),
                    WeightResourceId = table.Column<long>(type: "bigint", nullable: false),
                    CapacityKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ClosedForSale = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightWeightInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirportSlotAdjustments",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AirportSlotInventoryId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousTotal = table.Column<int>(type: "int", nullable: false),
                    NewTotal = table.Column<int>(type: "int", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpectedVersion = table.Column<long>(type: "bigint", nullable: false),
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportSlotAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirportSlotAdjustments_AirportSlotInventories_AirportSlotInventoryId",
                        column: x => x.AirportSlotInventoryId,
                        principalSchema: "Ancillary",
                        principalTable: "AirportSlotInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryPassengerUsageLimits",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    InventoryPolicyId = table.Column<long>(type: "bigint", nullable: false),
                    LimitScope = table.Column<int>(type: "int", nullable: false),
                    MaxUnits = table.Column<int>(type: "int", nullable: false),
                    CountingFamilyCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryPassengerUsageLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryPassengerUsageLimits_AncillaryInventoryPolicies_InventoryPolicyId",
                        column: x => x.InventoryPolicyId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryInventoryPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightCountAdjustments",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FlightCountInventoryId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousTotal = table.Column<int>(type: "int", nullable: false),
                    NewTotal = table.Column<int>(type: "int", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpectedVersion = table.Column<long>(type: "bigint", nullable: false),
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightCountAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightCountAdjustments_FlightCountInventories_FlightCountInventoryId",
                        column: x => x.FlightCountInventoryId,
                        principalSchema: "Ancillary",
                        principalTable: "FlightCountInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightWeightAdjustments",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FlightWeightInventoryId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NewKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpectedVersion = table.Column<long>(type: "bigint", nullable: false),
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightWeightAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightWeightAdjustments_FlightWeightInventories_FlightWeightInventoryId",
                        column: x => x.FlightWeightInventoryId,
                        principalSchema: "Ancillary",
                        principalTable: "FlightWeightInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotAdjustments_AirportSlotInventoryId_CorrelationId",
                schema: "Ancillary",
                table: "AirportSlotAdjustments",
                columns: new[] { "AirportSlotInventoryId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotInventories_OwnerAirlineId_FacilityId_EndUtc",
                schema: "Ancillary",
                table: "AirportSlotInventories",
                columns: new[] { "OwnerAirlineId", "FacilityId", "EndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotInventories_PhysicalKey_Current",
                schema: "Ancillary",
                table: "AirportSlotInventories",
                columns: new[] { "OwnerAirlineId", "FacilityId", "StartUtc", "EndUtc" },
                unique: true,
                filter: "[Status] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotInventories_Status",
                schema: "Ancillary",
                table: "AirportSlotInventories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryInventoryPolicies_Identity_Current",
                schema: "Ancillary",
                table: "AncillaryInventoryPolicies",
                columns: new[] { "OwnerAirlineId", "ServiceDefinitionRef" },
                unique: true,
                filter: "[Status] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryInventoryPolicies_ServiceDefinitionId",
                schema: "Ancillary",
                table: "AncillaryInventoryPolicies",
                column: "ServiceDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryInventoryPolicies_Status",
                schema: "Ancillary",
                table: "AncillaryInventoryPolicies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountAdjustments_FlightCountInventoryId_CorrelationId",
                schema: "Ancillary",
                table: "FlightCountAdjustments",
                columns: new[] { "FlightCountInventoryId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountInventories_OwnerAirlineId_ResourceId",
                schema: "Ancillary",
                table: "FlightCountInventories",
                columns: new[] { "OwnerAirlineId", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountInventories_PhysicalKey_Current",
                schema: "Ancillary",
                table: "FlightCountInventories",
                columns: new[] { "OwnerAirlineId", "FlightId", "ResourceId" },
                unique: true,
                filter: "[Status] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountInventories_Status",
                schema: "Ancillary",
                table: "FlightCountInventories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FlightWeightAdjustments_FlightWeightInventoryId_CorrelationId",
                schema: "Ancillary",
                table: "FlightWeightAdjustments",
                columns: new[] { "FlightWeightInventoryId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightWeightInventories_PhysicalKey_Current",
                schema: "Ancillary",
                table: "FlightWeightInventories",
                columns: new[] { "OwnerAirlineId", "FlightId", "WeightResourceId" },
                unique: true,
                filter: "[Status] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_FlightWeightInventories_Status",
                schema: "Ancillary",
                table: "FlightWeightInventories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPassengerUsageLimits_InventoryPolicyId_LimitScope",
                schema: "Ancillary",
                table: "InventoryPassengerUsageLimits",
                columns: new[] { "InventoryPolicyId", "LimitScope" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirportSlotAdjustments",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "FlightCountAdjustments",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "FlightWeightAdjustments",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "InventoryPassengerUsageLimits",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AirportSlotInventories",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "FlightCountInventories",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "FlightWeightInventories",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AncillaryInventoryPolicies",
                schema: "Ancillary");
        }
    }
}
