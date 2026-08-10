using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatisticsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatisticsSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiCode = table.Column<byte>(type: "tinyint", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<short>(type: "smallint", nullable: false),
                    ValueFormat = table.Column<byte>(type: "tinyint", nullable: false),
                    DecimalPlaces = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    WarningThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CriticalThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ComparisonDirection = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticsSettings", x => x.Id);
                    table.CheckConstraint("CK_StatisticsSettings_DecimalPlaces", "[DecimalPlaces] BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_StatisticsSettings_DisplayOrder", "[DisplayOrder] > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsSettings_IsDeleted",
                table: "StatisticsSettings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsSettings_IsEnabled_DisplayOrder",
                table: "StatisticsSettings",
                columns: new[] { "IsEnabled", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsSettings_KpiCode",
                table: "StatisticsSettings",
                column: "KpiCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatisticsSettings");
        }
    }
}
