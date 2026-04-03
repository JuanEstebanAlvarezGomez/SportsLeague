using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsLeague.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_ContactEmail",
                table: "Sponsors",
                column: "ContactEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_Name",
                table: "Sponsors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sponsors");
        }
    }
}
