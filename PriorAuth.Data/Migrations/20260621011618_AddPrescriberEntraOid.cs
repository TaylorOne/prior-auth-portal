using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriberEntraOid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EntraOid",
                table: "Practitioners",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Practitioners_EntraOid",
                table: "Practitioners",
                column: "EntraOid",
                unique: true,
                filter: "[EntraOid] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Practitioners_EntraOid",
                table: "Practitioners");

            migrationBuilder.DropColumn(
                name: "EntraOid",
                table: "Practitioners");
        }
    }
}
