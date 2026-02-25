using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "product",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("""
                                     UPDATE product
                                     SET "Stock" = 10
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "product");
        }
    }
}
