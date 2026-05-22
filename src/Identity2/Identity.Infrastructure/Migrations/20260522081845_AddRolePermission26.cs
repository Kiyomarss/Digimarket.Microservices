using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermission26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_permissions_roles_RoleId",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_permissions_RoleId",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_RoleId",
                table: "permissions",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_permissions_roles_RoleId",
                table: "permissions",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id");
        }
    }
}
