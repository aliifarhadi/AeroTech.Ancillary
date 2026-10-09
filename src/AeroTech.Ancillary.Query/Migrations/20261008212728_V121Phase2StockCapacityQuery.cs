using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V121Phase2StockCapacityQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirportSlotAdjustments",
                schema: "ReadModel",
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
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportSlotAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirportSlotInventories",
                schema: "ReadModel",
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
                    AdjustmentCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportSlotInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryInventoryPassengerUsageLimits",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    InventoryPolicyId = table.Column<long>(type: "bigint", nullable: false),
                    LimitScope = table.Column<int>(type: "int", nullable: false),
                    MaxUnits = table.Column<int>(type: "int", nullable: false),
                    CountingFamilyCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryInventoryPassengerUsageLimits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AncillaryInventoryPolicies",
                schema: "ReadModel",
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
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryInventoryPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightCountAdjustments",
                schema: "ReadModel",
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
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightCountAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightCountInventories",
                schema: "ReadModel",
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
                    AdjustmentCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightCountInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightWeightAdjustments",
                schema: "ReadModel",
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
                    ResultingVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightWeightAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightWeightInventories",
                schema: "ReadModel",
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
                    AdjustmentCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightWeightInventories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotAdjustments_AirportSlotInventoryId",
                schema: "ReadModel",
                table: "AirportSlotAdjustments",
                column: "AirportSlotInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AirportSlotInventories_OwnerAirlineId_FacilityId_StartUtc",
                schema: "ReadModel",
                table: "AirportSlotInventories",
                columns: new[] { "OwnerAirlineId", "FacilityId", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryInventoryPassengerUsageLimits_InventoryPolicyId",
                schema: "ReadModel",
                table: "AncillaryInventoryPassengerUsageLimits",
                column: "InventoryPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryInventoryPolicies_OwnerAirlineId_ServiceDefinitionRef_Status",
                schema: "ReadModel",
                table: "AncillaryInventoryPolicies",
                columns: new[] { "OwnerAirlineId", "ServiceDefinitionRef", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountAdjustments_FlightCountInventoryId",
                schema: "ReadModel",
                table: "FlightCountAdjustments",
                column: "FlightCountInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightCountInventories_OwnerAirlineId_FlightId_ResourceId",
                schema: "ReadModel",
                table: "FlightCountInventories",
                columns: new[] { "OwnerAirlineId", "FlightId", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_FlightWeightAdjustments_FlightWeightInventoryId",
                schema: "ReadModel",
                table: "FlightWeightAdjustments",
                column: "FlightWeightInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightWeightInventories_OwnerAirlineId_FlightId_WeightResourceId",
                schema: "ReadModel",
                table: "FlightWeightInventories",
                columns: new[] { "OwnerAirlineId", "FlightId", "WeightResourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirportSlotAdjustments",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AirportSlotInventories",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryInventoryPassengerUsageLimits",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "AncillaryInventoryPolicies",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "FlightCountAdjustments",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "FlightCountInventories",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "FlightWeightAdjustments",
                schema: "ReadModel");

            migrationBuilder.DropTable(
                name: "FlightWeightInventories",
                schema: "ReadModel");
        }
    }
}
