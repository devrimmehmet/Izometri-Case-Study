using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedThreeTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "Name",
                value: "izometri");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "Name",
                value: "test1");

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "test2", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "İzometri Admin", "admin@izometri.com", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "İzometri İK", "hr@izometri.com", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "İzometri Personel", "personel@izometri.com" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Test1 Admin", "pattabanoglu@devrimmehmet.com", "905438194976" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Test1 İK", "devrimmehmet@gmail.com", "905393649361" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "Test1 Personel", "devrimmehmet@msn.com" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DisplayName", "Email", "IsDeleted", "PasswordHash", "Phone", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000007"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Admin", "admin@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000008"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 İK", "hr@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000009"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Personel", "personel@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "Role", "TenantId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("00000003-0000-0000-0000-000000000007"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Admin", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000007") },
                    { new Guid("00000003-0000-0000-0000-000000000008"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "HR", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000008") },
                    { new Guid("00000003-0000-0000-0000-000000000009"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personnel", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000009") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "Name",
                value: "acme");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "Name",
                value: "globex");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Acme Admin", "devrimmehmet@gmail.com", "905438194976" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Acme HR", "devrimmehmet@msn.com", "905393649361" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "Acme Personnel", "personel@demo.com" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Globex Admin", "admin@globex.com", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "DisplayName", "Email", "Phone" },
                values: new object[] { "Globex HR", "hr@globex.com", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "Globex Personnel", "personel@demo.com" });
        }
    }
}
