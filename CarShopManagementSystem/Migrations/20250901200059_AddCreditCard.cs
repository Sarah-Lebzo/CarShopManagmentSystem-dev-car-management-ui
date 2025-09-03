using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    HolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryMonth = table.Column<int>(type: "int", nullable: false),
                    ExpiryYear = table.Column<int>(type: "int", nullable: false),
                    CVV = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCards", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CreditCards",
                columns: new[] { "Id", "Balance", "CVV", "CardNumber", "ExpiryMonth", "ExpiryYear", "HolderName", "IsValid" },
                values: new object[,]
                {
                    { 1, 50000m, "123", "4111111111111111", 12, 2027, "John Doe", true },
                    { 2, 45000m, "456", "5500000000000004", 11, 2028, "Jane Smith", true },
                    { 3, 60000m, "789", "340000000000009", 10, 2029, "Ali Ahmad", true },
                    { 4, 10000m, "012", "30000000000004", 9, 2026, "Sara Ibrahim", true },
                    { 5, 8000m, "345", "6011000000000004", 8, 2030, "Mohamed Ali", true },
                    { 6, 12000m, "678", "3530111333300000", 7, 2027, "Lina Hassan", true },
                    { 7, 30000m, "901", "6331101999990016", 6, 2028, "Omar Saleh", true },
                    { 8, 15000m, "234", "6759649826438453", 5, 2029, "Rania Khaled", true },
                    { 9, 20000m, "567", "4111111111111129", 4, 2027, "Invalid Card", false },
                    { 10, 0m, "890", "5500000000000051", 3, 2027, "No Funds", true }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 1, 23, 0, 58, 560, DateTimeKind.Local).AddTicks(4548));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCards");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2025, 9, 1, 21, 48, 47, 429, DateTimeKind.Local).AddTicks(9667));
        }
    }
}
