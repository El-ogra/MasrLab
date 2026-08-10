using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasrLab.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MasrLabDbContext))]
[Migration("20260810120000_AlignPatientHistoryViewComparisonFlag")]
public partial class AlignPatientHistoryViewComparisonFlag : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql(CreateViewSql(compareAllClinicalFields: true));

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql(CreateViewSql(compareAllClinicalFields: false));

    private static string CreateViewSql(bool compareAllClinicalFields) => $@"
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
    prev.Value AS PreviousValue,
    prev.Unit AS PreviousUnit,
    prev.ReferenceRange AS PreviousReferenceRange,
    prev.StatusString AS PreviousStatus,
    prev.VisitDate AS PreviousVisitDate,
    curr.Value AS CurrentValue,
    curr.Unit AS CurrentUnit,
    curr.ReferenceRange AS CurrentReferenceRange,
    curr.StatusString AS CurrentStatus,
    curr.VisitDate AS CurrentVisitDate,
    CAST(CASE
        WHEN prev.PatientId IS NULL THEN 0
        WHEN {ComparisonCondition(compareAllClinicalFields)} THEN 1
        ELSE 0
    END AS BIT) AS ComparisonFlag
FROM NumberedResults curr
LEFT JOIN NumberedResults prev
    ON prev.PatientId = curr.PatientId
   AND prev.TestId = curr.TestId
   AND prev.RowNum = curr.RowNum + 1;";

    private static string ComparisonCondition(bool compareAllClinicalFields) => compareAllClinicalFields
        ? "(curr.Value <> prev.Value OR curr.Unit <> prev.Unit OR curr.ReferenceRange <> prev.ReferenceRange OR (curr.Value IS NULL AND prev.Value IS NOT NULL) OR (curr.Value IS NOT NULL AND prev.Value IS NULL) OR (curr.Unit IS NULL AND prev.Unit IS NOT NULL) OR (curr.Unit IS NOT NULL AND prev.Unit IS NULL) OR (curr.ReferenceRange IS NULL AND prev.ReferenceRange IS NOT NULL) OR (curr.ReferenceRange IS NOT NULL AND prev.ReferenceRange IS NULL))"
        : "(curr.Value <> prev.Value OR (curr.Value IS NULL AND prev.Value IS NOT NULL) OR (curr.Value IS NOT NULL AND prev.Value IS NULL))";
}
