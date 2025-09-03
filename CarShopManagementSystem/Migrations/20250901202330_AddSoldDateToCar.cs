using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSoldDateToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SoldDate",
                table: "Cars",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 1, 23, 23, 29, 252, DateTimeKind.Local).AddTicks(1898));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldDate",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 1, 23, 0, 58, 560, DateTimeKind.Local).AddTicks(4548));
        }
    }
}
