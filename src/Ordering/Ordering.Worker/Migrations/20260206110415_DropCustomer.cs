using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Worker.Migrations
{
    /// <inheritdoc />
    public partial class DropCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customer",
                table: "OrderState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "OrderState",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
