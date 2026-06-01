using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationForBasket24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_basket_items_baskets_BasketEntityId",
                table: "basket_items");

            migrationBuilder.DropIndex(
                name: "IX_basket_items_BasketEntityId",
                table: "basket_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "BasketEntityId",
                table: "basket_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "BasketEntityId",
                table: "basket_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_basket_items_BasketEntityId",
                table: "basket_items",
                column: "BasketEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_basket_items_baskets_BasketEntityId",
                table: "basket_items",
                column: "BasketEntityId",
                principalTable: "baskets",
                principalColumn: "Id");
        }
    }
}
