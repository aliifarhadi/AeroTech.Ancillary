using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V121FinalProvisionDescriptorsQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingConfirmationRequirement",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "AdvancePurchaseMaximumPeriod",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageAllowanceConcept",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaggageChargeKind",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseStage",
                schema: "ReadModel",
                table: "AncillaryProvisions",
                type: "int",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingConfirmationRequirement",
                schema: "ReadModel",
                table: "AncillaryServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "AdvancePurchaseMaximumPeriod",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageAllowanceConcept",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "BaggageChargeKind",
                schema: "ReadModel",
                table: "AncillaryProvisions");

            migrationBuilder.DropColumn(
                name: "PurchaseStage",
                schema: "ReadModel",
                table: "AncillaryProvisions");
        }
    }
}
