using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice7_Corrections_AddVisitCommercialPackageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VisitCommercialPackageId",
                table: "VisitTests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisitCommercialPackageId",
                table: "VisitTests");
        }
    }
}
