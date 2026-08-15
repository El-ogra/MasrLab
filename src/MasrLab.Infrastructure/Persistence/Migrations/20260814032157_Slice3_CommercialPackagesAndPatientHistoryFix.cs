using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Slice3_CommercialPackagesAndPatientHistoryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "TestGroupItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommercialPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitCommercialPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientVisitId = table.Column<int>(type: "int", nullable: false),
                    CommercialPackageId = table.Column<int>(type: "int", nullable: false),
                    PackageNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitCommercialPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitCommercialPackages_PatientVisits_PatientVisitId",
                        column: x => x.PatientVisitId,
                        principalTable: "PatientVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommercialPackageItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommercialPackageId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialPackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommercialPackageItems_CommercialPackages_CommercialPackageId",
                        column: x => x.CommercialPackageId,
                        principalTable: "CommercialPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommercialPackagePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommercialPackageId = table.Column<int>(type: "int", nullable: false),
                    PriceListId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialPackagePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommercialPackagePrices_CommercialPackages_CommercialPackageId",
                        column: x => x.CommercialPackageId,
                        principalTable: "CommercialPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitTests_PatientVisitId_TestId",
                table: "VisitTests",
                columns: new[] { "PatientVisitId", "TestId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackageItems_CommercialPackageId",
                table: "CommercialPackageItems",
                column: "CommercialPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackageItems_CommercialPackageId_DisplayOrder",
                table: "CommercialPackageItems",
                columns: new[] { "CommercialPackageId", "DisplayOrder" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackageItems_CommercialPackageId_TestId",
                table: "CommercialPackageItems",
                columns: new[] { "CommercialPackageId", "TestId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackageItems_IsDeleted",
                table: "CommercialPackageItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackageItems_TestId",
                table: "CommercialPackageItems",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackagePrices_CommercialPackageId",
                table: "CommercialPackagePrices",
                column: "CommercialPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackagePrices_CommercialPackageId_PriceListId",
                table: "CommercialPackagePrices",
                columns: new[] { "CommercialPackageId", "PriceListId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackagePrices_IsDeleted",
                table: "CommercialPackagePrices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackagePrices_PriceListId",
                table: "CommercialPackagePrices",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackages_IsDeleted",
                table: "CommercialPackages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialPackages_Name",
                table: "CommercialPackages",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VisitCommercialPackages_CommercialPackageId",
                table: "VisitCommercialPackages",
                column: "CommercialPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitCommercialPackages_IsDeleted",
                table: "VisitCommercialPackages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VisitCommercialPackages_PatientVisitId",
                table: "VisitCommercialPackages",
                column: "PatientVisitId");

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW [dbo].[PatientHistoryView]
AS
WITH CurrentResults AS (
    SELECT
        P.Id AS PatientId,
        P.LabId,
        VT.TestId,
        T.Name AS TestName,
        T.ReportName AS TestReportName,
        TR.Value,
        TR.Unit,
        TR.ReferenceRange,
        CASE TR.Status
            WHEN 0 THEN 'High'
            WHEN 1 THEN 'Low'
            WHEN 2 THEN 'Normal'
            ELSE 'Normal'
        END AS StatusString,
        PV.VisitDate,
        ROW_NUMBER() OVER (
            PARTITION BY P.Id, VT.TestId, PV.VisitDate
            ORDER BY VTRI.DisplayOrder DESC
        ) AS RowNum
    FROM TestResults TR
    INNER JOIN VisitTestResultItems VTRI ON TR.VisitTestResultItemId = VTRI.Id
    INNER JOIN VisitTests VT ON VTRI.VisitTestId = VT.Id
    INNER JOIN PatientVisits PV ON VT.PatientVisitId = PV.Id
    INNER JOIN Patients P ON PV.PatientId = P.Id
    INNER JOIN Tests T ON VT.TestId = T.Id
    WHERE P.IsDeleted = 0
      AND PV.IsDeleted = 0
      AND VT.IsDeleted = 0
      AND VTRI.IsDeleted = 0
      AND TR.IsDeleted = 0
      AND T.IsDeleted = 0
      AND VTRI.ResultEntryKind = 0
),
NumberedVisits AS (
    SELECT
        cr.*,
        ROW_NUMBER() OVER (
            PARTITION BY cr.PatientId, cr.TestId
            ORDER BY cr.VisitDate DESC
        ) AS VisitRank
    FROM CurrentResults cr
    WHERE cr.RowNum = 1
)
SELECT
    curr.PatientId,
    curr.LabId,
    curr.TestId,
    curr.TestName,
    curr.TestReportName,
    prev.Value              AS PreviousValue,
    prev.Unit               AS PreviousUnit,
    prev.ReferenceRange     AS PreviousReferenceRange,
    prev.StatusString       AS PreviousStatus,
    prev.VisitDate          AS PreviousVisitDate,
    curr.Value              AS CurrentValue,
    curr.Unit               AS CurrentUnit,
    curr.ReferenceRange     AS CurrentReferenceRange,
    curr.StatusString       AS CurrentStatus,
    curr.VisitDate          AS CurrentVisitDate,
    CAST(
        CASE
            WHEN prev.Value IS NOT NULL
             AND (curr.Value <> prev.Value OR curr.Unit <> prev.Unit OR curr.ReferenceRange <> prev.ReferenceRange)
            THEN 1
            ELSE 0
        END AS BIT)         AS ComparisonFlag
FROM NumberedVisits curr
LEFT JOIN NumberedVisits prev
    ON prev.PatientId = curr.PatientId
   AND prev.TestId    = curr.TestId
   AND prev.VisitRank = curr.VisitRank + 1
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommercialPackageItems");

            migrationBuilder.DropTable(
                name: "CommercialPackagePrices");

            migrationBuilder.DropTable(
                name: "VisitCommercialPackages");

            migrationBuilder.DropTable(
                name: "CommercialPackages");

            migrationBuilder.DropIndex(
                name: "IX_VisitTests_PatientVisitId_TestId",
                table: "VisitTests");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "TestGroupItems");
        }
    }
}
