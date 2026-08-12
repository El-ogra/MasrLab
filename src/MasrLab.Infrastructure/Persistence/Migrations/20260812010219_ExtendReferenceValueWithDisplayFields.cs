using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendReferenceValueWithDisplayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForPregnantOnly",
                table: "ReferenceValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HighFlag",
                table: "ReferenceValues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HighLimit",
                table: "ReferenceValues",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LowFlag",
                table: "ReferenceValues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LowLimit",
                table: "ReferenceValues",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestUnit",
                table: "ReferenceValues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForPregnantOnly",
                table: "ReferenceValues");

            migrationBuilder.DropColumn(
                name: "HighFlag",
                table: "ReferenceValues");

            migrationBuilder.DropColumn(
                name: "HighLimit",
                table: "ReferenceValues");

            migrationBuilder.DropColumn(
                name: "LowFlag",
                table: "ReferenceValues");

            migrationBuilder.DropColumn(
                name: "LowLimit",
                table: "ReferenceValues");

            migrationBuilder.DropColumn(
                name: "TestUnit",
                table: "ReferenceValues");
        }
    }
}
