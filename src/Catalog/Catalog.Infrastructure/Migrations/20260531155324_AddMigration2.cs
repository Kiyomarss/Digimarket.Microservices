using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttributesJson",
                table: "product",
                newName: "Attributes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Attributes",
                table: "product",
                newName: "AttributesJson");
        }
    }
}
