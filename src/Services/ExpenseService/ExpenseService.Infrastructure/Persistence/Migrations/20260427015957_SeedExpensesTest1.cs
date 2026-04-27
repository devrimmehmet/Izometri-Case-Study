using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedExpensesTest1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Expenses",
                columns: new[] { "Id", "AdminApproved", "Amount", "ApprovedAt", "Category", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "DeletedBy", "Description", "HrApproved", "IsDeleted", "RejectedAt", "RejectionReason", "RequestedByUserId", "Status", "SubmittedAt", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), false, 1250.00m, null, 1, new DateTime(2026, 4, 21, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "Ankara Müşteri Ziyareti", false, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 1, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), false, 450.00m, null, 2, new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "Kırtasiye Malzemeleri", false, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 2, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), true, 5000.00m, null, 3, new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "Udemy Eğitim Kursu", true, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 3, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), false, 200.00m, null, 4, new DateTime(2026, 4, 24, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000005"), 2, null, null, "Yazılım Lisansı", false, false, null, "Bütçe onayı yok", new Guid("20000000-0000-0000-0000-000000000005"), 4, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), false, 5500.00m, null, 1, new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "İstanbul Uçak Bileti", true, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 2, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Expenses",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));
        }
    }
}
