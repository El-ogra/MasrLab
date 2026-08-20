using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice8VisitTestGroupSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceTestGroupId",
                table: "VisitTests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestGroupNameSnapshot",
                table: "VisitTests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceTestGroupId",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "TestGroupNameSnapshot",
                table: "VisitTests");
        }
    }
}
