using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice13_2_CultureAntibioticMasterSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CultureAntibiotics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CultureTestId = table.Column<int>(type: "int", nullable: false),
                    AntibioticId = table.Column<int>(type: "int", nullable: false),
                    SensitivityText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Pregnant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Children = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureAntibiotics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CultureAntibiotics_Antibiotics_AntibioticId",
                        column: x => x.AntibioticId,
                        principalTable: "Antibiotics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CultureAntibiotics_Tests_CultureTestId",
                        column: x => x.CultureTestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CultureAntibioticCommercialNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CultureAntibioticId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Print = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureAntibioticCommercialNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CultureAntibioticCommercialNames_CultureAntibiotics_CultureAntibioticId",
                        column: x => x.CultureAntibioticId,
                        principalTable: "CultureAntibiotics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibioticCommercialNames_CultureAntibioticId",
                table: "CultureAntibioticCommercialNames",
                column: "CultureAntibioticId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibioticCommercialNames_IsDeleted",
                table: "CultureAntibioticCommercialNames",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibiotics_AntibioticId",
                table: "CultureAntibiotics",
                column: "AntibioticId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibiotics_CultureTestId",
                table: "CultureAntibiotics",
                column: "CultureTestId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibiotics_CultureTestId_AntibioticId",
                table: "CultureAntibiotics",
                columns: new[] { "CultureTestId", "AntibioticId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibiotics_IsDeleted",
                table: "CultureAntibiotics",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CultureAntibioticCommercialNames");

            migrationBuilder.DropTable(
                name: "CultureAntibiotics");
        }
    }
}
