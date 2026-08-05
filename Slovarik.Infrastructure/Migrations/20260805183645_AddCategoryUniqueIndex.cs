using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slovarik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_UserId",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId_Name",
                table: "categories",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_UserId_Name",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId",
                table: "categories",
                column: "UserId");
        }
    }
}
