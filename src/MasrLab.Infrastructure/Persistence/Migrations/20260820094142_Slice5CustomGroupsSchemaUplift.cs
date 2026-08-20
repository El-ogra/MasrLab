using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice5CustomGroupsSchemaUplift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS
(
    SELECT 1
    FROM [TestGroupItems]
    WHERE [IsDeleted] = 0
    GROUP BY [TestGroupId], [TestId]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(
        'Cannot create unique index IX_TestGroupItems_TestGroupId_TestId because duplicate TestGroupItems rows exist for the same TestGroupId and TestId.',
        16,
        1);
END");

            migrationBuilder.Sql("ALTER TABLE [TestGroupItems] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0");

            migrationBuilder.Sql("ALTER TABLE [TestGroups] DROP COLUMN [GroupPrice]");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupItems_TestGroupId_TestId",
                table: "TestGroupItems",
                columns: new[] { "TestGroupId", "TestId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_TestGroupItems_Tests_TestId",
                table: "TestGroupItems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestGroupItems_Tests_TestId",
                table: "TestGroupItems");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupItems_TestGroupId_TestId",
                table: "TestGroupItems");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "TestGroupItems");

            migrationBuilder.AddColumn<decimal>(
                name: "GroupPrice",
                table: "TestGroups",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
