using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice12_ReferralEntityLabToLabDiscriminator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ReferralEntities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                table: "ReferralEntities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "ReferralEntities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLabToLab",
                table: "PriceLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_IsLabToLab",
                table: "PriceLists",
                column: "IsLabToLab",
                unique: true,
                filter: "[IsLabToLab] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralEntities_PriceLists_PriceListId",
                table: "ReferralEntities",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralEntities_PriceLists_PriceListId",
                table: "ReferralEntities");

            migrationBuilder.DropIndex(
                name: "IX_PriceLists_IsLabToLab",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ReferralEntities");

            migrationBuilder.DropColumn(
                name: "Commission",
                table: "ReferralEntities");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "ReferralEntities");

            migrationBuilder.DropColumn(
                name: "IsLabToLab",
                table: "PriceLists");
        }
    }
}
