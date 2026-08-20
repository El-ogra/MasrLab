using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    public partial class Slice4PriceListItemUniquenessAndForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS
(
    SELECT 1
    FROM [PriceListItems]
    GROUP BY [PriceListId], [TestId]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(
        'Cannot create unique index IX_PriceListItems_PriceListId_TestId because duplicate PriceListItems rows exist for the same PriceListId and TestId.',
        16,
        1);
END");

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems",
                columns: new[] { "PriceListId", "TestId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceListItems_Tests_TestId",
                table: "PriceListItems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceListItems_Tests_TestId",
                table: "PriceListItems");

            migrationBuilder.DropIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems");
        }
    }
}
