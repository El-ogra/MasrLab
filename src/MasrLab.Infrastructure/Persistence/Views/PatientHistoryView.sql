/* ============================================================================
   MasrLab — vw_PatientHistory
   ----------------------------------------------------------------------------
   Entity 3 (PatientHistory) — كيان مُشتق (Derived Entity / SQL View)
   المرجع: Docs/MasrLab_Specifications_and_Audit.md — القسم 2.1 / Entity 3
           + الجزء الثالث (قرارات سد الفجوات — الفجوة رقم 1)
   القرار الحاكم: DD-03 (اعتماد طبقتي SQL + C# معاً)

   ⚠️ PLACEHOLDER — لم يُكتب منطق الاستعلام بعد.
   يُملأ لاحقاً في مرحلة Business Logic بحيث يجمع نتائج TestResult
   عبر المسار: TestResult → VisitTest → PatientVisit → Patient
   لنفس المريض (PatientId / LabId) ونفس TestId، مرتبةً تصاعدياً بـ VisitDate，
   ويعرض القيمة السابقة والحالية جنباً إلى جنب للمقارنة.

   الحقول الستة عشر المطلوبة (View Columns):
     01. PatientId              INT            — من Patient
     02. LabId                  NVARCHAR       — من Patient
     03. TestId                 INT            — من Test عبر VisitTest
     04. TestName               NVARCHAR       — Test.Name
     05. TestReportName         NVARCHAR       — Test.ReportName
     06. PreviousValue          NVARCHAR       — TestResult.Value (السابقة)
     07. PreviousUnit           NVARCHAR       — TestResult.Unit (السابقة)
     08. PreviousReferenceRange NVARCHAR       — TestResult.ReferenceRange (السابقة)
     09. PreviousStatus         NVARCHAR       — High/Low/Normal (السابقة)
     10. PreviousVisitDate      DATE           — PatientVisit.VisitDate (السابقة)
     11. CurrentValue           NVARCHAR       — TestResult.Value (الحالية)
     12. CurrentUnit            NVARCHAR       — TestResult.Unit (الحالية)
     13. CurrentReferenceRange  NVARCHAR       — TestResult.ReferenceRange (الحالية)
     14. CurrentStatus          NVARCHAR       — High/Low/Normal (الحالية)
     15. CurrentVisitDate       DATE           — تاريخ الزيارة الحالية
     16. ComparisonFlag         BIT            — هل يوجد فرق بين القيمتين

   ملاحظات إلزامية:
     - هذا الـ View لا يحمل أعمدة تدقيق (CreatedAt/UpdatedAt/IsDeleted)
       لأنه ليس جدولاً مخزَّناً — راجع القسم 7.3.2 (الاستثناء).
     - ComparisonFlag يتطلب استعلاماً/حساباً من طبقة التطبيق，
       وهو سبب اعتماد الطبقتين معاً (SQL View + keyless entity في EF Core).
     - الطبقة المقابلة في C#:
       src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.cs
       (keyless entity — تُستهلك عبر EF Core بعد ربطها بـ ToView).
   ============================================================================ */

-- TODO (مرحلة Business Logic): استبدال هذا الـ Placeholder بتعريف الـ View الفعلي.
-- CREATE OR ALTER VIEW dbo.vw_PatientHistory AS
-- SELECT ... ;
