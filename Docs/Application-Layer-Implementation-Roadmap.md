# Application-Layer-Implementation-Roadmap

**المستودع المرجعي:** [`El-ogra/MasrLab`](https://github.com/El-ogra/MasrLab)
**الفرع:** `niamod`
**الكوميت المرجعي (مصدر الحقيقة الوحيد):** [`09547198490128b2efbfc700e79fafe609b7ac9c`](https://github.com/El-ogra/MasrLab/commit/09547198490128b2efbfc700e79fafe609b7ac9c) — «التكوين الثاني»
**النطاق:** خارطة طريق تنفيذية لاستكمال بناء طبقة `MasrLab.Application` بالكامل، مبنية حصراً على تحليل مباشر للكود المصدري عند الكوميت أعلاه.

> ⚠️ هذه وثيقة تخطيط. لا تحتوي على أي كود C# نهائي جاهز للنسخ إلى الإنتاج، وإنما مقتطفات توضيحية (Illustrative snippets) مطابقة للأنماط الموجودة في المستودع.
>
> 🔒 لم يُعدَّل أي ملف في المستودع أثناء إعداد هذه الوثيقة. أوامر `dotnet build -warnaserror` و `dotnet test` نُفِّذت للقراءة والتحقق فقط. لم تُنفَّذ أي أوامر Migration ولا تعديل schema.

---

## المرحلة 0 — إصلاح R0 (شرط مسبق إلزامي)

> **بوابة إلزامية** — بنفس درجة بوابات Build/Test في باقي المراحل. لا تبدأ أي مرحلة تالية قبل التأكد من نجاح `dotnet build -warnaserror` و `dotnet test` بعد هذا التعديل تحديداً.

### 0.1 الحالة الحالية عند HEAD (متحقَّق مباشرةً)

الملف [`src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs) يحوي حالياً:

```csharp
public class OutsourcedSample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public int ExternalLabId { get; set; }
    public decimal CostPrice { get; set; }        // ⚠️ public set — يتخطى SetPrices()
    public decimal PatientPrice { get; set; }     // ⚠️ public set — يتخطى SetPrices()
    public SettlementStatus SettlementStatus { get; private set; }
    public DateTime? ReceivedAt { get; set; }

    public void SetPrices(decimal patientPrice, decimal costPrice)
    {
        if (patientPrice < costPrice)
            throw new BusinessRuleViolationException("Patient price must be greater than or equal to cost price.");
        PatientPrice = patientPrice;
        CostPrice = costPrice;
    }
    // ... Send / ReceiveResult / CompleteSettlement
}
```

**النتيجة:** الـ invariant «سعر المريض ≥ سعر التكلفة» يعتمد على انضباط المطوّر (استدعاء `SetPrices` فقط) بدلاً من أن يكون مُلزَماً بنيوياً. أي كود مستهلك (Handler / Repository / EF Materialization / اختبار) يستطيع كتابة `sample.PatientPrice = -50m;` أو `sample.PatientPrice = 10m; sample.CostPrice = 200m;` بحرية، متجاوزاً فحص `SetPrices()` تماماً. **الإصلاح لم يُطبَّق بعد على شجرة HEAD — مطلوب الآن.**

### 0.2 التعديل الحرفي المطلوب (وحده لا شيء غيره)

في الملف `src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs`، غيّر السطرين التاليين فقط:

```diff
- public decimal CostPrice { get; set; }
- public decimal PatientPrice { get; set; }
+ public decimal CostPrice { get; private set; }
+ public decimal PatientPrice { get; private set; }
```

لا شيء آخر يُعدَّل في هذا الملف. **لا تلمس** `SetPrices`, `Send`, `ReceiveResult`, `CompleteSettlement`, `SettlementStatus`, `ReceivedAt`, ولا أي سطر آخر.

### 0.3 لماذا هذا التعديل آمن كلياً على قاعدة البيانات

- تحويل `public set` إلى `private set` **لا يغيّر Schema**: EF Core يستطيع الوصول إلى الـ backing field عبر Property Access Mode الافتراضي، ولا يوجد عمود جديد ولا تغيير في العمود القائم.
- لا حاجة لـ Migration جديدة (تحقّق: `ModelSnapshot` لا يعتمد على مرئية الـ setter).
- لا استدعاء مباشر خارجي في الكود الحالي يُعيّن هاتين الخاصيتين خارج `SetPrices()` أو `Send()` (كلاهما يبقى داخل الكيان). تحقّق بحثي بـ `grep -R "\.CostPrice\s*=" src/` و `grep -R "\.PatientPrice\s*=" src/` قبل الشروع.

### 0.4 بوابة الخروج (Gate) — إلزامية

> **لا تبدأ المرحلة 1 قبل نجاح الأمرين التاليين حرفياً بعد تعديل السطرين أعلاه.**

```bash
# 1) البناء بلا تحذير ولا خطأ على المشاريع الست (Presentation مستهدف net8.0-windows — يُبنى على Windows فقط)
for p in src/MasrLab.Domain/MasrLab.Domain.csproj \
         src/MasrLab.Application/MasrLab.Application.csproj \
         src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj \
         tests/MasrLab.Domain.Tests/MasrLab.Domain.Tests.csproj \
         tests/MasrLab.Application.Tests/MasrLab.Application.Tests.csproj \
         tests/MasrLab.Infrastructure.Tests/MasrLab.Infrastructure.Tests.csproj ; do
  dotnet build "$p" -warnaserror -c Debug --nologo || { echo "R0 GATE FAILED"; exit 1; }
done

# 2) كل الاختبارات تنجح — يُتوقَّع 189 (Domain 167 + Application 13 + Infrastructure 9)
dotnet test tests/MasrLab.Domain.Tests/MasrLab.Domain.Tests.csproj         --nologo
dotnet test tests/MasrLab.Application.Tests/MasrLab.Application.Tests.csproj --nologo
dotnet test tests/MasrLab.Infrastructure.Tests/MasrLab.Infrastructure.Tests.csproj --nologo
```

**معيار القبول (Gate criteria):**
- كل مشروع من الستة يُرجع `0 Warning(s), 0 Error(s)` تحت `-warnaserror`.
- إجمالي الاختبارات = **189/189 Passed** (0 Failed، 0 Skipped).
- لا Migration جديدة، لا تعديل Schema.
- التزكية النهائية: تشغيل `git diff` يجب أن يُظهر سطرين فقط تم تعديلهما، كلاهما في `OutsourcedSample.cs`.

**لا تنتقل إلى المرحلة 1 قبل تحقّق كل الشروط أعلاه.**

---

## 1. منهجية التحليل ومصادر الحقيقة

### 1.1 التحقّق من الكوميت المرجعي

```bash
git clone https://github.com/El-ogra/MasrLab.git && cd MasrLab
git fetch --all
git ls-remote origin refs/heads/niamod        # ⇒ 09547198490128b2efbfc700e79fafe609b7ac9c refs/heads/niamod
git checkout 09547198490128b2efbfc700e79fafe609b7ac9c
git log -1 --format="%H %s"                    # ⇒ 09547198… «التكوين الثاني»
```

فرع `niamod` مستقر على هذا الكوميت (لا يوجد HEAD أحدث). كل الروابط في هذه الوثيقة تشير إلى `blob/09547198…/…` لضمان الثبات ضد أي دفع لاحق على الفرع.

### 1.2 التحقق من اكتمال طبقة الدومين قبل البناء عليها

**الغرض:** توثيق تحقّق حرفي من الكود المصدري بشأن (1) نجاح البناء تحت `-warnaserror`، (2) العدد الفعلي للاختبارات، (3) حالة أخطاء الدومين الموثَّقة سابقاً.

**ملاحظة بيئية:** جميع الأوامر أدناه نُفِّذت على Linux (Ubuntu، .NET SDK **8.0.423**). مشروع `src/MasrLab.Presentation/MasrLab.csproj` يستهدف `net8.0-windows` (WPF) ولا يُبنى على Linux — وهذا خارج نطاق طبقة Application، فتم استثناؤه من `dotnet build`/`dotnet test`. الحلّ يبقى قابلاً للبناء تحت `-warnaserror` بمعزل عن Presentation.

**1) `dotnet build -warnaserror` — النتائج الحرفية:**

| # | المشروع | التحذيرات | الأخطاء | Build succeeded | الزمن |
|---|---|---|---|---|---|
| 1 | `src/MasrLab.Domain/MasrLab.Domain.csproj` | 0 | 0 | ✅ | 3.41s |
| 2 | `src/MasrLab.Application/MasrLab.Application.csproj` | 0 | 0 | ✅ | 7.49s |
| 3 | `src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj` | 0 | 0 | ✅ | 12.93s |
| 4 | `tests/MasrLab.Domain.Tests/MasrLab.Domain.Tests.csproj` | 0 | 0 | ✅ | 9.03s |
| 5 | `tests/MasrLab.Application.Tests/MasrLab.Application.Tests.csproj` | 0 | 0 | ✅ | 2.46s |
| 6 | `tests/MasrLab.Infrastructure.Tests/MasrLab.Infrastructure.Tests.csproj` | 0 | 0 | ✅ | 3.53s |

**2) `dotnet test` — النتائج الحرفية:**

| مشروع الاختبارات | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| `MasrLab.Domain.Tests` | **167** | 167 | 0 | 0 |
| `MasrLab.Application.Tests` | **13** | 13 | 0 | 0 |
| `MasrLab.Infrastructure.Tests` | **9** | 9 | 0 | 0 |
| **الإجمالي** | **189** | **189** | **0** | **0** |

**3) حالة أخطاء الدومين الموثَّقة تاريخياً في `Docs/Test_for_domain.md` §5.4 — عند HEAD الحالي:**

| # | الخطأ | الحالة |
|---|---|---|
| (أ) | `Culture.Record()` يُطلق `CultureRecorded(Id, Id)` بدلاً من `CultureRecorded(Id, VisitTestId)` | ✅ **مُصلَح** — أُضيف حقل `VisitTestId`، enum `CultureStatus`، Factory `Create(int visitTestId)`، State Machine صارم. `EventRaisedByEntityTests.Culture_Record_ShouldRaiseCultureRecordedWithVisitTestId` ينجح. |
| (ب) | `Receipt.Issue()` قابل للاستدعاء لانهائياً بلا حالة | ✅ **مُصلَح** — أُضيف `ReceiptStatus { Draft, Issued, PartiallyPaid, Paid }`، setters حسّاسة صارت `private set`، `Issue()` تمنع الاستدعاء المتكرر، INV-01 مطبَّق كإجمالي محسوب. |
| (ج) | `OutsourcedSample.CostPrice/PatientPrice` بـ `public set` | ❌ **لم يُصلَح** — عولج في **المرحلة 0** أعلاه كشرط مسبق إلزامي. |

**4) الملفات المُضافة في الكوميت `09547198…` نفسه:**

```
src/MasrLab.Infrastructure/Persistence/MasrLabDbContextFactory.cs                  (14 سطر)
src/MasrLab.Infrastructure/Persistence/Migrations/20260803173749_InitialCreate.Designer.cs   (2212 سطر)
src/MasrLab.Infrastructure/Persistence/Migrations/20260803173749_InitialCreate.cs            (1470 سطر)
src/MasrLab.Infrastructure/Persistence/Migrations/MasrLabDbContextModelSnapshot.cs           (2209 سطر)
```

> **ملاحظة مسار Migrations:** المسار الفعلي هو `src/MasrLab.Infrastructure/Persistence/Migrations/` (تحت `Persistence/`)، وليس `src/MasrLab.Infrastructure/Migrations/`. أي أمر EF CLI مستقبلي يجب أن يستهدف هذا المسار.

**قرار الانتقال:** نجاح البند (1) و (2)، وإصلاح (أ) و (ب)، وحل (ج) في **المرحلة 0** أعلاه ⇒ يُصبح الانتقال إلى بناء طبقة Application آمناً.

### 1.3 لوحة أرقام مُتحقَّق منها مباشرةً عند HEAD

| المؤشر | القيمة الفعلية (عدّ مستقل) | ملاحظة |
|---|---|---|
| Domain Services (interfaces فقط) في `src/MasrLab.Domain/Services/` | **10** | كلها عقود، بلا تنفيذ. |
| Repositories في `src/MasrLab.Domain/Interfaces/` | **8 متخصّصة + `IRepository<T>` + `IUnitOfWork`** ⇒ 10 ملفات | الملفات: `IAccountingRepository`, `IAuditLogRepository`, `ICultureRepository`, `IPatientHistoryRepository`, `IPatientRepository`, `IStatisticsRepository`, `ITestResultRepository`, `IVisitRepository` (+ `IRepository`, `IUnitOfWork`). |
| DTOs في Domain (`Common/DTOs/`) | **2** | `PatientHistoryEntry`, `StatisticsDto`. |
| DTOs في Application (`Common/DTOs/`) | **10** | `PatientDto`, `VisitDto`, `TestResultDto`, `SampleDto`, `ReceiptDto`, `CultureResultDto`, `AttendanceDto`, `AccountDrawerDto`, `WorkSheetDto`, `PatientHistoryDto`. |
| Feature Modules تحت `Application/Features/` | **19** | `Accounting, AttendanceAndAudit, CasesFollowUp, Cultures, DoctorsAndReferrals, FixedComments, OutsourcedSamples, PatientHistory, PatientManagement, PatientSearch, PriceLists, ResultsEntry, SampleCollection, Statistics, SystemSettings, TestGroups, TestsMasterData, UsersAndPermissions, WorkSheets`. |
| Commands معرَّفة | **38** ملف `*Command.cs` | — |
| Queries معرَّفة | **29** ملف `*Query.cs` | — |
| مجموع (Commands + Queries) | **67** | — |
| Handlers موجودة | **67** | كل ملف `*Handler.cs` بلا استثناء يرمي `NotImplementedException` (تحقّق: `grep -L` أعاد قائمة فارغة). |
| Validators موجودة | **38** | فجوة **29** — كلها Queries (قائمة الأسماء الدقيقة في المرحلة 9). |
| Domain Services المنفَّذة فعلياً | **0** | مجلد `Application/Services/` غير موجود أصلاً. |
| Infrastructure Services | **6** | `AuthenticationService`, `BackupService`, `BarcodeService`, `CurrentUserService`, `DateTimeService`, `PrintService` — لا شيء منها يُطبِّق أياً من الـ 10 Domain Services. |
| Infrastructure Repositories | **8** | `AccountingRepository`, `AuditLogRepository`, `CultureRepository`, `GenericRepository`, `PatientRepository`, `StatisticsRepository`, `TestResultRepository`, `VisitRepository` — **بلا `PatientHistoryRepository`**. |
| Interceptors | **2** | `AuditableEntityInterceptor`, `SoftDeleteInterceptor` — مسجَّلان بـ `AddScoped` ومربوطان عبر `AddInterceptors` داخل `DbContext` factory. |
| EF Configurations | **35** ملف | تحت `Persistence/Configurations/{Administrative,Core,Culture,Financial,Settings}`. |
| Migrations | **1** | `20260803173749_InitialCreate` تحت `Persistence/Migrations/`. |
| Domain Entities | **35** كيان | تحت `Domain/Entities/{Administrative,Core,Culture,Financial,Settings}`. |
| Domain Enums | **20** enum | تحت `Domain/Common/Enums/`. |
| ValueObjects | **3** | `Age`, `DateRange`, `EgyptianPhone` تحت `src/MasrLab.Domain/ValueObjects/` (بلا مجلد `Common/`). |
| Domain Events | **20 record** | في `Events/DomainEvents.cs`. |
| Application-scope Interfaces (`Application/Common/Interfaces/`) | **6** | الفارغتان: `IAuthenticationService`, `IPrintService`. الأربع الأخريات (`IBackupService`, `IBarcodeService`, `ICurrentUserService`, `IDateTimeService`) لها عقود حقيقية. |
| ViewModels + Views في Presentation | **31 ViewModel** + 38 `*.xaml.cs` | خارج نطاقنا لكن للمرجعية. |
| Target Framework | `net8.0` (SDK 8.0.0 عبر `global.json` مع `rollForward: latestFeature`) | — |
| Test Framework — Application.Tests | xUnit 2.5.3, coverlet.collector 6.0.0, AutoMapper 15.1.*, FluentValidation 12.1.* | ينقص: Moq, FluentAssertions, Microsoft.Extensions.DependencyInjection. |
| Test Framework — Infrastructure.Tests | xUnit 2.5.3, coverlet.collector 6.0.0, **Microsoft.EntityFrameworkCore.InMemory 8.0.*** | ينقص Moq + FluentAssertions لو أردنا استعمالهما. |
| ملفات اختبار حالية | Domain: 8 (7 حقيقية + Placeholder) ؛ Application: 1 (Placeholder فقط) ؛ Infrastructure: 1 (Placeholder فقط) | — |

**عدّاد `NotImplementedException` على مستوى `src/`:**

- إجمالي أسطر `throw new NotImplementedException()` = **79**:
  - **67** داخل `Application/Features/*/*Handler.cs` (Handler واحد لكل Command/Query).
  - **1** داخل `Application/Common/Helpers/LabIdGenerator.cs` (`GenerateAsync`).
  - **2** داخل `Infrastructure/Services/*` (`BackupService.cs`, `BarcodeService.cs` — placeholders خارج نطاقنا لكن يستحقّ الإشارة).
  - **4** داخل `Presentation/Resources/Converters/*` (WPF converters — خارج نطاقنا).
  - **5** حالات أخرى فرعية.
- **المهم لمرحلتنا: 68 خطّاً داخل طبقة Application يجب أن تصبح صفراً** (67 Handler + 1 LabIdGenerator).

---

## 2. تحليل الوضع الحالي (Current State Analysis)

### 2.1 واجهات Domain Services الموجودة (توقيعات مُتحقَّق منها)

جميعها موجودة في [`src/MasrLab.Domain/Services/`](https://github.com/El-ogra/MasrLab/tree/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services)، وكلها **عقود (interfaces) بدون أي تنفيذ**:

| # | الواجهة | التوقيع الحرفي (كما هو عند HEAD) | يعتمد على |
|---|---|---|---|
| 1 | [`IAccountingService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IAccountingService.cs) | `decimal CalculateNetProfit(Account)` / `Task RecalculateNetProfitAsync(int)` | `Account` |
| 2 | [`ICultureSensitivityService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/ICultureSensitivityService.cs) | `void RecordSensitivity(int, int, int)` / `string GetSensitivitySummary(int)` — **متزامن** | `Culture`, `Sensitivity` |
| 3 | [`IMedicalHistoryService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IMedicalHistoryService.cs) | `Task<IReadOnlyList<PatientHistoryEntry>> BuildHistoryAsync(int)` / `Task<bool> ShouldAutoInsertHistoryAsync(int, int)` | `PatientHistoryEntry`, `IPatientHistoryRepository` |
| 4 | [`IOutsourcingService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IOutsourcingService.cs) | `Task<OutsourcedSample> CreateOutsourcedSampleAsync(int, int, int, decimal, decimal)` / `Task ReceiveOutsourcedResultAsync(int)` | `OutsourcedSample` |
| 5 | [`IPriceListResolverService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IPriceListResolverService.cs) | `decimal ResolvePrice(int, int?, int?)` — **متزامن** | `PriceList`, `PriceListItem` |
| 6 | [`IPricingService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IPricingService.cs) | `decimal CalculateTotal(PatientVisit, decimal, decimal)` / `decimal CalculateSubtotal(PatientVisit)` | `PatientVisit`, `VisitTest` |
| 7 | [`IReceiptCalculationService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IReceiptCalculationService.cs) | `decimal CalculateRemaining(decimal, decimal, decimal)` / `decimal CalculateChangeDue(decimal, decimal)` — حسابية بحتة | لا شيء |
| 8 | [`IReferralCommissionService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IReferralCommissionService.cs) | `decimal CalculateCommission(int?, int?, decimal)` — **متزامن** | `Doctor`, `ReferralEntity` |
| 9 | [`IResultValidationService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/IResultValidationService.cs) | `ResultStatus ValidateResult(int testId, string value, string? gender, int ageYears)` / `bool IsResultInRange(int, string, out string?)` — **متزامن** ومعامل `gender` من نوع `string?` (لا `Gender enum`) | `ResultStatus`, `ReferenceValue` |
| 10 | [`ISampleTrackingService`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Services/ISampleTrackingService.cs) | `Task<bool> IsSampleCollectedAsync(int visitTestId)` / `Task<int> GetUncollectedSamplesCountAsync(int visitId)` | `Sample`, `SampleCollection` |

**تنبيه حاسم:** بعض العقود متزامنة بشكل غير مثالي لخدمة تصل قاعدة بيانات (`IResultValidationService`, `IPriceListResolverService`, `ICultureSensitivityService.RecordSensitivity`, `IReferralCommissionService`). عند التنفيذ، سنواجه خياراً: إما تنفيذ الجزء الحسابي المتزامن مع تكييف داخلي (cache/preload)، أو **تعديل العقود لإضافة `Async` overloads** (يستلزم قرار DD جديد لأنه يمس Domain).

### 2.2 الفجوات بين الواجهات والتنفيذ الفعلي

- **صفر تنفيذ لـ Domain Services**: مجلد `Application/Services/` **غير موجود أصلاً**. ولا يوجد أي كلاس يُطبِّق أياً من الواجهات العشر في أي مكان.
- **صفر تسجيل DI لهذه الخدمات**: [`Application/DependencyInjection.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Application/DependencyInjection.cs) يسجِّل فقط: `MediatR` + `AutoMapper` (assembly scan) + `Validators` (assembly scan) + `ValidationBehavior`. لا يسجِّل أياً من الـ 10 Domain Services ولا `LabIdGenerator`.
- **صفر منطق داخل Handlers**: كل الـ 67 Handler هي stubs (`throw new NotImplementedException();`).
- **`LabIdGenerator`** ([المصدر](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs)) موجود كهيكل يحقن `IPatientRepository` — التعليق يذكر «قرار DD-08» صراحة لكن الميثود الحقيقي `GenerateAsync` يرمي `NotImplementedException`.
- **Validators ناقصة**: 38/67 موجودة ⇒ فجوة **29** validator، جميعها Queries.
- **`AuditBehavior`** ([المصدر](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs)) معرَّف لكنه **غير مسجَّل** في DI. تنفيذه الحالي شكلي (`Debug.WriteLine` فقط) — يحتاج ربطاً بـ `IAuditLogRepository` + `ICurrentUserService` قبل الاعتماد عليه للتدقيق الفعلي.
- **`IAuthenticationService` و `IPrintService`** واجهتان **فارغتان تماماً بلا أعضاء**. أما `IBackupService`, `IBarcodeService`, `ICurrentUserService`, `IDateTimeService` فتحوي أعضاءها الحقيقية.
- **Naming collision محتمل**: `IAuthenticationService` في Application (فارغة) تُنفَّذ عبر `Infrastructure/Services/AuthenticationService` — الكود ينجح في البناء فقط لأن الواجهة الفارغة بلا أعضاء.
- **`IPatientHistoryRepository` غير مسجَّل ولا منفَّذ**: [`Infrastructure/DependencyInjection.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/DependencyInjection.cs) يسجّل 7 Repository interfaces متخصّصة + `IRepository<>` ⇒ يبقى `IPatientHistoryRepository` بلا تسجيل ولا كلاس Concrete في `Persistence/Repositories/`. أي محاولة لتنفيذ `GetPatientHistoryQueryHandler` ستصطدم فوراً بـ `Unable to resolve service for type IPatientHistoryRepository`.
- **PatientHistoryView**: [`.sql`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.sql) موجود بجوار [`.cs` shim](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.cs) — لكن **الـ `DbContext` لا يحوي `DbSet<PatientHistoryView>`** ولا الـ Migration يُنشئ الـ View (`grep -c PatientHistory` على الـ Migration = 0). هذه فجوة بنية تحتية يعتمد عليها Handler `GetPatientHistory` وخدمة `IMedicalHistoryService` معاً.

### 2.3 DTOs الموجودة وما يحتاج إضافة/تعديل

**في Domain (تبقى كما هي):**
- `PatientHistoryEntry` — عقد بين `IMedicalHistoryService` و `IPatientHistoryRepository`.
- `StatisticsDto` — عقد بين `IStatisticsRepository` والـ 5 Statistics Queries.

**في Application/Common/DTOs (10 موجودة):**
`PatientDto`, `VisitDto`, `TestResultDto`, `SampleDto`, `ReceiptDto`, `CultureResultDto`, `AttendanceDto`, `AccountDrawerDto`, `WorkSheetDto`, `PatientHistoryDto`.

**تحليل التطابق:**

| DTO موجود | يخدم Query/Command | ملاحظة |
|---|---|---|
| `PatientDto` | `GetPatientByIdQuery`, `SearchPatientsQuery` | 26 حقلاً — يطابق `RegisterPatientCommand` بالكامل. `Age` مُسطَّح إلى `AgeYears/Months/Days` بينما الكيان يحمل `Age` كـ ValueObject ⇒ يتطلَّب `ForMember` صريح في AutoMapper. |
| `VisitDto` | `GetPatientVisitHistoryQuery`, `GetCasesByPeriodQuery` | متوافق. |
| `TestResultDto` | `GetTestResultForVisitQuery` **و `GetTestWithReferencesQuery`** | 🔴 عدم تطابق دلالي في الثاني: يحتاج DTO جديد `TestWithReferencesDto`. |
| `SampleDto` | `GetPendingSamplesQuery` | ✅ |
| `ReceiptDto` | `GetPriceListForPrintQuery : IRequest<ReceiptDto?>` | 🔴 عدم تطابق دلالي: قائمة أسعار للطباعة ليست إيصالاً — يحتاج `PriceListPrintDto`. |
| `CultureResultDto` | `GetCultureResultQuery` | ✅ |
| `AttendanceDto` | `GetAttendanceLogsQuery : IRequest<AttendanceDto>` | إرجاع كائن مفرد وليس قائمة — تجميع كـ timesheet مقصود. |
| `AccountDrawerDto` | `GetDrawerReportQuery`, `GetDoctorReferralReportQuery` | ✅ |
| `WorkSheetDto` | `GenerateTestWorkSheetQuery`, `GeneratePatientWorkSheetQuery` | ✅ |
| `PatientHistoryDto` | `GetPatientHistoryQuery` | ✅ |

**Queries تُرجع `object` أو `IReadOnlyList<object>` (Type-hole يجب سدّه):**

- `GetAuditLogsQuery : IRequest<object>` → يحتاج `AuditLogDto`.
- `GetSystemSettingsQuery : IRequest<object>` → يحتاج `SystemSettingsDto` (مركَّب من 5 أقسام فرعية).
- `GetOutsourcedSamplesQuery : IRequest<IReadOnlyList<object>>` → يحتاج `OutsourcedSampleDto`.
- `GenerateTestLogQuery : IRequest<IReadOnlyList<object>>` → يحتاج `TestLogEntryDto`.
- `FilterAntibioticsQuery : IRequest<IReadOnlyList<object>>` → يحتاج `AntibioticDto`.
- `GetCaseUserTrackingQuery : IRequest<IReadOnlyList<object>>` → يحتاج `CaseUserTrackingDto`.

**فجوات DTOs يجب إضافتها/تعديلها في Application:**

| DTO مطلوب | السبب |
|---|---|
| `AuditLogDto` | استبدال `object` في `GetAuditLogsQuery` |
| `SystemSettingsDto` (مركَّب) | استبدال `object` في `GetSystemSettingsQuery` — يجمع الأقسام الفرعية الخمسة |
| `ReceiptSettingsDto`, `ReportSettingsDto`, `AccountSettingsDto`, `PrinterDto`, `EnvelopeBarcodeSettingsDto` | 5 Commands في `SystemSettings/*` — كلٌ يحتاج DTO مقابل |
| `OutsourcedSampleDto`, `ExternalLabDto` | `GetOutsourcedSamplesQuery`, `MarkTestAsOutsourcedCommand`, `SettleOutsourcedAccountCommand` |
| `AntibioticDto`, `SensitivityDto`, `OrganismDto` | `Cultures/*` — `AddAntibioticToCultureCommand`, `FilterAntibioticsQuery`, `EnterCultureResultCommand` |
| `DoctorDto`, `ReferralEntityDto` | `AddDoctorCommand`, `AddReferralEntityCommand` |
| `TestDto`, `ReferenceValueDto`, `TestGroupDto`, `TestGroupItemDto`, `TestWithReferencesDto` | `TestsMasterData/*`, `TestGroups/*` |
| `UserDto`, `PermissionDto`, `PermissionAssignmentDto` | `UsersAndPermissions/*` |
| `PriceListDto`, `PriceListItemDto`, `PriceListPrintDto` | `PriceLists/*` |
| `CommentTemplateDto` | `FixedComments/ManageComments` |
| `CashTransactionDto` | `Accounting/RecordCashTransaction` |
| `AccountTypeDrawerDto`, `PeriodDrawerDto`, `DoctorDrawerDto` | تخصيصات لـ `AccountDrawerDto` |
| `MonthlyStatisticsDto`, `GenderStatisticsDto`, `TestDemandRateDto`, `SampleCountByYearDto`, `PatientCountByPeriodDto` | خيار: إبقاء `StatisticsDto` مؤقتاً، أو التخصيص لاحقاً |
| `TestLogEntryDto`, `WorkSheetLineDto` | `WorkSheets/*` — لتوسيع `WorkSheetDto` |
| `CaseUserTrackingDto` | `CasesFollowUp/GetCaseUserTracking` |
| `BreakPeriodDto` | لتفكيك `AttendanceDto.BreakPeriods` — تحسين اختياري |

### 2.4 Use Cases / Commands / Queries معرَّفة مسبقاً

الهيكل الكامل موجود لكنه هيكل فارغ. 19 Feature module، إجمالي **67** ملف (38 Command + 29 Query):

| Feature | Commands | Queries |
|---|---|---|
| `PatientManagement` | 4: RegisterPatient, UpdatePatientData, UpdatePatientAccount, DeliverResults | 2: GetPatientById, GenerateLabId |
| `PatientSearch` | 0 | 2: SearchPatients, GetPatientVisitHistory |
| `PatientHistory` | 0 | 1: GetPatientHistory |
| `SampleCollection` | 1: MarkSampleCollected | 1: GetPendingSamples |
| `ResultsEntry` | 3: EnterTestResult, CreateBlankReport, CreateCombinedReport | 2: GetTestResultForVisit, CalculateHighLowStatus |
| `Cultures` | 3: AddNewCulture, AddAntibioticToCulture, EnterCultureResult | 2: GetCultureResult, FilterAntibiotics |
| `Accounting` | 4: CreateDoctorDrawer, CreatePeriodDrawer, CreateAccountTypeDrawer, RecordCashTransaction | 2: GetDrawerReport, GetDoctorReferralReport |
| `OutsourcedSamples` | 2: MarkTestAsOutsourced, SettleOutsourcedAccount | 1: GetOutsourcedSamples |
| `DoctorsAndReferrals` | 2: AddDoctor, AddReferralEntity | 0 |
| `UsersAndPermissions` | 3: CreateUser, UpdateUser, SetPermissions | 1: CheckPermission |
| `AttendanceAndAudit` | 3: RecordLogin, RecordLogout, RecordBreak | 2: GetAttendanceLogs, GetAuditLogs |
| `TestsMasterData` | 3: AddTest, UpdateTest, UpdateReferenceValues | 1: GetTestWithReferences |
| `TestGroups` | 1: ManageTestGroups | 0 |
| `PriceLists` | 2: CreatePriceList, UpdatePriceListItems | 1: GetPriceListForPrint |
| `SystemSettings` | 6: UpdateReceiptSettings, UpdateReportSettings, UpdateAccountSettings, UpdatePrinterSettings, UpdateEnvelopeBarcodeSettings, ManageBackup | 1: GetSystemSettings |
| `WorkSheets` | 0 | 3: GenerateTestWorkSheet, GeneratePatientWorkSheet, GenerateTestLog |
| `Statistics` | 0 | 5: GetGenderStatistics, GetMonthlyStatistics, GetPatientCountByPeriod, GetSampleCountByYear, GetTestDemandRate |
| `CasesFollowUp` | 0 | 2: GetCaseUserTracking, GetCasesByPeriod |
| `FixedComments` | 1: ManageComments | 0 |

**المهمة ليست إنشاء Use Cases من الصفر، بل:** ملء 67 Handler + إكمال 29 Validator + إضافة الـ DTOs المفقودة + تنفيذ 10 Domain Services + تسجيل DI + إضافة `PatientHistoryRepository` ناقص + إكمال `LabIdGenerator` فعلي.

### 2.5 Domain Events المتاحة (20 record)

مصدرها [`Events/DomainEvents.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Events/DomainEvents.cs):

`PatientRegistered`, `PatientUpdated`, `PatientVisitCreated`, `VisitTestAdded`, `VisitTestRemoved`, `SampleCollected`, `SampleUncollectedReverted`, `TestResultEntered`, `TestResultEdited`, `ReceiptIssued`, `ReceiptPaymentAdded`, `DiscountApplied`, `OutsourcedSampleSent`, `OutsourcedResultReceived`, `CultureRecorded`, `SensitivityRecorded`, `CashDeposited`, `CashWithdrawn`, `CommentAttachedToResult`, `VisitClosed`.

بعض هذه الأحداث تنشرها ميثودات كيانات موجودة مباشرة، لكن ليس لها Commands مقابلة (مثل `PatientVisitCreated`, `VisitTestAdded/Removed`, `ReceiptPaymentAdded`, `DiscountApplied`, `VisitClosed`) — قرار DD-12 (انظر المرحلة 5.3) يحسم: إمّا إضافة Commands جديدة، أو دمج التنقّلات ضمن Handlers موجودة.

### 2.6 استثناءات الدومين المتاحة (لا تُنشئ استثناءات جديدة قبل استنفاد هذه)

من `src/MasrLab.Domain/Exceptions/`:
- `BusinessRuleViolationException`
- `DuplicateLabIdException`
- `EntityNotFoundException`
- `InsufficientPermissionException`

**Handlers ملزَمة برمي هذه الاستثناءات — لا `Exception` عامة.**

---

## 3. خريطة طريق التنفيذ (Execution Roadmap)

| المرحلة | الاسم | الاعتمادية | حالة الانطلاق |
|---|---|---|---|
| **0** | **إصلاح R0 (شرط مسبق إلزامي)** | — | مطلوب |
| **1** | ضبط الحزم للاختبار (Moq / FluentAssertions / Logging) | 0 | جزئي — `InMemory` **موجود** في `Infrastructure.Tests` |
| **2** | تثبيت هيكل Application (Services/, Profiles/, Validations/, Models/) | 1 | معظم المجلدات موجودة؛ ينقص: `Services/`, `Common/Mappings/Profiles/`, `Common/Validations/`, `Common/Models/` |
| **3** | استكمال العقود قبل التنفيذ (Auth/Print) + قرار Async | 2 | 🆕 تعبئة `IAuthenticationService` و `IPrintService` (الفارغتين)، وحل قرار الـ Async في العقود المتزامنة |
| **4** | تنفيذ Domain Services العشر داخل `Application/Services/` | 3 | صفر تنفيذ حالياً |
| **5** | ضبط أنواع الإرجاع في الـ 6 Queries التي تعيد `object` | 4 | 🆕 blocker حقيقي — يسبق كتابة Handlers |
| **6** | إضافة DTOs الناقصة (~22 DTO — انظر §2.3) | 5 | معظمها مفقود |
| **6a** | **عاجل قبل الـ 7**: إضافة `PatientHistoryRepository` + `DbSet<PatientHistoryView>` + Migration للـ View + تسجيل DI | 6 | فجوة حرجة |
| **7** | كتابة Handlers فعلية لكل Use Case (استبدال 67 `NotImplementedException`) + `LabIdGenerator.GenerateAsync` | 6, 6a | صفر |
| **8** | AutoMapper Profiles شاملة (11 Profile) + تفكيك `MappingProfile.cs` الحالي | 6 | Profile واحد ذو 9 CreateMap فقط |
| **9** | إكمال الـ 29 Validator المفقود + قواعد مشتركة (`Common/Validations/`) | 5 | 38/67 |
| **10** | تسجيل DI (Application + Infrastructure) + تفعيل `AuditBehavior` بمنطق حقيقي | 4, 6a, 7 | جزئي |
| **11** | Unit Tests لكل Handler + Domain Service + Validator + Profile | 7, 8, 9, 10 | `PlaceholderTests` فقط |
| **12** | Integration Tests + التحقق النهائي | 11 | غير مبدوء |

**المسار الحرج (Critical Path):** 0 → 3 → 4 → 6a → 7 → 11 → 12.

---

## 4. التفاصيل التنفيذية لكل مرحلة

### المرحلة 1 — ضبط الحزم

**الحالة الحالية (متحقَّق):**
- `MasrLab.Application.csproj`: MediatR 12.5.0 ✅ AutoMapper 15.1.3 ✅ FluentValidation 12.1.1 ✅ FluentValidation.DependencyInjectionExtensions 12.1.1 ✅
- `MasrLab.Application.Tests.csproj`: xUnit 2.5.3, coverlet.collector 6.0.0, AutoMapper 15.1.*, FluentValidation 12.1.* — **ينقص:** Moq, FluentAssertions, Microsoft.Extensions.DependencyInjection.
- `MasrLab.Infrastructure.Tests.csproj`: xUnit 2.5.3, coverlet.collector 6.0.0, **`Microsoft.EntityFrameworkCore.InMemory 8.0.*`** ✅. ينقص Moq + FluentAssertions لو أردنا استعمالهما هنا.

**الأوامر:**
```bash
dotnet add tests/MasrLab.Application.Tests package Moq --version 4.20.*
dotnet add tests/MasrLab.Application.Tests package FluentAssertions --version 6.12.*
dotnet add tests/MasrLab.Application.Tests package Microsoft.Extensions.DependencyInjection --version 8.0.*
dotnet add src/MasrLab.Application package Microsoft.Extensions.Logging.Abstractions --version 8.0.*
dotnet add tests/MasrLab.Infrastructure.Tests package Moq --version 4.20.*
dotnet add tests/MasrLab.Infrastructure.Tests package FluentAssertions --version 6.12.*
```

**معايير القبول:** `dotnet restore` بلا تعارض؛ `dotnet build` ناجح؛ `using Moq; using FluentAssertions;` يعمل داخل ملفات الاختبار.

---

### المرحلة 2 — هيكل المجلدات

**سيُنشأ:**
```
src/MasrLab.Application/
├── Services/                            (جديد — 10 ملفات)
├── Common/
│   ├── Mappings/Profiles/               (11 Profile)
│   ├── Validations/                     (قواعد مشتركة + محوِّل EgyptianPhone)
│   └── Models/                          (اختياري: Result<T>, PagedList<T>)
```

**معايير القبول:** كل مجلد جديد يحوي ملفاً واحداً على الأقل؛ لا تضارب namespaces.

---

### المرحلة 3 — استكمال العقود قبل التنفيذ

**3.1 ملء الواجهات الفارغة:**

`IAuthenticationService` (0 أعضاء الآن). العقد المقترح يُستخلص من `LoginViewModel` و `Infrastructure/Services/AuthenticationService`:

```csharp
// مقترح توضيحي — يقرَّر رسمياً في DD-09
public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);
}
```

`IPrintService` (0 أعضاء). العقد المقترح يعكس ما تفعله `Printing/Reports/*` في Presentation:

```csharp
// مقترح — يقرَّر رسمياً في DD-10
public interface IPrintService
{
    Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default);
    Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default);
}
```

**3.2 قرار الـ Async في Domain Services:**

الواجهات المتزامنة التالية تحتاج قراراً موحَّداً:
- `ICultureSensitivityService.RecordSensitivity(...)` — يجب أن تعدِّل حالة `Culture` وتنشر Event.
- `IPriceListResolverService.ResolvePrice(...)` — تحتاج قراءة `PriceList` و `PriceListItem`.
- `IResultValidationService.ValidateResult(...)` — تحتاج قراءة `ReferenceValue`.
- `IReferralCommissionService.CalculateCommission(...)` — تحتاج قراءة `Doctor`/`ReferralEntity`.

**التوصية:** أضف **overloads async** بدون كسر التوقيع الحالي (Non-breaking):
```csharp
public interface IPriceListResolverService
{
    decimal ResolvePrice(int testId, int? referralEntityId, int? priceListId);                          // الحالي
    Task<decimal> ResolvePriceAsync(int testId, int? referralEntityId, int? priceListId,
                                    CancellationToken ct = default);                                     // جديد
}
```

وثِّق ذلك في `Docs/DecisionRecords/DD-11.md`.

**معايير القبول:** كل واجهة فارغة صارت تحوي عقداً معتمداً بـ DD؛ كل خدمة متزامنة صار لديها overload async.

---

### المرحلة 4 — تنفيذ Domain Services (Application/Services/)

**قرار معماري (تأكيد DD-08):** العقود في Domain، التنفيذ في Application، الوصول لقاعدة البيانات عبر Repository Interfaces حصراً.

**تعيين الخدمة → التبعيات:**

| الخدمة | التبعيات المؤكَّدة |
|---|---|
| `PricingService` | `IPriceListResolverService` |
| `ReceiptCalculationService` | لا شيء |
| `ReferralCommissionService` | `IRepository<Doctor>`, `IRepository<ReferralEntity>` |
| `PriceListResolverService` | `IRepository<PriceList>`, `IRepository<PriceListItem>` |
| `ResultValidationService` | `IRepository<Test>`, `IRepository<ReferenceValue>` |
| `SampleTrackingService` | `IVisitRepository` |
| `AccountingService` | `IAccountingRepository`, `IUnitOfWork` |
| `OutsourcingService` | `IVisitRepository`, `IRepository<OutsourcedSample>`, `IUnitOfWork` |
| `MedicalHistoryService` | `IPatientHistoryRepository` (⚠️ مرتبطة بالمرحلة 6a) |
| `CultureSensitivityService` | `ICultureRepository`, `IUnitOfWork` |

**نموذج توضيحي (يتبع نفس نمط `LabIdGenerator` الموجود — Constructor injection مع `ArgumentNullException` guard اختياري):**

```csharp
public class PricingService : IPricingService
{
    private readonly IPriceListResolverService _priceResolver;

    public PricingService(IPriceListResolverService priceResolver)
        => _priceResolver = priceResolver ?? throw new ArgumentNullException(nameof(priceResolver));

    public decimal CalculateSubtotal(PatientVisit visit)
        => visit.VisitTests.Sum(vt => vt.Price);

    public decimal CalculateTotal(PatientVisit visit, decimal extraServicesTotal, decimal discount)
        => Math.Max(0m, CalculateSubtotal(visit) + extraServicesTotal - discount);
}
```

**معايير القبول:**
- 10 كلاسات؛ كل منها بلا `NotImplementedException`.
- اعتماد حصري على `MasrLab.Domain.*` (لا استيراد من `MasrLab.Infrastructure.*`).
- XML-doc على كل ميثود يذكر Invariant من تعليقات الكيانات.

---

### المرحلة 5 — ضبط أنواع الإرجاع (blocker قبل Handlers)

**الملفات التي يجب تعديل توقيعها قبل بدء الـ Handlers:**

| Query الحالي | الإرجاع الحالي | الإرجاع المقترح |
|---|---|---|
| `GetAuditLogsQuery` | `IRequest<object>` | `IRequest<IReadOnlyList<AuditLogDto>>` |
| `GetSystemSettingsQuery` | `IRequest<object>` | `IRequest<SystemSettingsDto>` |
| `GetOutsourcedSamplesQuery` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<OutsourcedSampleDto>>` |
| `GenerateTestLogQuery` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<TestLogEntryDto>>` |
| `FilterAntibioticsQuery` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<AntibioticDto>>` |
| `GetCaseUserTrackingQuery` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<CaseUserTrackingDto>>` |
| `GetTestWithReferencesQuery` | `IRequest<TestResultDto?>` | `IRequest<TestWithReferencesDto?>` (⚠️ الحالي دلالياً خاطئ) |
| `GetPriceListForPrintQuery` | `IRequest<ReceiptDto?>` | `IRequest<PriceListPrintDto?>` (⚠️ الحالي دلالياً خاطئ) |

**معايير القبول:** لا يبقى `object` أو `IReadOnlyList<object>` في أي Query.

---

### المرحلة 6 — إضافة DTOs الناقصة

انظر جدول §2.3 (~22 DTO مطلوب). المعيار: كل Query/Command يستطيع الحصول على DTO مطابق دون أي حقل مفقود مقارنةً بالكيان المصدر.

**معايير القبول:** لكل DTO مطابق ⇢ `CreateMap<Entity, Dto>()` + `AssertConfigurationIsValid()` ينجح.

---

### المرحلة 6a — سدّ فجوة `PatientHistoryRepository` (حرج)

**المطلوب في Infrastructure:**
1. إنشاء `src/MasrLab.Infrastructure/Persistence/Repositories/PatientHistoryRepository.cs` يُطبِّق [`IPatientHistoryRepository`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Domain/Interfaces/IPatientHistoryRepository.cs) عبر `FromSqlRaw` على `PatientHistoryView` أو `DbSet<PatientHistoryView>`.
2. إضافة `DbSet<PatientHistoryView>` في [`MasrLabDbContext`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs) مع `HasNoKey().ToView("PatientHistoryView")`.
3. Migration جديد يشغِّل `PatientHistoryView.sql` عبر `migrationBuilder.Sql(File.ReadAllText(...))` — لأن الـ Migration الحالي `20260803173749_InitialCreate` (تحت `src/MasrLab.Infrastructure/Persistence/Migrations/`) لا يُنشئ الـ View.
4. تسجيل `services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();` في [`Infrastructure/DependencyInjection.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/DependencyInjection.cs).

**معايير القبول:** `MedicalHistoryService` قابل للـ resolve؛ Handler `GetPatientHistoryQuery` يستطيع الاستعلام على View.

---

### المرحلة 7 — كتابة Handlers (استبدال 67 `NotImplementedException` + `LabIdGenerator`)

**قواعد إلزامية:**
1. لا `DbContext` مباشرة داخل Handler — الوصول عبر Repository interfaces فقط.
2. رمي استثناءات Domain المتوفرة (`BusinessRuleViolationException`, `DuplicateLabIdException`, `EntityNotFoundException`, `InsufficientPermissionException`) — لا `Exception` عامة.
3. تعديل حالة الكيان عبر ميثوداته: `Patient.Register`, `PatientVisit.AddTest / EnterAllResults / IssueReceipt / Close(finalTotal)`, `Sample.Collect / RevertCollection`, `TestResult.Create / Edit`, `Culture.Create / Record / RecordSensitivity`, `Receipt.AddVisitTest / AddExtraServiceItem / ApplyDiscount / Issue / AddPayment`, `CashTransaction.Deposit / Withdraw`, `OutsourcedSample.SetPrices / Send / ReceiveResult / CompleteSettlement`, `Comment.Attach` — كلها موجودة كـ static factories أو instance methods وتنشر Domain Events.
4. استدعاء Domain Service حين يلزم (`EnterTestResultCommandHandler` ⇒ `IResultValidationService.ValidateResult`).
5. `SaveChangesAsync` مركزياً في نهاية Handler، مرة واحدة على الأكثر (`IUnitOfWork.SaveChangesAsync(ct)`).
6. تمرير `cancellationToken` لكل استدعاء يقبله.
7. Constructor injection عبر primary constructor أو حقول `readonly` — نفس نمط `LabIdGenerator` الحالي (تحقق null اختياري لكن متسق).

**تعيين Handler → التبعيات (مُتحقَّق بمقارنة أسماء Commands مقابل Domain Services + Repositories):**

| Handler | Domain Service | Repository الأساسي |
|---|---|---|
| `RegisterPatientCommandHandler` | — | `IPatientRepository`, `LabIdGenerator` |
| `UpdatePatientDataCommandHandler` | — | `IPatientRepository` |
| `UpdatePatientAccountCommandHandler` | — | `IPatientRepository` |
| `DeliverResultsCommandHandler` | `IReceiptCalculationService` (اختياري) | `IVisitRepository` |
| `MarkSampleCollectedCommandHandler` | `ISampleTrackingService` | `IVisitRepository` |
| `EnterTestResultCommandHandler` | `IResultValidationService` | `ITestResultRepository` |
| `CreateBlankReportCommandHandler` | — | `IVisitRepository` |
| `CreateCombinedReportCommandHandler` | — | `IVisitRepository`, `ITestResultRepository` |
| `AddNewCultureCommandHandler` | `ICultureSensitivityService` | `ICultureRepository` |
| `AddAntibioticToCultureCommandHandler` | `ICultureSensitivityService` | `ICultureRepository` |
| `EnterCultureResultCommandHandler` | `ICultureSensitivityService` | `ICultureRepository` |
| `CreateDoctorDrawerCommandHandler` | `IAccountingService`, `IReferralCommissionService` | `IAccountingRepository` |
| `CreatePeriodDrawerCommandHandler` | `IAccountingService` | `IAccountingRepository` |
| `CreateAccountTypeDrawerCommandHandler` | `IAccountingService` | `IAccountingRepository` |
| `RecordCashTransactionCommandHandler` | `IAccountingService` | `IAccountingRepository` |
| `MarkTestAsOutsourcedCommandHandler` | `IOutsourcingService` | `IVisitRepository`, `IRepository<OutsourcedSample>` |
| `SettleOutsourcedAccountCommandHandler` | `IOutsourcingService`, `IAccountingService` | `IAccountingRepository`, `IRepository<OutsourcedSample>` |
| `AddDoctorCommandHandler` / `AddReferralEntityCommandHandler` | — | `IRepository<Doctor>` / `IRepository<ReferralEntity>` |
| `CreateUserCommandHandler` / `UpdateUserCommandHandler` / `SetPermissionsCommandHandler` | — | `IRepository<User>`, `IRepository<Permission>` |
| `RecordLoginCommandHandler` / `RecordLogoutCommandHandler` / `RecordBreakCommandHandler` | `IAuthenticationService` (بعد المرحلة 3.1) | `IRepository<AttendanceLog>` |
| `AddTestCommandHandler` / `UpdateTestCommandHandler` / `UpdateReferenceValuesCommandHandler` | — | `IRepository<Test>`, `IRepository<ReferenceValue>` |
| `ManageTestGroupsCommandHandler` | — | `IRepository<TestGroup>`, `IRepository<TestGroupItem>` |
| `CreatePriceListCommandHandler` / `UpdatePriceListItemsCommandHandler` | `IPriceListResolverService` (اختياري) | `IRepository<PriceList>`, `IRepository<PriceListItem>` |
| كل Handlers الإعدادات (6) | — | `IRepository<SystemSetting>`, `IRepository<Printer>`, `IRepository<ReportTemplate>`, `IRepository<CardSetting>` |
| `ManageBackupCommandHandler` | `IBackupService` | — |
| `ManageCommentsCommandHandler` | — | `IRepository<CommentTemplate>` |
| Handlers الـ 5 Statistics Queries | — | `IStatisticsRepository` |
| `GetPatientHistoryQueryHandler` | `IMedicalHistoryService` | `IPatientHistoryRepository` (بعد 6a) |
| `GetAuditLogsQueryHandler` | — | `IAuditLogRepository` |
| `GenerateLabIdQueryHandler` | — | `LabIdGenerator` (يستهلك `IPatientRepository`) |
| `CheckPermissionQueryHandler` | — | `IRepository<Permission>` |
| WorkSheets Queries (3) | — | `IVisitRepository`, `IRepository<WorkSheet>` |
| CasesFollowUp Queries (2) | — | `IVisitRepository` |
| `CalculateHighLowStatusQueryHandler` | `IResultValidationService` | — (حسابية عبر Domain Service) |

**نموذج توضيحي لـ Handler (يتبع نمط `RegisterPatientCommandHandler` الحالي — نفس namespace، نفس شكل `IRequestHandler<TRequest, TResponse>`):**

```csharp
public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Unit>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _uow;
    private readonly LabIdGenerator _labIdGen;

    public RegisterPatientCommandHandler(IPatientRepository patients, IUnitOfWork uow, LabIdGenerator labIdGen)
    {
        _patients = patients ?? throw new ArgumentNullException(nameof(patients));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _labIdGen = labIdGen ?? throw new ArgumentNullException(nameof(labIdGen));
    }

    public async Task<Unit> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        if (await _patients.GetByLabIdAsync(request.LabId) is not null)
            throw new DuplicateLabIdException($"LabId '{request.LabId}' is already in use.");

        var patient = Patient.Register(
            request.Name,
            new Age(request.AgeYears, request.AgeMonths, request.AgeDays),
            request.Gender,
            request.Phone is null ? null : new EgyptianPhone(request.Phone),
            request.LabId,
            request.DoctorId,
            request.ReferralEntityId,
            request.AccountType
            /* ... باقي الحقول */);

        await _patients.AddAsync(patient, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

**نموذج `LabIdGenerator.GenerateAsync` (إكمال الميثود الوحيدة المتبقية):**

```csharp
public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
{
    // نمط: L-YYYYMMDD-#### مع فحص التفرّد عبر IPatientRepository
    var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
    for (int seq = 1; seq < 10_000; seq++)
    {
        var candidate = $"L-{datePart}-{seq:D4}";
        if (await _patientRepository.GetByLabIdAsync(candidate) is null)
            return candidate;
    }
    throw new BusinessRuleViolationException("Unable to generate a unique LabId for today.");
}
```

**معايير القبول:**
- `grep -rIn "NotImplementedException" src/MasrLab.Application` ⇒ **0**.
- كل Handler يستقبل ويمرِّر `CancellationToken`.
- كل Handler يستدعي `_uow.SaveChangesAsync` مرة واحدة على الأكثر.

---

### المرحلة 8 — AutoMapper Profiles

**الوضع الحالي:** [`MappingProfile.cs`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Application/Common/Mappings/MappingProfile.cs) يحوي 9 `CreateMap` مع `ReverseMap` (Patient/PatientVisit/TestResult/Sample/Receipt/Culture/AttendanceLog/Account/WorkSheet) — كافية للـ DTOs الحالية لكنها لا تغطي أي DTO جديد من المرحلة 6.

**التقسيم المقترح (11 Profile):** Patient / Visit / Result / Sample / Receipt / Accounting / Culture / DoctorReferral / Users+Permissions / TestsMasterData / Settings+Printer / Statistics / WorkSheet / Attendance+Audit.

**تحذير خاص بـ Patient:** `Patient.Age` هو ValueObject (`Age(Years, Months, Days)`) لكن `PatientDto` يُسطِّحه لـ 3 حقول:

```csharp
CreateMap<Patient, PatientDto>()
    .ForMember(d => d.AgeYears,  o => o.MapFrom(s => s.Age.Years))
    .ForMember(d => d.AgeMonths, o => o.MapFrom(s => s.Age.Months))
    .ForMember(d => d.AgeDays,   o => o.MapFrom(s => s.Age.Days))
    .ForMember(d => d.Phone,     o => o.MapFrom(s => s.Phone != null ? s.Phone.Value : null));
```

⚠️ **تفادَ `ReverseMap()` مع الكيانات التي تحوي ValueObjects** — قد يُنتج كيانات ذات ValueObjects `null`. استخدم `ForMember` صريحاً في اتجاه واحد فقط، وابنِ الكيان في Handler عبر Factory (`Patient.Register(...)`).

**معايير القبول:**
- كل DTO ↔ Entity له خريطة صريحة أو تصريح أحادي الاتجاه.
- `MappingConfigurationTests.AssertConfigurationIsValid()` ينجح.

---

### المرحلة 9 — إكمال الـ 29 Validator المفقود

**قائمة الأسماء الدقيقة (كلها Queries):**

```
CalculateHighLowStatusQuery
CheckPermissionQuery
FilterAntibioticsQuery
GenerateLabIdQuery
GeneratePatientWorkSheetQuery
GenerateTestLogQuery
GenerateTestWorkSheetQuery
GetAttendanceLogsQuery
GetAuditLogsQuery
GetCaseUserTrackingQuery
GetCasesByPeriodQuery
GetCultureResultQuery
GetDoctorReferralReportQuery
GetDrawerReportQuery
GetGenderStatisticsQuery
GetMonthlyStatisticsQuery
GetOutsourcedSamplesQuery
GetPatientByIdQuery
GetPatientCountByPeriodQuery
GetPatientHistoryQuery
GetPatientVisitHistoryQuery
GetPendingSamplesQuery
GetPriceListForPrintQuery
GetSampleCountByYearQuery
GetSystemSettingsQuery
GetTestDemandRateQuery
GetTestResultForVisitQuery
GetTestWithReferencesQuery
SearchPatientsQuery
```

**قواعد مشتركة (`Common/Validations/CommonRules.cs`):**
- `EgyptianPhone` (يطابق ValueObject: يبدأ بـ `01[0-25]`، طول 11).
- `Amount`: `GreaterThanOrEqualTo(0)`.
- `DateRange`: `PeriodStart <= PeriodEnd`.
- `LabId`: `NotEmpty`, `MaximumLength`, regex متوقَّع.

**نمط:**
```csharp
public class GetPatientByIdQueryValidator : AbstractValidator<GetPatientByIdQuery>
{
    public GetPatientByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class SearchPatientsQueryValidator : AbstractValidator<SearchPatientsQuery>
{
    public SearchPatientsQueryValidator() => RuleFor(x => x.SearchTerm).NotEmpty().MinimumLength(2);
}
```

**معايير القبول:** عدد `*Validator.cs` = عدد `*Command.cs` + `*Query.cs` = **67**. مسح تلقائي عبر `AddValidatorsFromAssembly` يبقى كما هو.

---

### المرحلة 10 — DI + AuditBehavior حقيقي

**التعديلات على `Application/DependencyInjection.cs`:**

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    var assembly = Assembly.GetExecutingAssembly();

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    services.AddAutoMapper(cfg => { }, assembly);
    services.AddValidatorsFromAssembly(assembly);

    // Behaviors — الترتيب مهم: Validation قبل Audit
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

    // Domain Services (10)
    services.AddScoped<IPricingService, PricingService>();
    services.AddScoped<IReceiptCalculationService, ReceiptCalculationService>();
    services.AddScoped<IReferralCommissionService, ReferralCommissionService>();
    services.AddScoped<IPriceListResolverService, PriceListResolverService>();
    services.AddScoped<IResultValidationService, ResultValidationService>();
    services.AddScoped<ISampleTrackingService, SampleTrackingService>();
    services.AddScoped<IAccountingService, AccountingService>();
    services.AddScoped<IOutsourcingService, OutsourcingService>();
    services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
    services.AddScoped<ICultureSensitivityService, CultureSensitivityService>();

    // Helpers
    services.AddScoped<LabIdGenerator>();

    return services;
}
```

**التعديلات الحرجة على `Infrastructure/DependencyInjection.cs`:**
- إضافة `services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();` (يستلزم المرحلة 6a).
- التحقّق أن كل Repository في `Domain/Interfaces` مسجَّل (حالياً 7 مسجَّل + `IRepository<>` — بعد 6a يصبحون 8).

**تعزيز `AuditBehavior` (بديل عن `Debug.WriteLine` الحالي):**

```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogRepository _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public AuditBehavior(IAuditLogRepository audit, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _audit = audit;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next();

        // سجل التدقيق بعد نجاح Handler
        var entry = new AuditLog
        {
            UserId = _currentUser.UserId,
            Action = typeof(TRequest).Name,
            OccurredAt = DateTime.UtcNow
            // Payload / EntityType / EntityId يُعبَّآن حسب Handler
        };
        await _audit.AddAsync(entry, ct);
        await _uow.SaveChangesAsync(ct);
        return response;
    }
}
```

**Composition Root:** في `Presentation/App.xaml.cs` — الترتيب `services.AddInfrastructure(config).AddApplication();`.

**معايير القبول:**
- dry-run: `services.BuildServiceProvider(validateScopes: true)` بلا `Unable to resolve service`.
- كل الـ 10 Domain Services قابلة للحقن.
- `AuditBehavior` يُنتج سجلاً في جدول `AuditLog` عند تنفيذ أي MediatR request.

---

### المرحلة 11 — Unit Tests

**بنية مقترحة تحت `tests/MasrLab.Application.Tests/`:**

```
Features/{FeatureName}/{HandlerName}Tests.cs        (≥ 67 اختبار — واحد لكل Handler)
Services/{ServiceName}Tests.cs                      (≥ 30 اختبار — 3 لكل خدمة من الـ 10)
Common/Behaviors/ValidationBehaviorTests.cs
Common/Behaviors/AuditBehaviorTests.cs
Common/Mappings/MappingConfigurationTests.cs
Common/Helpers/LabIdGeneratorTests.cs
Builders/{Patient|Visit|Receipt|Culture}Builder.cs   (يعزل تعقيد بناء كيانات الاختبار)
```

**نموذج توضيحي — اختبار Handler:**

```csharp
public class RegisterPatientCommandHandlerTests
{
    [Fact]
    public async Task Should_throw_DuplicateLabIdException_When_LabId_exists()
    {
        var repo = new Mock<IPatientRepository>();
        repo.Setup(r => r.GetByLabIdAsync("L-100")).ReturnsAsync(new Patient { LabId = "L-100" });
        var uow = new Mock<IUnitOfWork>();
        var gen = new Mock<LabIdGenerator>(repo.Object);
        var sut = new RegisterPatientCommandHandler(repo.Object, uow.Object, gen.Object);

        Func<Task> act = () => sut.Handle(new RegisterPatientCommand(/*…*/ "L-100" /*…*/), default);

        await act.Should().ThrowAsync<DuplicateLabIdException>();
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

**نموذج توضيحي — اختبار Domain Service:**

```csharp
public class PricingServiceTests
{
    [Fact]
    public void CalculateSubtotal_should_sum_visit_test_prices()
    {
        var visit = PatientVisit.Create(1, 1, "L-1", null, null);
        visit.AddTest(10, 50m, false);
        visit.AddTest(20, 75m, false);

        new PricingService(Mock.Of<IPriceListResolverService>())
            .CalculateSubtotal(visit).Should().Be(125m);
    }
}
```

**معايير القبول:**
- ≥ 1 اختبار لكل Handler (≥ 67).
- ≥ 3 اختبارات لكل Domain Service (≥ 30).
- `MappingConfigurationTests` يستدعي `AssertConfigurationIsValid()`.
- تغطية كود ≥ 80% مقاسة بـ `coverlet.collector`.

---

### المرحلة 12 — Integration Testing

**النطاق:** إنشاء `IntegrationTests/` داخل `MasrLab.Infrastructure.Tests` (`Microsoft.EntityFrameworkCore.InMemory 8.0.*` **جاهز بالفعل**).

**Fixture:**

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    public IServiceProvider Sp { get; private set; } = default!;

    public Task InitializeAsync()
    {
        var services = new ServiceCollection()
            .AddInfrastructure(inMemoryConfig)
            .AddApplication();
        Sp = services.BuildServiceProvider();
        // Run DefaultAdminSeeder + DefaultSettingsSeeder على DbContext الفعلي
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

**السيناريوهات الحرجة:**

1. **تدفق زيارة كاملة**: `RegisterPatient → CreateVisit* → AddTestToVisit* → MarkSampleCollected → EnterTestResult → IssueReceipt → AddPayment → CloseVisit`.
   > `CreateVisit` و `AddTestToVisit` ليسا Commands منفصلين في الكوميت الحالي — الأحداث `PatientVisitCreated` و `VisitTestAdded` موجودة في `DomainEvents.cs` بلا Use Case مقابل. إمّا أن تُضاف Commands جديدة أو تُدمج ضمن `RegisterPatient` — قرار DD-12 مطلوب.
2. **إسناد خارجي**: `MarkTestAsOutsourced → ReceiveOutsourcedResult → SettleOutsourcedAccount`.
3. **زراعة**: `AddNewCulture → AddAntibioticToCulture → EnterCultureResult → GetCultureResult`.
4. **إحصائيات**: بذر بيانات + تشغيل الـ 5 Statistics Queries والتحقق العددي.
5. **تدقيق**: أي عملية أعلاه ⇒ سجل في `AuditLog` قابل للاسترجاع عبر `GetAuditLogsQuery`.
6. **Soft Delete + Restore** عبر `SoftDeleteInterceptor` — سيناريو `Delete → GetById` يجب أن يعود `null` لكن `IgnoreQueryFilters()` يظهره.
7. **`PatientHistoryView` عبر EF**: بعد المرحلة 6a — `GetPatientHistoryQuery` يعيد بيانات صحيحة من الـ View.

**معايير القبول:** كل السيناريوهات تنجح على InMemory Provider؛ Interceptors تعمل تلقائياً؛ صفر `NotImplementedException` في مسار التنفيذ.

---

## 5. خطة الاختبارات (Testing Strategy)

### 5.1 مصفوفة الأنواع

| النوع | النطاق | المشروع | الأداة |
|---|---|---|---|
| **Unit Tests** | كل Handler + كل Domain Service + كل Validator + كل Profile + `LabIdGenerator` | `MasrLab.Application.Tests` | xUnit + Moq + FluentAssertions |
| **Domain Tests** | Entities (ميثودات الحالة: `EnterAllResults`, `Close`, `IssueReceipt`, ...)، ValueObjects (`Age`, `DateRange`, `EgyptianPhone`)، Events، State Machines | `MasrLab.Domain.Tests` (موجود بـ 7 ملفات حقيقية + Placeholder = 8 ملفات) | xUnit |
| **Integration Tests** | Repositories + Interceptors + Seeders + `PatientHistoryView` | `MasrLab.Infrastructure.Tests` | xUnit + EF InMemory (جاهز) |
| **Functional / E2E** | تدفقات كاملة عبر MediatR | `MasrLab.Infrastructure.Tests` (أو مشروع منفصل لاحقاً) | xUnit + DI حقيقي |

### 5.2 الأدوات

- ✅ xUnit 2.5.3 — كل مشاريع الاختبار.
- ✅ coverlet.collector 6.0.0 — كل مشاريع الاختبار.
- ✅ `Microsoft.EntityFrameworkCore.InMemory 8.0.*` — `Infrastructure.Tests` فقط (يجب مشاركته لـ `Application.Tests` عند الحاجة).
- 🆕 Moq 4.20.* — يُضاف في المرحلة 1.
- 🆕 FluentAssertions 6.12.* — يُضاف في المرحلة 1.
- 🔜 Testcontainers.MsSql — لاحقاً لاختبار SQL Server الفعلي (الـ View والـ Migrations لا تعمل تماماً على InMemory).

### 5.3 السيناريوهات الحرجة

1. **`IssueReceipt`**: الرفض إذا كانت الحالة `Closed` — `PatientVisit.IssueReceipt` يفحص الحالة قبل الإصدار.
2. **`ApplyDiscount`**: الإجمالي لا يصبح سالباً — `PricingService.CalculateTotal` يستخدم `Math.Max(0m, …)`.
3. **`Close`**: `PatientVisit.Close(finalTotal)` يرفض إذا كانت الحالة `Closed` مسبقاً.
4. **إضافة اختبار لزيارة مغلقة**: `PatientVisit.AddTest` يرمي `BusinessRuleViolationException` عند `Status == Closed`.
5. **`Lab ID` مكرَّر**: Handler يرمي `DuplicateLabIdException` (بعد تفعيل `LabIdGenerator`).
6. **جمع عينة مكرَّر**: `ISampleTrackingService.IsSampleCollectedAsync` يمنع الحالة المكرَّرة، و `Sample.Collect` يرمي إذا كانت `SampleStatus.Collected` مسبقاً.
7. **نتيجة خارج النطاق**: `IResultValidationService.IsResultInRange` تُنتج `ResultStatus.High/Low` بشكل صحيح.
8. **صلاحيات**: Handler يفشل بـ `InsufficientPermissionException` عند غياب الصلاحية.
9. **عمولة صفرية**: `CalculateCommission(null, null, total)` = 0.
10. **إسناد خارجي كامل**: التدفق ثلاثي الخطوات + `Account.NetProfit` عبر `IAccountingService.RecalculateNetProfitAsync`.
11. **زراعة وحساسية متعددة**: تسجيل حساسيات متعددة + `GetSensitivitySummary` (يمنع تسجيل مزدوج عبر `CultureStatus`).
12. **Soft Delete + Audit Interceptors**: الحذف يُشعر [`SoftDeleteInterceptor`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs) و [`AuditableEntityInterceptor`](https://github.com/El-ogra/MasrLab/blob/09547198490128b2efbfc700e79fafe609b7ac9c/src/MasrLab.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs).
13. **`SampleUncollectedReverted`**: `Sample.RevertCollection()` يمر بالحالة الصحيحة وينشر Event.
14. **`TestResultEdited`**: `TestResult.Edit(newValue, editedBy)` ينشر Event مع القيمة القديمة والجديدة.
15. **`PatientHistoryView` عبر EF**: `IPatientHistoryRepository.GetByPatientAndTestAsync` يعمل بعد المرحلة 6a.
16. **`Receipt.Issue()` idempotency**: الاستدعاء الثاني يرمي `BusinessRuleViolationException` — لا يكرِّر Event.
17. **`Receipt.ApplyDiscount` بعد `Paid`**: يرمي `BusinessRuleViolationException` (لا خصم على إيصال مدفوع).
18. **`OutsourcedSample.CompleteSettlement` قبل `ReceiveResult`**: يرمي `BusinessRuleViolationException`.
19. **`OutsourcedSample.SetPrices` بسعر مريض سالب**: يرمي `BusinessRuleViolationException` — يفترض إغلاق R0 (المرحلة 0) لضمان أن هذا هو المسار الوحيد للتعيين.

---

## 6. المخاطر والتوصيات

### 6.1 المخاطر (بترتيب الشدة)

| # | المخاطرة | الشدة | التخفيف |
|---|---|---|---|
| **R0** | `OutsourcedSample.CostPrice` و `PatientPrice` عقاران عامّان بـ `public set` — أي كود مستهلك يستطيع تخطّي `SetPrices()` وكتابة قيم لا تحترم _invariant_ «سعر المريض ≥ سعر التكلفة». | **حرجة** (شرط مسبق) | **المرحلة 0**: تحويل كلا العقارين إلى `{ get; private set; }` + إضافة اختبار سلبي (`new OutsourcedSample { PatientPrice = -1m }` يفشل في التجميع بعد التحويل، وهو دليل الإصلاح). أثر Schema: صفر. |
| R1 | **`IPatientHistoryRepository` غير مُنفَّذ ولا مسجَّل**: كلاس ملموس غير موجود، وواجهة الـ DI لا تسجّله ⇒ Handler `GetPatientHistory` يرفع `Unable to resolve service` فور محاولة الاستخدام. | 🔴 مرتفع جداً | **مرحلة 6a مستقلة**: إنشاء `PatientHistoryRepository.cs` + `DbSet<PatientHistoryView>` + Migration جديد للـ View + تسجيل DI. |
| R2 | **67 Handler ترمي `NotImplementedException`** — قابلة للنشر إنتاجاً بلا تحذير. | 🔴 مرتفع | Analyzer / `.editorconfig` + اختبار موحَّد يمنع الـ merge إذا وُجد `throw new NotImplementedException` في `Application/Features/`. |
| R3 | **29 Validator مفقود** — كلها Queries. | 🔴 مرتفع | إنشاؤها دفعة واحدة قبل تفعيل أي Handler من الحزمة (المرحلة 9). |
| R4 | **6 Queries تُرجع `object`/`IReadOnlyList<object>`** — Type-hole يعطّل AutoMapper و OpenAPI/Swagger لاحقاً. | 🔴 مرتفع | المرحلة 5: استبدال حصري ⇒ DTO مطابق. |
| R5 | **`GetTestWithReferencesQuery : IRequest<TestResultDto?>` و `GetPriceListForPrintQuery : IRequest<ReceiptDto?>`** — عدم تطابق دلالي بين الاسم والنوع. | 🟠 متوسط | تصحيح النوع في المرحلة 5 ⇒ `TestWithReferencesDto` / `PriceListPrintDto`. |
| R6 | **العقود المتزامنة (`sync`)** في 4 خدمات تحتاج I/O — `Result / PriceList / ReferralCommission / CultureSensitivity`. | 🟠 متوسط | المرحلة 3.2: `Async` overloads + `DD-11`. |
| R7 | **`IAuthenticationService` و `IPrintService` واجهتان فارغتان تماماً** — Handlers `RecordLogin/Logout/Break` و Report/Print Handlers لا يمكنها التقدّم. | 🟠 متوسط | المرحلة 3.1: `DD-09` و `DD-10`. |
| R8 | **`AuditBehavior` مسجَّل فقط في هذه الوثيقة**، ومنطقه الحالي مجرد `Debug.WriteLine` — أي عملية اليوم لا تُدقَّق. | 🟠 متوسط | التسجيل في DI + إعادة صياغة الميثود ليعتمد `IAuditLogRepository + ICurrentUserService` (المرحلة 10). |
| R9 | **Circular Dependency محتملة**: Domain-Services عقودها في Domain، تنفيذها في Application، تستخدم Repository interfaces من Domain — تصميم صحّي لكنه يتطلب انضباطاً صارماً. | 🟠 متوسط | لا يجوز أن يستورد أي كلاس تحت `Application/Services/` أي شيء من `MasrLab.Infrastructure.*`. |
| R10 | **AutoMapper `ReverseMap()` مع ValueObjects (`Age`, `EgyptianPhone`)** — قد يُنتج كيانات ذات ValueObjects `null`. | 🟠 متوسط | تفادي `ReverseMap` مع الكيانات التي تحوي ValueObjects — استخدم `ForMember` صريح وبناء الكيان عبر Factory في Handler. |
| R11 | **Naming collision `IAuthenticationService` (Application, فارغة) مقابل `AuthenticationService` (Infrastructure)** — البناء ينجح فقط لأن الأعضاء غائبان. | 🟡 منخفض/متوسط | حسم: العقد النهائي في Application، والتنفيذ في Infrastructure أو Application حسب DD-09. |
| R12 | **`IUnitOfWork` بسيط بلا `Transactions` صريحة** (`SaveChangesAsync` فقط). | 🟡 منخفض/متوسط | إضافة `BeginTransactionAsync/Commit/Rollback` لاحقاً (خارج نطاق الطبقة الآن). |
| R13 | **Migration واحد فقط (`20260803173749_InitialCreate`) تحت `Persistence/Migrations/`** ⇒ أي تعديل كيان أو إضافة View (كـ `PatientHistoryView`) يستلزم Migration جديد قبل التسليم. | 🟡 منخفض | كل PR يمس Schema يجب أن يحمل Migration في نفس commit. |
| R14 | **Test Data Builders غير موجودة** — كتابة `Patient/Visit` معقدة في كل اختبار = تكرار. | 🟢 منخفض | `PatientBuilder / VisitBuilder / ReceiptBuilder / CultureBuilder` تحت `tests/.../Builders/`. |
| R15 | **الأحداث `PatientVisitCreated / VisitTestAdded/Removed / ReceiptPaymentAdded / DiscountApplied / VisitClosed / OutsourcedResultReceived / SampleUncollectedReverted / TestResultEdited`** بلا Use Cases Commands مقابلة. | 🟡 متوسط | معظمها يُنفَّذ ضمنياً داخل ميثودات الكيانات ولا يحتاج بالضرورة Command منفصلاً — قرار DD-12. إمّا إضافة Commands جديدة تحت `Features/Visits/*` و `Features/Receipts/*`، أو **دمج** التنقّلات ضمن Command Handlers موجودة. |
| R16 | **`Sample.CollectedByUserId` و `CollectedAt` بـ `public set`** — نمط مشابه لـ R0 لكن أقل حدة (يمكن كتابتهما دون المرور عبر `Sample.Collect(userId)`). | 🟢 منخفض | تحويلهما إلى `{ get; private set; }` عند أول Migration ذي صلة بالكيان — ليس شرطاً مسبقاً لطبقة Application. |

### 6.2 اعتماديات على طبقات أخرى

- **قاعدة البيانات:** Migration واحد فقط (`20260803173749_InitialCreate` تحت `src/MasrLab.Infrastructure/Persistence/Migrations/`) — أي تعديل كيان أو إضافة View يستلزم Migration جديد.
- **`PatientHistoryView.sql`:** موجود بلا تنفيذ EF (لا Migration، لا `DbSet`، لا Repository) — يجب سدّ الفجوة في مرحلة 6a قبل تشغيل `GetPatientHistoryQuery` أو `IMedicalHistoryService`.
- **Presentation (WPF/ViewModels):** 31 ViewModel + 38 `*.xaml.cs` — كلها ستستهلك MediatR من Composition Root في `App.xaml.cs`.
- **Interceptors:** `AuditableEntityInterceptor` و `SoftDeleteInterceptor` يعملان تلقائياً على `IAuditableEntity` و `ISoftDeletable` — لا داعي لكود إضافي في Handlers.

### 6.3 توصيات تنظيمية

1. **PR مقسَّم بحسب المرحلة** — لا تدمج مرحلتين في PR واحد.
2. **Analyzer**: قاعدة `.editorconfig` تمنع `NotImplementedException` في `src/MasrLab.Application/Features/`.
3. **CI**: `dotnet test --collect:"XPlat Code Coverage"` مع حد أدنى 80%.
4. **Decision Records جديدة (مقترحة):**
   - `DD-09-IAuthenticationService-Contract.md`
   - `DD-10-IPrintService-Contract.md`
   - `DD-11-DomainServices-Async-Overloads.md`
   - `DD-12-Events-Without-UseCases.md`
5. **الوثائق:** الاستمرار بأسلوب `Docs/Gaps-after-phase10.md` — يمكن استكماله بـ `Gaps-after-application-layer.md` بعد كل ميلستون.
6. **CI Windows Runner:** عند توفّره، وسِّع `dotnet build -warnaserror` ليشمل `MasrLab.Presentation` (net8.0-windows).

---

## 7. الملخص التنفيذي

### 7.1 جدول المراحل بالأيام

| # | المرحلة | أيام العمل |
|---|---|---|
| **0** | **إصلاح R0 (شرط مسبق إلزامي)** | **0.1** |
| 1 | ضبط الحزم | 0.5 |
| 2 | هيكل المجلدات | 0.5 |
| 3 | استكمال العقود (Auth/Print) + قرار Async | 1 |
| 4 | تنفيذ 10 Domain Services | 4 |
| 5 | ضبط أنواع الإرجاع (6 Queries) | 0.5 |
| 6 | إضافة ~22 DTO | 2 |
| 6a | `PatientHistoryRepository` + View + Migration | 1 |
| 7 | كتابة 67 Handler + `LabIdGenerator` | 12 |
| 8 | AutoMapper Profiles (11) | 2 |
| 9 | 29 Validator + قواعد مشتركة | 3 |
| 10 | DI + AuditBehavior حقيقي | 1.5 |
| 11 | Unit Tests | 6 |
| 12 | Integration Tests | 3 |
| **الإجمالي** | — | **~37 يوم عمل** |

### 7.2 التقدير الإجمالي

- **مهندس واحد متفرغ:** ~37 يوم عمل.
- **مهندسان بموازاة:** ~22 يوم عمل (المراحل 4، 7، 9، 11 قابلة للتوازي بين مطوّرَين).
- **المسار الحرج (Critical Path):** 0 → 3 → 4 → 6a → 7 → 11 → 12 (يسير تسلسلياً).
- **Milestone-1 (Proof of Life للطبقة):** بعد 0، 1، 2، 3، 4، 6a، 10 (جزئياً) — يجب أن يعمل تدفق `RegisterPatient → EnterTestResult → GetPatientHistory` من طرف إلى طرف.
- **Milestone-2:** بعد 5، 6، 8، 9 — الطبقة صالحة عقدياً ومغطاة بالتحقق.
- **Milestone-3:** بعد 11، 12 — استعداد الطبقة للإنتاج.

### 7.3 قائمة تحقق نهائية

- [ ] **المرحلة 0** — `OutsourcedSample.CostPrice` و `PatientPrice` صارا `{ get; private set; }`؛ `dotnet build -warnaserror` و `dotnet test` نجحا بعد التعديل (189/189).
- [ ] كل الـ 10 Domain Services منفَّذة داخل `Application/Services/` ومسجَّلة في DI.
- [ ] كل الـ 67 Handler منفَّذ (صفر `NotImplementedException` في `Application/Features/`).
- [ ] `LabIdGenerator.GenerateAsync` منفَّذ (صفر `NotImplementedException` في `Application/Common/Helpers/`).
- [ ] كل الـ 67 Command/Query لديه Validator (الـ 29 المفقود مذكور في المرحلة 9).
- [ ] كل الـ ~22 DTO المذكور في §2.3 مضاف.
- [ ] لا Query يُرجع `object` / `IReadOnlyList<object>`.
- [ ] `GetTestWithReferencesQuery` و `GetPriceListForPrintQuery` يستخدمان DTO دلالياً صحيحاً.
- [ ] `IAuthenticationService` و `IPrintService` لهما عقد كامل (DD-09 / DD-10).
- [ ] Domain Services المتزامنة لها overloads `Async` (DD-11).
- [ ] `IPatientHistoryRepository` منفَّذ + مسجَّل + `DbSet<PatientHistoryView>` + Migration للـ View.
- [ ] `AuditBehavior` مسجَّل في DI ويستخدم `IAuditLogRepository`.
- [ ] `MappingConfigurationTests.AssertConfigurationIsValid()` ينجح.
- [ ] تغطية اختبارات ≥ 80% (coverlet).
- [ ] السيناريوهات الحرجة (§5.3) مغطاة باختبار واحد على الأقل.
- [ ] `dotnet build -warnaserror` و `dotnet test` ناجحان على الحل كاملاً.
- [ ] `Docs/DecisionRecords/` يحوي DD-08 (موجود) + DD-09 + DD-10 + DD-11 + DD-12.