using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsolidatedReportPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsolidatedReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientVisitId = table.Column<int>(type: "int", nullable: false),
                    PrintGroupSubtitles = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrintedByUserId = table.Column<int>(type: "int", nullable: true),
                    PrintCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidatedReportItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsolidatedReportId = table.Column<int>(type: "int", nullable: false),
                    VisitTestId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedReportItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedReportItems_ConsolidatedReports_ConsolidatedReportId",
                        column: x => x.ConsolidatedReportId,
                        principalTable: "ConsolidatedReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedReportItems_ConsolidatedReportId_DisplayOrder",
                table: "ConsolidatedReportItems",
                columns: new[] { "ConsolidatedReportId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedReportItems_ConsolidatedReportId_VisitTestId",
                table: "ConsolidatedReportItems",
                columns: new[] { "ConsolidatedReportId", "VisitTestId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedReports_IsDeleted",
                table: "ConsolidatedReports",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedReports_PatientVisitId",
                table: "ConsolidatedReports",
                column: "PatientVisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsolidatedReportItems");

            migrationBuilder.DropTable(
                name: "ConsolidatedReports");
        }
    }
}
