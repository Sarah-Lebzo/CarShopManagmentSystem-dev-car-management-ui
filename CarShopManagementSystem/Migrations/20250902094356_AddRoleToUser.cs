using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RegisterDate", "Role" },
                values: new object[] { new DateTime(2025, 9, 2, 12, 43, 55, 55, DateTimeKind.Local).AddTicks(1118), "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 1, 23, 23, 29, 252, DateTimeKind.Local).AddTicks(1898));
        }
    }
}
