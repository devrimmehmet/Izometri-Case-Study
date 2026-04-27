using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "Expenses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DisplayName", "Email", "IsDeleted", "PasswordHash", "Phone", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000010"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "İzometri Personel 2", "personel2@izometri.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000011"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test1 Personel 2", "personel2@test1.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000012"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Personel 2", "personel2@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "Role", "TenantId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("00000003-0000-0000-0000-000000000010"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personnel", new Guid("10000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000010") },
                    { new Guid("00000003-0000-0000-0000-000000000011"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personnel", new Guid("10000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000011") },
                    { new Guid("00000003-0000-0000-0000-000000000012"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personnel", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000012") }
                });

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "ExchangeRate",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "ExchangeRate",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedBy", "ExchangeRate", "RequestedByUserId" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000011"), 1m, new Guid("20000000-0000-0000-0000-000000000011") });

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "ExchangeRate",
                value: 32.5m);

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"),
                column: "ExchangeRate",
                value: 1m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"));

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "Expenses");

            migrationBuilder.UpdateData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedBy", "RequestedByUserId" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000006"), new Guid("20000000-0000-0000-0000-000000000006") });
        }
    }
}
