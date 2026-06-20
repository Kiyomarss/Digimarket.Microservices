using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAfterPaymentState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                     UPDATE orders
                                     SET order_state_id = order_state_id + 1
                                     WHERE order_state_id >= 4;
                                 """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                     UPDATE orders
                                     SET order_state_id = order_state_id - 1
                                     WHERE order_state_id >= 5;
                                 """);
        }
    }
}
