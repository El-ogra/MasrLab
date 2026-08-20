using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1C_FixPriceListItemUniqueIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS
(
    SELECT 1
    FROM [PriceListItems]
    WHERE [IsDeleted] = 0
    GROUP BY [PriceListId], [TestId]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(
        'Cannot create unique index IX_PriceListItems_PriceListId_TestId because duplicate non-deleted PriceListItems rows exist for the same PriceListId and TestId.',
        16,
        1);
END");

            migrationBuilder.DropIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems");

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems",
                columns: new[] { "PriceListId", "TestId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems");

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId_TestId",
                table: "PriceListItems",
                columns: new[] { "PriceListId", "TestId" },
                unique: true);
        }
    }
}
