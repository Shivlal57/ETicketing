using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ETicketing.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalQuota = table.Column<int>(type: "int", nullable: false),
                    AvailableQuota = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.CheckConstraint("CK_Tickets_Price", "[Price] > 0");
                    table.CheckConstraint("CK_Tickets_Quota", "[TotalQuota] >= 0 AND [AvailableQuota] >= 0 AND [AvailableQuota] <= [TotalQuota]");
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "AvailableQuota", "CreatedAt", "IsActive", "Name", "Price", "TotalQuota" },
                values: new object[,]
                {
                    { 1, 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Gold", 100m, 100 },
                    { 2, 200, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Premium", 200m, 200 },
                    { 3, 50, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "VIP", 500m, 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Name",
                table: "Tickets",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
