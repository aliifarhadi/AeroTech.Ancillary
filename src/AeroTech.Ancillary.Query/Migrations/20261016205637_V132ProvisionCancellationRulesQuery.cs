using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ancillary.Query.Migrations
{
    /// <inheritdoc />
    public partial class V132ProvisionCancellationRulesQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AncillaryProvisionCancellationRules",
                schema: "ReadModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AncillaryProvisionId = table.Column<long>(type: "bigint", nullable: false),
                    RuleRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonKind = table.Column<int>(type: "int", nullable: false),
                    BeforeStartMinutesFrom = table.Column<int>(type: "int", nullable: true),
                    BeforeStartMinutesTo = table.Column<int>(type: "int", nullable: true),
                    PenaltyKind = table.Column<int>(type: "int", nullable: false),
                    PenaltyFixedAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: true),
                    PenaltyPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    PenaltyCurrencyId = table.Column<int>(type: "int", nullable: true),
                    PercentageBase = table.Column<int>(type: "int", nullable: true),
                    WaiverPermitted = table.Column<bool>(type: "bit", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncillaryProvisionCancellationRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AncillaryProvisionCancellationRules_AncillaryProvisionId",
                schema: "ReadModel",
                table: "AncillaryProvisionCancellationRules",
                column: "AncillaryProvisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AncillaryProvisionCancellationRules",
                schema: "ReadModel");
        }
    }
}
