using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class HopeLastFixing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubBrandId",
                table: "Cars",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 4, 0, 5, 48, 303, DateTimeKind.Local).AddTicks(5405));

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SubBrandId",
                table: "Cars",
                column: "SubBrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_SubBrands_SubBrandId",
                table: "Cars",
                column: "SubBrandId",
                principalTable: "SubBrands",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_SubBrands_SubBrandId",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "IX_Cars_SubBrandId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "SubBrandId",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 3, 23, 57, 40, 16, DateTimeKind.Local).AddTicks(2385));
        }
    }
}
