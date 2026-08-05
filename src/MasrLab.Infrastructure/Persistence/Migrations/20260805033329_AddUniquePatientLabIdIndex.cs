using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePatientLabIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_LabId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_LabId",
                table: "Patients",
                column: "LabId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_LabId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_LabId",
                table: "Patients",
                column: "LabId");
        }
    }
}
