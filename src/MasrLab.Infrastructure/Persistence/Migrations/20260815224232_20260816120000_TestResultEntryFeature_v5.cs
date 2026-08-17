using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260816120000_TestResultEntryFeature_v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TestResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReprintRequired",
                table: "TestResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
-- Guarded AlterColumn: fail if active null-component ReferenceValues exist
IF EXISTS (SELECT 1 FROM ReferenceValues WHERE TestComponentId IS NULL AND IsDeleted = 0)
    RAISERROR('Cannot convert ReferenceValues.TestComponentId to non-null: active null-component rows exist.', 16, 1);
ELSE
BEGIN
    DROP INDEX IX_ReferenceValues_TestComponentId ON ReferenceValues;
    ALTER TABLE ReferenceValues ALTER COLUMN TestComponentId int NOT NULL;
    CREATE NONCLUSTERED INDEX IX_ReferenceValues_TestComponentId ON ReferenceValues(TestComponentId);
END
");

            migrationBuilder.Sql(@"
-- Idempotent backfill: fill VisitTestResultItems slots missing for any VisitTest
INSERT INTO VisitTestResultItems (VisitTestId, SourceTestComponentId, ComponentName, ComponentUnit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
SELECT vt.Id, tc.Id, tc.Name, tc.Unit, tc.DisplayOrder, tc.ResultEntryKind, GETUTCDATE(), 1, 0
FROM VisitTests vt
INNER JOIN Tests t ON vt.TestId = t.Id
INNER JOIN TestComponents tc ON tc.TestId = t.Id AND tc.IsDeleted = 0
WHERE vt.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM VisitTestResultItems vtri
      WHERE vtri.VisitTestId = vt.Id
        AND vtri.SourceTestComponentId = tc.Id
        AND vtri.IsDeleted = 0
  );
");

            migrationBuilder.CreateTable(
                name: "CulturePrintReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitTestResultItemId = table.Column<int>(type: "int", nullable: false),
                    PrintedByUserId = table.Column<int>(type: "int", nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrintCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturePrintReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CulturePrintReceipts_VisitTestResultItems_VisitTestResultItemId",
                        column: x => x.VisitTestResultItemId,
                        principalTable: "VisitTestResultItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestComponentChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestComponentId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestComponentChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestComponentChoices_TestComponents_TestComponentId",
                        column: x => x.TestComponentId,
                        principalTable: "TestComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestResultEditHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestResultId = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OldComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    EditedByUserId = table.Column<int>(type: "int", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResultEditHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResultEditHistories_TestResults_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Name",
                table: "Patients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CulturePrintReceipts_IsDeleted",
                table: "CulturePrintReceipts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CulturePrintReceipts_VisitTestResultItemId",
                table: "CulturePrintReceipts",
                column: "VisitTestResultItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponentChoices_IsDeleted",
                table: "TestComponentChoices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponentChoices_TestComponentId",
                table: "TestComponentChoices",
                column: "TestComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponentChoices_TestComponentId_Value",
                table: "TestComponentChoices",
                columns: new[] { "TestComponentId", "Value" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestResultEditHistories_EditedByUserId",
                table: "TestResultEditHistories",
                column: "EditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResultEditHistories_IsDeleted",
                table: "TestResultEditHistories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TestResultEditHistories_TestResultId",
                table: "TestResultEditHistories",
                column: "TestResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CulturePrintReceipts");

            migrationBuilder.DropTable(
                name: "TestComponentChoices");

            migrationBuilder.DropTable(
                name: "TestResultEditHistories");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Name",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "ReprintRequired",
                table: "TestResults");

            migrationBuilder.AlterColumn<int>(
                name: "TestComponentId",
                table: "ReferenceValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
