using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module1_Slice2_MedicalHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAnemia",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OnAntibiotic",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OnAntiviralTreatment",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OnBloodPressureTreatment",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecentContrastOrUltrasound",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAnemia",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OnAntibiotic",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OnAntiviralTreatment",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OnBloodPressureTreatment",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "RecentContrastOrUltrasound",
                table: "Patients");
        }
    }
}
