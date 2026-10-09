using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V122ProfilesAndPriceOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsumptionUnit",
                schema: "Ancillary",
                table: "InventoryPassengerUsageLimits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsPerPurchase",
                schema: "Ancillary",
                table: "InventoryPassengerUsageLimits",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentRouting",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profile",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceOrigin",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuoteProviderKey",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxTreatment",
                schema: "Ancillary",
                table: "AncillaryPriceComponents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AirportServiceSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    TerminalRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FacilityId = table.Column<long>(type: "bigint", nullable: true),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    ServiceWindowStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    ServiceWindowEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    IanaTimeZone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    VisitDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    MaxGuestsPerPrimary = table.Column<int>(type: "int", nullable: true),
                    RequiresSpecificAppointment = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportServiceSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AirportServiceSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    AssistanceKind = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaggageSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    ChargeKind = table.Column<int>(type: "int", nullable: false),
                    AllowanceConcept = table.Column<int>(type: "int", nullable: true),
                    PackageWeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxKgPerPiece = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WeightFromExclusiveKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WeightToInclusiveKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxLengthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxWidthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxHeightCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxLinearSumCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    EquipmentKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChargeCombination = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaggageSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_BaggageSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectivitySpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    PlanKind = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IncludedDataMb = table.Column<int>(type: "int", nullable: true),
                    MaxDevices = table.Column<int>(type: "int", nullable: true),
                    DeliveryStage = table.Column<int>(type: "int", nullable: false),
                    FulfillmentProviderRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectivitySpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_ConnectivitySpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    MealKind = table.Column<int>(type: "int", nullable: false),
                    MealCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    MenuItemRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DietaryCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CateringLeadTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    ExclusiveMealFamilyCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_MealSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    TransportMode = table.Column<int>(type: "int", nullable: false),
                    MaxCombinedWeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CarrierMaxLengthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CarrierMaxWidthCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CarrierMaxHeightCm = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MinAnimalAgeWeeks = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_PetSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrioritySpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    PriorityZoneCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PriorityGroupCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FareBenefitRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrioritySpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_PrioritySpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionAirportServiceRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    TerminalRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Direction = table.Column<int>(type: "int", nullable: true),
                    ServiceWindowStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    ServiceWindowEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    FacilityId = table.Column<long>(type: "bigint", nullable: true),
                    MaxGuestsPerPrimary = table.Column<int>(type: "int", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAirportServiceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAirportServiceRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionAssistedTravelRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    MinimumLeadTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    ConnectionPolicy = table.Column<int>(type: "int", nullable: true),
                    MedicalApprovalRequired = table.Column<bool>(type: "bit", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionAssistedTravelRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionAssistedTravelRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionPetRules",
                schema: "Ancillary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    CountryExceptionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MinAnimalAgeWeeksOverride = table.Column<int>(type: "int", nullable: true),
                    MaxCombinedKgOverride = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    AcceptanceMode = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionPetRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionPetRules_AncillaryProvisions_AncillaryProvisionId",
                        column: x => x.AncillaryProvisionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryProvisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeatSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    SeatPurpose = table.Column<int>(type: "int", nullable: false),
                    RequiresExitRowEligibility = table.Column<bool>(type: "bit", nullable: false),
                    RequiresAdjacentSeat = table.Column<bool>(type: "bit", nullable: false),
                    ExtraSeatPurpose = table.Column<int>(type: "int", nullable: true),
                    ExtraOccupiedSeatCount = table.Column<int>(type: "int", nullable: true),
                    RequiresExternalTicketAction = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_SeatSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UpgradeSpecifications",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FromCabinId = table.Column<int>(type: "int", nullable: false),
                    ToCabinId = table.Column<int>(type: "int", nullable: false),
                    AllowedUpgradeKind = table.Column<int>(type: "int", nullable: false),
                    RequiresTicketExchange = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpgradeSpecifications", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_UpgradeSpecifications_AncillaryServiceDefinitions_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AncillaryServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AirportServiceSpecificationComponents",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportServiceSpecificationComponents", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_AirportServiceSpecificationComponents_AirportServiceSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AirportServiceSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelBassinetDetails",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    MaxInfantWeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxInfantAgeMonths = table.Column<int>(type: "int", nullable: true),
                    RequiresInfantAndGuardian = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelBassinetDetails", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelBassinetDetails_AssistedTravelSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelDisabilityDetails",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    RequiredCommunicationMethod = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelDisabilityDetails", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelDisabilityDetails_AssistedTravelSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelMedicalDetails",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    MedicalServiceCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    RequiresMedicalApproval = table.Column<bool>(type: "bit", nullable: false),
                    EquipmentKind = table.Column<int>(type: "int", nullable: false),
                    OxygenUnits = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelMedicalDetails", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelMedicalDetails_AssistedTravelSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelMinorDetails",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    MinAgeYears = table.Column<int>(type: "int", nullable: false),
                    MaxAgeYearsExclusive = table.Column<int>(type: "int", nullable: false),
                    GuardianContactRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionPolicy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelMinorDetails", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelMinorDetails_AssistedTravelSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelWheelchairDetails",
                schema: "Ancillary",
                columns: table => new
                {
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    AssistanceLevelCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeadTimeMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelWheelchairDetails", x => x.AncillaryServiceDefinitionId);
                    table.ForeignKey(
                        name: "FK_AssistedTravelWheelchairDetails_AssistedTravelSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectivitySpecificationAircraft",
                schema: "Ancillary",
                columns: table => new
                {
                    AircraftId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectivitySpecificationAircraft", x => new { x.AncillaryServiceDefinitionId, x.AircraftId });
                    table.ForeignKey(
                        name: "FK_ConnectivitySpecificationAircraft_ConnectivitySpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "ConnectivitySpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSpecificationAnimalTypes",
                schema: "Ancillary",
                columns: table => new
                {
                    AnimalType = table.Column<int>(type: "int", nullable: false),
                    OtherCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSpecificationAnimalTypes", x => new { x.AncillaryServiceDefinitionId, x.AnimalType, x.OtherCode });
                    table.ForeignKey(
                        name: "FK_PetSpecificationAnimalTypes_PetSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "PetSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSpecificationDocumentCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSpecificationDocumentCodes", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_PetSpecificationDocumentCodes_PetSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "PetSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSpecificationSizeBrackets",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    WeightFromExclusiveKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WeightToInclusiveKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSpecificationSizeBrackets", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_PetSpecificationSizeBrackets_PetSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "PetSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrioritySpecificationAirports",
                schema: "Ancillary",
                columns: table => new
                {
                    AirportId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrioritySpecificationAirports", x => new { x.AncillaryServiceDefinitionId, x.AirportId });
                    table.ForeignKey(
                        name: "FK_PrioritySpecificationAirports_PrioritySpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "PrioritySpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeatSpecificationCabins",
                schema: "Ancillary",
                columns: table => new
                {
                    CabinClassId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatSpecificationCabins", x => new { x.AncillaryServiceDefinitionId, x.CabinClassId });
                    table.ForeignKey(
                        name: "FK_SeatSpecificationCabins_SeatSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "SeatSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeatSpecificationCharacteristicCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatSpecificationCharacteristicCodes", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_SeatSpecificationCharacteristicCodes_SeatSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "SeatSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UpgradeSpecificationFareFamilies",
                schema: "Ancillary",
                columns: table => new
                {
                    FareFamilyId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpgradeSpecificationFareFamilies", x => new { x.AncillaryServiceDefinitionId, x.FareFamilyId });
                    table.ForeignKey(
                        name: "FK_UpgradeSpecificationFareFamilies_UpgradeSpecifications_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "UpgradeSpecifications",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelBassinetSeatGroups",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelBassinetSeatGroups", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_AssistedTravelBassinetSeatGroups_AssistedTravelBassinetDetails_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelBassinetDetails",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelDisabilitySsrCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelDisabilitySsrCodes", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_AssistedTravelDisabilitySsrCodes_AssistedTravelDisabilityDetails_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelDisabilityDetails",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelMedicalEvidenceCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelMedicalEvidenceCodes", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_AssistedTravelMedicalEvidenceCodes_AssistedTravelMedicalDetails_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelMedicalDetails",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelMinorTransitAirports",
                schema: "Ancillary",
                columns: table => new
                {
                    AirportId = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelMinorTransitAirports", x => new { x.AncillaryServiceDefinitionId, x.AirportId });
                    table.ForeignKey(
                        name: "FK_AssistedTravelMinorTransitAirports_AssistedTravelMinorDetails_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelMinorDetails",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistedTravelWheelchairSsrCodes",
                schema: "Ancillary",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AncillaryServiceDefinitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistedTravelWheelchairSsrCodes", x => new { x.AncillaryServiceDefinitionId, x.Code });
                    table.ForeignKey(
                        name: "FK_AssistedTravelWheelchairSsrCodes_AssistedTravelWheelchairDetails_AncillaryServiceDefinitionId",
                        column: x => x.AncillaryServiceDefinitionId,
                        principalSchema: "Ancillary",
                        principalTable: "AssistedTravelWheelchairDetails",
                        principalColumn: "AncillaryServiceDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAirportServiceRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionAirportServiceRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionAssistedTravelRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionAssistedTravelRules",
                column: "AncillaryProvisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionPetRules_AncillaryProvisionId",
                schema: "Ancillary",
                table: "ProvisionPetRules",
                column: "AncillaryProvisionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirportServiceSpecificationComponents",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelBassinetSeatGroups",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelDisabilitySsrCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelMedicalEvidenceCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelMinorTransitAirports",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelWheelchairSsrCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "BaggageSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ConnectivitySpecificationAircraft",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "MealSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PetSpecificationAnimalTypes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PetSpecificationDocumentCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PetSpecificationSizeBrackets",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PrioritySpecificationAirports",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionAirportServiceRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionAssistedTravelRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ProvisionPetRules",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "SeatSpecificationCabins",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "SeatSpecificationCharacteristicCodes",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "UpgradeSpecificationFareFamilies",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AirportServiceSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelBassinetDetails",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelDisabilityDetails",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelMedicalDetails",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelMinorDetails",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelWheelchairDetails",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "ConnectivitySpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PetSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "PrioritySpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "SeatSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "UpgradeSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropTable(
                name: "AssistedTravelSpecifications",
                schema: "Ancillary");

            migrationBuilder.DropColumn(
                name: "ConsumptionUnit",
                schema: "Ancillary",
                table: "InventoryPassengerUsageLimits");

            migrationBuilder.DropColumn(
                name: "UnitsPerPurchase",
                schema: "Ancillary",
                table: "InventoryPassengerUsageLimits");

            migrationBuilder.DropColumn(
                name: "DocumentRouting",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "Profile",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "PriceOrigin",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "QuoteProviderKey",
                schema: "Ancillary",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TaxTreatment",
                schema: "Ancillary",
                table: "AncillaryPriceComponents");
        }
    }
}
