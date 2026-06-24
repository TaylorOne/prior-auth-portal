using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuth.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPractitionerSpecialtyToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Practitioners SET Specialty = 'Orthopedics' WHERE Specialty = 'Orthopedic Surgery'");
            migrationBuilder.Sql("UPDATE Practitioners SET Specialty = 'Oncology' WHERE Specialty = 'Medical Oncology'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Practitioners SET Specialty = 'Orthopedic Surgery' WHERE Specialty = 'Orthopedics'");
            migrationBuilder.Sql("UPDATE Practitioners SET Specialty = 'Medical Oncology' WHERE Specialty = 'Oncology'");
        }
    }
}
