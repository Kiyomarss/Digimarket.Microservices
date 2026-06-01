using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration231 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasketEntityId",
                table: "basket_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BasketEntityId",
                table: "basket_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
