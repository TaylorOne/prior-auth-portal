using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredSpecialtyToAuthRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredSpecialty",
                table: "AuthRules",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredSpecialty",
                table: "AuthRules");
        }
    }
}
