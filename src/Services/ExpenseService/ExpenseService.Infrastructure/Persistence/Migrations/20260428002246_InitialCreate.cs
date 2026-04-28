using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeadLetteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FixedUsdRate = table.Column<decimal>(type: "numeric", nullable: true),
                    FixedEurRate = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HrApproved = table.Column<bool>(type: "boolean", nullable: false),
                    AdminApproved = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseApprovals_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "FixedEurRate", "FixedUsdRate", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, false, "izometri", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, false, "test1", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, false, "test2", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DisplayName", "Email", "IsDeleted", "PasswordHash", "Phone", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "İzometri Admin", "admin@izometri.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "İzometri İK", "hr@izometri.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "İzometri Personel", "personel@izometri.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test1 Admin", "pattabanoglu@devrimmehmet.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", "905438194976", new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test1 İK", "devrimmehmet@gmail.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", "905393649361", new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000006"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test1 Personel", "devrimmehmet@msn.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000007"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Admin", "admin@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000008"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 İK", "hr@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000009"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Personel", "personel@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000010"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "İzometri Personel 2", "personel2@izometri.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000011"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test1 Personel 2", "personel2@test1.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("20000000-0000-0000-0000-000000000012"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Test2 Personel 2", "personel2@test2.com", false, "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.", null, new Guid("10000000-0000-0000-0000-000000000003"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Expenses",
                columns: new[] { "Id", "AdminApproved", "Amount", "ApprovedAt", "Category", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "DeletedBy", "Description", "ExchangeRate", "HrApproved", "IsDeleted", "RejectedAt", "RejectionReason", "RequestedByUserId", "Status", "SubmittedAt", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), false, 1250.00m, null, 1, new DateTime(2026, 4, 21, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "Ankara Müşteri Ziyareti", 1m, false, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 1, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), false, 450.00m, null, 2, new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "Kırtasiye Malzemeleri", 1m, false, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 2, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), true, 5000.00m, null, 3, new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000011"), 1, null, null, "Udemy Eğitim Kursu", 1m, true, false, null, null, new Guid("20000000-0000-0000-0000-000000000011"), 3, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), false, 200.00m, null, 4, new DateTime(2026, 4, 24, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000005"), 2, null, null, "Yazılım Lisansı", 32.5m, false, false, null, "Bütçe onayı yok", new Guid("20000000-0000-0000-0000-000000000005"), 4, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), false, 5500.00m, null, 1, new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("20000000-0000-0000-0000-000000000006"), 1, null, null, "İstanbul Uçak Bileti", 1m, true, false, null, null, new Guid("20000000-0000-0000-0000-000000000006"), 2, null, new Guid("10000000-0000-0000-0000-000000000002"), null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "Role", "TenantId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Admin", new Guid("10000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("00000003-0000-0000-0000-000000000002"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "HR", new Guid("10000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("00000003-0000-0000-0000-000000000003"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000003") },
                    { new Guid("00000003-0000-0000-0000-000000000004"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Admin", new Guid("10000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000004") },
                    { new Guid("00000003-0000-0000-0000-000000000005"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "HR", new Guid("10000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000005") },
                    { new Guid("00000003-0000-0000-0000-000000000006"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000006") },
                    { new Guid("00000003-0000-0000-0000-000000000007"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Admin", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000007") },
                    { new Guid("00000003-0000-0000-0000-000000000008"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "HR", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000008") },
                    { new Guid("00000003-0000-0000-0000-000000000009"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000009") },
                    { new Guid("00000003-0000-0000-0000-000000000010"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000010") },
                    { new Guid("00000003-0000-0000-0000-000000000011"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000011") },
                    { new Guid("00000003-0000-0000-0000-000000000012"), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Personel", new Guid("10000000-0000-0000-0000-000000000003"), null, null, new Guid("20000000-0000-0000-0000-000000000012") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_ExpenseId",
                table: "ExpenseApprovals",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RequestedByUserId",
                table: "Expenses",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_DeadLetteredAt",
                table: "OutboxMessages",
                column: "DeadLetteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                table: "OutboxMessages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_Role",
                table: "UserRoles",
                columns: new[] { "UserId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseApprovals");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
