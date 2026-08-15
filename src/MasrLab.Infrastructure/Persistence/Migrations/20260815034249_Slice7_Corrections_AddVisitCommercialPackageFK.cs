using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice7_Corrections_AddVisitCommercialPackageFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VisitTests_VisitCommercialPackageId",
                table: "VisitTests",
                column: "VisitCommercialPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitTests_VisitCommercialPackages_VisitCommercialPackageId",
                table: "VisitTests",
                column: "VisitCommercialPackageId",
                principalTable: "VisitCommercialPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitTests_VisitCommercialPackages_VisitCommercialPackageId",
                table: "VisitTests");

            migrationBuilder.DropIndex(
                name: "IX_VisitTests_VisitCommercialPackageId",
                table: "VisitTests");
        }
    }
}
