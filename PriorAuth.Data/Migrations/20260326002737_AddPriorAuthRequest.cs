using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorAuthRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriorAuthRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeterminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalJustification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiagnosisCodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriorTreatments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelevantLabValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contraindications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PractitionerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorAuthRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriorAuthRequests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriorAuthRequests_Practitioners_PractitionerId",
                        column: x => x.PractitionerId,
                        principalTable: "Practitioners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthRequests_PatientId",
                table: "PriorAuthRequests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthRequests_PractitionerId",
                table: "PriorAuthRequests",
                column: "PractitionerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriorAuthRequests");
        }
    }
}
