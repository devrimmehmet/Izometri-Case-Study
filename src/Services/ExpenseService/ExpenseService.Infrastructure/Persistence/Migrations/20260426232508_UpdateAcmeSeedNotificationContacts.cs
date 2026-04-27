using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAcmeSeedNotificationContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "Email", "Phone" },
                values: new object[] { "devrimmehmet@gmail.com", "905438194976" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "Email", "Phone" },
                values: new object[] { "devrimmehmet@msn.com", "905393649361" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "Email", "Phone" },
                values: new object[] { "admin@acme.com", "905438194976" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "Email", "Phone" },
                values: new object[] { "hr@acme.com", "905393649361" });
        }
    }
}
