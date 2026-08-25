using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module1_Slice4_SpecimenCapture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SpecimenBlood",
                table: "PatientVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpecimenCsf",
                table: "PatientVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpecimenSemen",
                table: "PatientVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpecimenStool",
                table: "PatientVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpecimenUrine",
                table: "PatientVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecimenBlood",
                table: "PatientVisits");

            migrationBuilder.DropColumn(
                name: "SpecimenCsf",
                table: "PatientVisits");

            migrationBuilder.DropColumn(
                name: "SpecimenSemen",
                table: "PatientVisits");

            migrationBuilder.DropColumn(
                name: "SpecimenStool",
                table: "PatientVisits");

            migrationBuilder.DropColumn(
                name: "SpecimenUrine",
                table: "PatientVisits");
        }
    }
}
