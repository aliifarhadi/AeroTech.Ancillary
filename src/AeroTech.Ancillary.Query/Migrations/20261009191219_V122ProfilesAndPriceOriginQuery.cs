using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V122ProfilesAndPriceOriginQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentRouting",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profile",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AirportServiceDirection",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AirportServiceFacilityId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AirportServiceMaxGuestsPerPrimary",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AirportServiceRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AirportServiceTerminalRef",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AirportServiceWindowEnd",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AirportServiceWindowStart",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssistedTravelConnectionPolicy",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AssistedTravelMedicalApprovalRequired",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssistedTravelMinimumLeadTimeMinutes",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AssistedTravelRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PetAcceptanceMode",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PetCountryExceptionCode",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PetMaxCombinedKgOverride",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PetMinAnimalAgeWeeksOverride",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PetRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceOrigin",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuoteProviderKey",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxTreatment",
                schema: "ReadModel",
                table: "AncillaryPriceComponents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsumptionUnit",
                schema: "ReadModel",
                table: "AncillaryInventoryPassengerUsageLimits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsPerPurchase",
                schema: "ReadModel",
                table: "AncillaryInventoryPassengerUsageLimits",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentRouting",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "Profile",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "AirportServiceDirection",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceFacilityId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceMaxGuestsPerPrimary",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceTerminalRef",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceWindowEnd",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AirportServiceWindowStart",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AssistedTravelConnectionPolicy",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AssistedTravelMedicalApprovalRequired",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AssistedTravelMinimumLeadTimeMinutes",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "AssistedTravelRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PetAcceptanceMode",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PetCountryExceptionCode",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PetMaxCombinedKgOverride",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PetMinAnimalAgeWeeksOverride",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PetRuleId",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PriceOrigin",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "QuoteProviderKey",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "TaxTreatment",
                schema: "ReadModel",
                table: "AncillaryPriceComponents");

            migrationBuilder.DropColumn(
                name: "ConsumptionUnit",
                schema: "ReadModel",
                table: "AncillaryInventoryPassengerUsageLimits");

            migrationBuilder.DropColumn(
                name: "UnitsPerPurchase",
                schema: "ReadModel",
                table: "AncillaryInventoryPassengerUsageLimits");
        }
    }
}
