using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisonFlagToPatientHistoryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientVisits_LabId",
                table: "PatientVisits");

            migrationBuilder.RenameColumn(
                name: "NetProfit",
                table: "Accounts",
                newName: "NetActivityAfterCommission");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "PriceLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_IsDefault",
                table: "PriceLists",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_LabId",
                table: "PatientVisits",
                column: "LabId",
                unique: true);

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW [dbo].[PatientHistoryView]
AS
WITH NumberedResults AS (
    SELECT
        P.Id AS PatientId,
        P.LabId,
        T.Id AS TestId,
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
            PARTITION BY P.Id, T.Id
            ORDER BY PV.VisitDate DESC
        ) AS RowNum
    FROM TestResults TR
    INNER JOIN VisitTests VT ON TR.VisitTestId = VT.Id
    INNER JOIN PatientVisits PV ON VT.PatientVisitId = PV.Id
    INNER JOIN Patients P ON PV.PatientId = P.Id
    INNER JOIN Tests T ON VT.TestId = T.Id
    WHERE P.IsDeleted = 0
      AND PV.IsDeleted = 0
      AND VT.IsDeleted = 0
      AND TR.IsDeleted = 0
      AND T.IsDeleted = 0
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
             AND curr.Value <> prev.Value THEN 1
            ELSE 0
        END
    AS BIT)                 AS ComparisonFlag
FROM NumberedResults curr
LEFT JOIN NumberedResults prev
    ON prev.PatientId = curr.PatientId
   AND prev.TestId    = curr.TestId
   AND prev.RowNum    = curr.RowNum + 1
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceLists_IsDefault",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_PatientVisits_LabId",
                table: "PatientVisits");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PriceLists");

            migrationBuilder.RenameColumn(
                name: "NetActivityAfterCommission",
                table: "Accounts",
                newName: "NetProfit");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_LabId",
                table: "PatientVisits",
                column: "LabId");

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW [dbo].[PatientHistoryView]
AS
WITH NumberedResults AS (
    SELECT
        P.Id AS PatientId,
        P.LabId,
        T.Id AS TestId,
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
            PARTITION BY P.Id, T.Id
            ORDER BY PV.VisitDate DESC
        ) AS RowNum
    FROM TestResults TR
    INNER JOIN VisitTests VT ON TR.VisitTestId = VT.Id
    INNER JOIN PatientVisits PV ON VT.PatientVisitId = PV.Id
    INNER JOIN Patients P ON PV.PatientId = P.Id
    INNER JOIN Tests T ON VT.TestId = T.Id
    WHERE P.IsDeleted = 0
      AND PV.IsDeleted = 0
      AND VT.IsDeleted = 0
      AND TR.IsDeleted = 0
      AND T.IsDeleted = 0
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
    curr.VisitDate          AS CurrentVisitDate
FROM NumberedResults curr
LEFT JOIN NumberedResults prev
    ON prev.PatientId = curr.PatientId
   AND prev.TestId    = curr.TestId
   AND prev.RowNum    = curr.RowNum + 1
");
        }
    }
}
