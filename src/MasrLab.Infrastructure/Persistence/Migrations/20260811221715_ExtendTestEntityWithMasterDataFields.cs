using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendTestEntityWithMasterDataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AddWithGroup",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ArabicName",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArrangeNo",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BarcodeName",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoryName",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMainTest",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LabToLabPrice",
                table: "Tests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogGroup",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OutsourcedCostPrice",
                table: "Tests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutsourcedLabName",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientQuestion",
                table: "Tests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrintWithOther",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceType",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SampleType",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SeeReport",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SentOutsideLab",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TestCode",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestTimeDays",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tube1",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tube2",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tube3",
                table: "Tests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tests_ArrangeNo",
                table: "Tests",
                column: "ArrangeNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tests_ArrangeNo",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "AddWithGroup",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "ArabicName",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "ArrangeNo",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "BarcodeName",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "HistoryName",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "IsMainTest",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "LabToLabPrice",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "LogGroup",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "OutsourcedCostPrice",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "OutsourcedLabName",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "PatientQuestion",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "PrintWithOther",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "SampleType",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "SeeReport",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "SentOutsideLab",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "TestCode",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "TestTimeDays",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Tube1",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Tube2",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Tube3",
                table: "Tests");
        }
    }
}
