using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveMedicationRequestFKToParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationRequests_PriorAuthRequests_PriorAuthRequestId",
                table: "MedicationRequests");

            migrationBuilder.DropIndex(
                name: "IX_MedicationRequests_PriorAuthRequestId",
                table: "MedicationRequests");

            migrationBuilder.DropColumn(
                name: "PriorAuthRequestId",
                table: "MedicationRequests");

            migrationBuilder.AddColumn<int>(
                name: "MedicationRequestId",
                table: "PriorAuthRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthRequests_MedicationRequestId",
                table: "PriorAuthRequests",
                column: "MedicationRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriorAuthRequests_MedicationRequests_MedicationRequestId",
                table: "PriorAuthRequests",
                column: "MedicationRequestId",
                principalTable: "MedicationRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriorAuthRequests_MedicationRequests_MedicationRequestId",
                table: "PriorAuthRequests");

            migrationBuilder.DropIndex(
                name: "IX_PriorAuthRequests_MedicationRequestId",
                table: "PriorAuthRequests");

            migrationBuilder.DropColumn(
                name: "MedicationRequestId",
                table: "PriorAuthRequests");

            migrationBuilder.AddColumn<int>(
                name: "PriorAuthRequestId",
                table: "MedicationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequests_PriorAuthRequestId",
                table: "MedicationRequests",
                column: "PriorAuthRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationRequests_PriorAuthRequests_PriorAuthRequestId",
                table: "MedicationRequests",
                column: "PriorAuthRequestId",
                principalTable: "PriorAuthRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
