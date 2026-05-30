using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresManualReviewToAuthRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualReview",
                table: "AuthRules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresManualReview",
                table: "AuthRules");
        }
    }
}
