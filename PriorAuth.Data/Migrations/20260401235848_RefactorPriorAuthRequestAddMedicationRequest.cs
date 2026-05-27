using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPriorAuthRequestAddMedicationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicalJustification",
                table: "PriorAuthRequests");

            migrationBuilder.DropColumn(
                name: "Contraindications",
                table: "PriorAuthRequests");

            migrationBuilder.DropColumn(
                name: "DiagnosisCodes",
                table: "PriorAuthRequests");

            migrationBuilder.RenameColumn(
                name: "RequestType",
                table: "PriorAuthRequests",
                newName: "ServiceCodeSystem");

            migrationBuilder.RenameColumn(
                name: "RelevantLabValues",
                table: "PriorAuthRequests",
                newName: "ServiceCodeDisplay");

            migrationBuilder.RenameColumn(
                name: "PriorTreatments",
                table: "PriorAuthRequests",
                newName: "ClinicalData");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "PriorAuthRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MedicationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubstitutionAllowed = table.Column<bool>(type: "bit", nullable: true),
                    SubstitutionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DosageInstructionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantityValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    QuantityUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfRepeatsAllowed = table.Column<int>(type: "int", nullable: true),
                    ExpectedSupplyDurationDays = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriorAuthRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationRequests_PriorAuthRequests_PriorAuthRequestId",
                        column: x => x.PriorAuthRequestId,
                        principalTable: "PriorAuthRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequests_PriorAuthRequestId",
                table: "MedicationRequests",
                column: "PriorAuthRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicationRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "PriorAuthRequests");

            migrationBuilder.RenameColumn(
                name: "ServiceCodeSystem",
                table: "PriorAuthRequests",
                newName: "RequestType");

            migrationBuilder.RenameColumn(
                name: "ServiceCodeDisplay",
                table: "PriorAuthRequests",
                newName: "RelevantLabValues");

            migrationBuilder.RenameColumn(
                name: "ClinicalData",
                table: "PriorAuthRequests",
                newName: "PriorTreatments");

            migrationBuilder.AddColumn<string>(
                name: "ClinicalJustification",
                table: "PriorAuthRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contraindications",
                table: "PriorAuthRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisCodes",
                table: "PriorAuthRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
