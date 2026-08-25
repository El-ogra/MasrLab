using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerOrganismSensitivityAndMicroscopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InhibitionZoneOverride",
                table: "Sensitivities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganismSlot",
                table: "Sensitivities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCommercialNameInReport",
                table: "Cultures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowReferenceInReport",
                table: "Cultures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowSensitivityInReport",
                table: "Cultures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MicroscopicFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CultureId = table.Column<int>(type: "int", nullable: false),
                    RowKey = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IncludeInPrint = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicroscopicFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicroscopicFindings_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sensitivities_CultureId_OrganismSlot_AntibioticId",
                table: "Sensitivities",
                columns: new[] { "CultureId", "OrganismSlot", "AntibioticId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MicroscopicFindings_CultureId",
                table: "MicroscopicFindings",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_MicroscopicFindings_CultureId_RowKey",
                table: "MicroscopicFindings",
                columns: new[] { "CultureId", "RowKey" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MicroscopicFindings_IsDeleted",
                table: "MicroscopicFindings",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MicroscopicFindings");

            migrationBuilder.DropIndex(
                name: "IX_Sensitivities_CultureId_OrganismSlot_AntibioticId",
                table: "Sensitivities");

            migrationBuilder.DropColumn(
                name: "InhibitionZoneOverride",
                table: "Sensitivities");

            migrationBuilder.DropColumn(
                name: "OrganismSlot",
                table: "Sensitivities");

            migrationBuilder.DropColumn(
                name: "ShowCommercialNameInReport",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "ShowReferenceInReport",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "ShowSensitivityInReport",
                table: "Cultures");
        }
    }
}
