using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAndUpdatePractitioner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Practitioners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Practitioners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Practitioners_OrganizationId",
                table: "Practitioners",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Practitioners_Organizations_OrganizationId",
                table: "Practitioners",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Practitioners_Organizations_OrganizationId",
                table: "Practitioners");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Practitioners_OrganizationId",
                table: "Practitioners");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Practitioners");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Practitioners");
        }
    }
}
