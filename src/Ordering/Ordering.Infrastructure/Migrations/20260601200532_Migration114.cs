using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration114 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_orders_UserId_order_state_id",
                table: "orders",
                newName: "IX_Order_UserId_State");

            migrationBuilder.RenameIndex(
                name: "IX_orders_Date",
                table: "orders",
                newName: "IX_Order_Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Order_UserId_State",
                table: "orders",
                newName: "IX_orders_UserId_order_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Date",
                table: "orders",
                newName: "IX_orders_Date");
        }
    }
}
