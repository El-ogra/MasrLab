using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompoundTestAndResultSlotModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_VisitTests_VisitTestId",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_VisitTestId",
                table: "TestResults");

            migrationBuilder.RenameColumn(
                name: "VisitTestId",
                table: "TestResults",
                newName: "VisitTestResultItemId");

            migrationBuilder.RenameColumn(
                name: "VisitTestId",
                table: "Cultures",
                newName: "VisitTestResultItemId");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompoundSnapshot",
                table: "VisitTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNameSnapshot",
                table: "VisitTests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportNameSnapshot",
                table: "VisitTests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TestNameSnapshot",
                table: "VisitTests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TestComponentId",
                table: "ReferenceValues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ResultEntryKind = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestComponents_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitTestResultItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitTestId = table.Column<int>(type: "int", nullable: false),
                    SourceTestComponentId = table.Column<int>(type: "int", nullable: true),
                    ComponentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ComponentUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ResultEntryKind = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitTestResultItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitTestResultItems_VisitTests_VisitTestId",
                        column: x => x.VisitTestId,
                        principalTable: "VisitTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_VisitTestResultItemId",
                table: "TestResults",
                column: "VisitTestResultItemId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceValues_TestComponentId",
                table: "ReferenceValues",
                column: "TestComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Cultures_VisitTestResultItemId",
                table: "Cultures",
                column: "VisitTestResultItemId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponents_IsDeleted",
                table: "TestComponents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponents_TestId",
                table: "TestComponents",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponents_TestId_DisplayOrder",
                table: "TestComponents",
                columns: new[] { "TestId", "DisplayOrder" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestComponents_TestId_Name",
                table: "TestComponents",
                columns: new[] { "TestId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VisitTestResultItems_IsDeleted",
                table: "VisitTestResultItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VisitTestResultItems_SourceTestComponentId",
                table: "VisitTestResultItems",
                column: "SourceTestComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitTestResultItems_VisitTestId",
                table: "VisitTestResultItems",
                column: "VisitTestId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitTestResultItems_VisitTestId_SourceTestComponentId",
                table: "VisitTestResultItems",
                columns: new[] { "VisitTestId", "SourceTestComponentId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestComponents");

            migrationBuilder.DropTable(
                name: "VisitTestResultItems");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_VisitTestResultItemId",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceValues_TestComponentId",
                table: "ReferenceValues");

            migrationBuilder.DropIndex(
                name: "IX_Cultures_VisitTestResultItemId",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "IsCompoundSnapshot",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "ReceiptNameSnapshot",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "ReportNameSnapshot",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "TestNameSnapshot",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "TestComponentId",
                table: "ReferenceValues");

            migrationBuilder.RenameColumn(
                name: "VisitTestResultItemId",
                table: "TestResults",
                newName: "VisitTestId");

            migrationBuilder.RenameColumn(
                name: "VisitTestResultItemId",
                table: "Cultures",
                newName: "VisitTestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_VisitTestId",
                table: "TestResults",
                column: "VisitTestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_VisitTests_VisitTestId",
                table: "TestResults",
                column: "VisitTestId",
                principalTable: "VisitTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
