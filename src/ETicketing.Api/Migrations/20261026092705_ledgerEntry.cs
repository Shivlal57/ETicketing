using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicketing.Api.Migrations
{
    /// <inheritdoc />
    public partial class ledgerEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTransactionId = table.Column<int>(type: "int", nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                    table.CheckConstraint("CK_LedgerEntries_AccountCode", "[AccountCode] IN ('CASH', 'TICKET_REVENUE')");
                    table.CheckConstraint("CK_LedgerEntries_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_LedgerEntries_EntryType", "[EntryType] IN ('Debit', 'Credit')");
                    table.ForeignKey(
                        name: "FK_LedgerEntries_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 9, 27, 2, 900, DateTimeKind.Utc).AddTicks(6831));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 9, 27, 2, 900, DateTimeKind.Utc).AddTicks(6840));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 9, 27, 2, 900, DateTimeKind.Utc).AddTicks(6841));

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_PaymentTransactionId_AccountCode",
                table: "LedgerEntries",
                columns: new[] { "PaymentTransactionId", "AccountCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerEntries");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 8, 57, 12, 847, DateTimeKind.Utc).AddTicks(1532));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 8, 57, 12, 847, DateTimeKind.Utc).AddTicks(1545));

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 26, 8, 57, 12, 847, DateTimeKind.Utc).AddTicks(1547));
        }
    }
}
