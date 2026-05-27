using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevisePatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Patients",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Patients",
                newName: "MemberId");
        }
    }
}
