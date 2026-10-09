using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V121FinalProvisionDescriptors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowanceConcept",
                schema: "Ancillary",
                table: "ProvisionBaggageApplicationRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChargeKind",
                schema: "Ancillary",
                table: "ProvisionBaggageApplicationRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumPeriod",
                schema: "Ancillary",
                table: "ProvisionAdvancePurchaseRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingConfirmationRequirement",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseStage",
                schema: "Ancillary",
                table: "AncillaryProvisions",
                type: "int",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowanceConcept",
                schema: "Ancillary",
                table: "ProvisionBaggageApplicationRules");

            migrationBuilder.DropColumn(
                name: "ChargeKind",
                schema: "Ancillary",
                table: "ProvisionBaggageApplicationRules");

            migrationBuilder.DropColumn(
                name: "MaximumPeriod",
                schema: "Ancillary",
                table: "ProvisionAdvancePurchaseRules");

            migrationBuilder.DropColumn(
                name: "BookingConfirmationRequirement",
                schema: "Ancillary",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "PurchaseStage",
                schema: "Ancillary",
                table: "AncillaryProvisions");
        }
    }
}
