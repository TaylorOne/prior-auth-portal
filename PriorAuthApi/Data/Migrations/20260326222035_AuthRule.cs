using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuthRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthRuleId",
                table: "PriorAuthRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuthRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    CodeSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndicationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthRequests_AuthRuleId",
                table: "PriorAuthRequests",
                column: "AuthRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriorAuthRequests_AuthRules_AuthRuleId",
                table: "PriorAuthRequests",
                column: "AuthRuleId",
                principalTable: "AuthRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriorAuthRequests_AuthRules_AuthRuleId",
                table: "PriorAuthRequests");

            migrationBuilder.DropTable(
                name: "AuthRules");

            migrationBuilder.DropIndex(
                name: "IX_PriorAuthRequests_AuthRuleId",
                table: "PriorAuthRequests");

            migrationBuilder.DropColumn(
                name: "AuthRuleId",
                table: "PriorAuthRequests");
        }
    }
}
