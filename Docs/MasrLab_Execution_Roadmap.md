# MasrLab_Execution_Roadmap.md
# خارطة الطريق التنفيذية لبناء هيكل مشروع MasrLab

> **مستند تنفيذ مرحلي (Execution Roadmap)** — موجّه إلى وكيل برمجي محلي (Cursor / Claude Code / GitHub Copilot Agent / Roo Code).
> يعتمد حصرياً على مرجعَين هما المصدر الوحيد للحقيقة:
> - `implementation_plan.md` (خطة التنفيذ والهيكل الفني)
> - `MasrLab_Specifications_and_Audit.md` (قاعدة المعرفة الوظيفية)

> **البيئة التقنية المستهدفة:** WPF / .NET 8 / SQL Server / MVVM / LAN / RTL / EGP / فرع واحد.
> **المعمارية:** Clean Architecture + MVVM.
> **نطاق كل مراحل هذه الوثيقة:** بناء **الهيكل فقط** (Skeleton) — لا Business Logic، لا Validation، لا Services، لا Algorithms.

---

## جدول محتويات المراحل

| # | اسم المرحلة | الطبقة الرئيسية |
|:---:|:---|:---|
| 0 | تحضير بيئة العمل وإنشاء الحل الفارغ | Solution |
| 1 | إنشاء مشروع Domain وعناصره الأساسية (Common + Enums + Exceptions) | Domain |
| 2 | إنشاء كيانات النطاق Core (Patient / Visit / Test / Result / …) | Domain |
| 3 | إنشاء كيانات النطاق Culture | Domain |
| 4 | إنشاء كيانات النطاق Financial | Domain |
| 5 | إنشاء كيانات النطاق Administrative | Domain |
| 6 | إنشاء كيانات النطاق Settings | Domain |
| 7 | إنشاء واجهات المستودعات (Repository Interfaces) في Domain | Domain |
| 8 | إنشاء مشروع Application وطبقة Common (DTOs / Interfaces / Behaviors) | Application |
| 9 | هياكل Features الأساسية (Modules 1–5) | Application |
| 10 | هياكل Features الأساسية (Modules 6–9) | Application |
| 11 | هياكل Features الإدارية (Modules 10–14) — Master Data | Application |
| 12 | هياكل Features الإدارية (Modules 15–16) — Users / Attendance / Audit | Application |
| 13 | هياكل Features المالية (Modules 17–19) — Drawers & Accounting | Application |
| 14 | هياكل Features الإحصاء والإعدادات (Modules 20–21) | Application |
| 15 | إنشاء مشروع Infrastructure وطبقة Persistence (DbContext + Interceptors + Migrations skeleton) | Infrastructure |
| 16 | ملفات Fluent API Configurations (Core + Culture) | Infrastructure |
| 17 | ملفات Fluent API Configurations (Financial + Administrative + Settings) | Infrastructure |
| 18 | Repositories وUnitOfWork وViews وSeeders | Infrastructure |
| 19 | خدمات Infrastructure (Auth / Print / Barcode / Backup / …) | Infrastructure |
| 20 | إنشاء مشروع Presentation (WPF) وطبقة Resources وNavigation | Presentation |
| 21 | هياكل ViewModels وViews (Core Modules 1–9) | Presentation |
| 22 | هياكل ViewModels وViews (Master Data 10–14) | Presentation |
| 23 | هياكل ViewModels وViews (Administrative + Financial + Statistics + Settings) | Presentation |
| 24 | هياكل Printing (تقارير الطباعة العشرين) | Presentation |
| 25 | مشاريع الاختبارات الثلاثة (Domain / Application / Infrastructure Tests) | Tests |
| 26 | التحقق النهائي وضبط تبعيات الحل | Solution-Wide |

> **قواعد عامة تسري على كل المراحل:**
> - **الهيكل فقط**: كل الكلاسات والواجهات تنشأ بدون Body وبدون Logic.
> - **لا تعديل خارج نطاق المرحلة**، ولا حذف لأي ملف قائم، ولا استنتاج أعضاء غير موجودة بالمراجع.
> - **قواعد التسمية:** PascalCase للـTypes/Properties، Interfaces بادئتها `I`، ملفات XAML مطابقة لاسم الـViewModel (مثال: `RegisterPatientView.xaml` ↔ `RegisterPatientViewModel.cs`).
> - **البناء إلزامي في نهاية كل مرحلة** بدون Compilation Errors.

---

# المرحلة 0 — تحضير بيئة العمل وإنشاء الحل الفارغ

## الهدف
إنشاء ملف الحل (Solution) وهيكل المجلدات الرئيسي (`src/`, `tests/`) وإعداد ملفات البناء العامة، بدون أي مشروع فعلي بعد.

## المتطلبات السابقة
لا توجد متطلبات سابقة.

## النطاق
### سيتم:
- إنشاء ملف `MasrLab.sln`.
- إنشاء مجلدَي `src/` و`tests/`.
- إنشاء `global.json` لتثبيت .NET 8 SDK.
- إنشاء `Directory.Build.props` (خصائص LangVersion / Nullable / TreatWarningsAsErrors مطابقة للخطة).
- إنشاء `.gitignore` قياسي لمشاريع .NET/WPF.

### لن يتم:
- إنشاء أي مشروع (سيتم في المراحل التالية).
- إضافة أي حزمة NuGet.
- كتابة أي كود C#.

## الأقسام المرجعية
- `implementation_plan.md` → «هيكل الحل (Solution Structure)».
- `MasrLab_Specifications_and_Audit.md` → القسم 7 (البيئة التقنية).

## تعليمات التنفيذ للوكيل المحلي
> أنشئ الجذر التالي:
> ```
> MasrLab/
> ├── MasrLab.sln                    (Solution فارغ)
> ├── global.json                    (SDK: 8.0.x)
> ├── Directory.Build.props          (Nullable=enable, LangVersion=latest, TreatWarningsAsErrors=false)
> ├── .gitignore                     (قالب Visual Studio / .NET)
> ├── src/
> └── tests/
> ```
> لا تُنشئ أي مشروع (`.csproj`) في هذه المرحلة.

## المحظورات
- لا Business Logic، لا Validation، لا Services، لا Algorithms.
- لا تنشئ ملفات إضافية غير المذكورة.
- لا تُعدّل ملفات خارج نطاق المرحلة.
- لا تحذف أي ملف موجود.
- لا تحسّن التصميم من تلقاء نفسك.

## ناتج المرحلة المتوقع
حل فارغ (`MasrLab.sln`) بمجلدَي `src/` و`tests/`، وملفات إعداد البناء الأساسية.

## معايير القبول
- [ ] `MasrLab.sln` موجود.
- [ ] `global.json` يُحدّد إصدار .NET 8.
- [ ] `Directory.Build.props` موجود.
- [ ] `.gitignore` موجود.
- [ ] المجلدَان `src/` و`tests/` موجودان (يمكن استخدام `.gitkeep`).
- [ ] `dotnet build` لا يُنتج أي خطأ (سيقول "لا مشاريع" وهذا مقبول).

## ملاحظات للمرحلة التالية
سيبنى فوقها مشروع `MasrLab.Domain` كأول مشروع في الحل.

---

# المرحلة 1 — مشروع Domain: العناصر المشتركة (Common + Enums + Exceptions)

## الهدف
إنشاء مشروع طبقة النطاق `MasrLab.Domain` وتعبئته بالعناصر المشتركة (BaseEntity، الواجهات القاعدية، جميع الـEnums، وExceptions) قبل أي كيان فعلي.

## المتطلبات السابقة
اكتمال المرحلة 0 (وجود `MasrLab.sln` والمجلدات الأساسية).

## النطاق
### سيتم:
- إنشاء مشروع `src/MasrLab.Domain/MasrLab.Domain.csproj` (Class Library — net8.0).
- إنشاء المجلدات `Common/`, `Common/Enums/`, `Entities/`, `Interfaces/`, `Exceptions/`.
- إنشاء الملفات التالية داخل `Common/`:
  - `BaseEntity.cs` بالخصائص: `Id`, `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `IsDeleted`.
  - `IAuditableEntity.cs`.
  - `ISoftDeletable.cs`.
- إنشاء جميع الـEnums داخل `Common/Enums/`:
  - `Gender.cs`
  - `AccountType.cs`
  - `VisitStatus.cs`
  - `SampleStatus.cs`
  - `ResultStatus.cs`
  - `SensitivityLevel.cs`
  - `ReferralEntityType.cs`
  - `TransactionType.cs`
  - `AuditActionType.cs`
  - `AuditEntityType.cs`
  - `PrinterPurposeType.cs`
  - `PaperSize.cs`
  - `AgeUnit.cs`
  - `WorkSheetType.cs`
- إنشاء ملفات `Exceptions/`:
  - `EntityNotFoundException.cs`
  - `BusinessRuleViolationException.cs`
  - `InsufficientPermissionException.cs`
  - `DuplicateLabIdException.cs`
- إضافة المشروع إلى `MasrLab.sln`.

### لن يتم:
- إنشاء أي Entity فعلي (سيتم في المراحل 2–6).
- إنشاء أي Repository Interface (سيتم في المرحلة 7).
- كتابة Body لأي Exception (Skeleton constructors فقط).

## الأقسام المرجعية
- `implementation_plan.md` → «1. MasrLab.Domain — طبقة النطاق».
- `MasrLab_Specifications_and_Audit.md` → القسم 7.3 (سياسة الحذف وأعمدة التدقيق)، القسم 2 (لأسماء الـEnums).

## تعليمات التنفيذ للوكيل المحلي
> أنشئ Class Library باسم `MasrLab.Domain` تحت `src/` تستهدف `net8.0`، Nullable مفعّل، بدون أي حزمة NuGet.
> ضع الملفات المذكورة أعلاه في مجلداتها.
> `BaseEntity` كلاس مجرّد (abstract) بخصائص أوتوماتيكية.
> كل Enum يحتوي فقط على القيم المذكورة في المراجع (لا تُضِف قيماً غير موجودة).
> كل Exception يرث من `Exception` مع Constructors قياسية فقط.

## المحظورات
- لا Business Logic، لا Validation، لا Services، لا Algorithms.
- لا تُدخل قيماً في الـEnums غير المذكورة في المراجع.
- لا تنشئ Entities أو Repository Interfaces هنا.
- لا تحسّن التصميم.
- لا تحذف أي ملف قائم.

## ناتج المرحلة المتوقع
مشروع `MasrLab.Domain` قابل للبناء، يحوي `BaseEntity`، الواجهتين، جميع الـEnums، وExceptions.

## معايير القبول
- [ ] `MasrLab.Domain.csproj` موجود ومرتبط بالحل.
- [ ] `BaseEntity.cs` يحوي الخصائص الخمس + `Id` بالضبط.
- [ ] عدد ملفات Enums = 14 ملفاً.
- [ ] عدد ملفات Exceptions = 4 ملفات.
- [ ] `dotnet build` ينجح بدون تحذيرات جوهرية.

## ملاحظات للمرحلة التالية
ستُبنى فوقها كيانات Core (Patient / Visit / Test / Result / …) في المرحلة 2.

---

# المرحلة 2 — Domain/Entities/Core

## الهدف
إنشاء كيانات القسم الجوهري (Core) داخل مشروع Domain: 11 كياناً تمثّل نواة حركة المرضى والتحاليل والنتائج والعينات.

## المتطلبات السابقة
اكتمال المرحلة 1 (Common + Enums جاهزة).

## النطاق
### سيتم إنشاء الملفات داخل `MasrLab.Domain/Entities/Core/`:
1. `Patient.cs`
2. `PatientVisit.cs`
3. `VisitTest.cs`               ← كيان وسيط M:N (قرار الفجوة 3)
4. `Test.cs`
5. `TestResult.cs`
6. `ReferenceValue.cs`
7. `TestGroup.cs`
8. `TestGroupItem.cs`           ← M:N Junction بين TestGroup و Test
9. `Comment.cs`
10. `Sample.cs`
11. `SampleCollection.cs`

كل كيان:
- يرث من `BaseEntity` (ما لم يُذكر خلاف ذلك في مرجع Master Data).
- الحقول مطابقة **بالضبط** لأسمائها وأنواعها المذكورة في المرجعين (بما في ذلك الحقول الملاحية Navigation Properties عندما تُذكر).
- لا Methods، لا Validation، لا Constructors مخصّصة.

### لن يتم:
- إنشاء كيانات Culture / Financial / Administrative / Settings (مراحل لاحقة).
- إنشاء Fluent API Configurations (Infrastructure).
- إنشاء Repository Interfaces (المرحلة 7).

## الأقسام المرجعية
- `implementation_plan.md` → «Entities/Core».
- `MasrLab_Specifications_and_Audit.md` → القسم 2.1 (Entities 1–10)، القرار 3 (VisitTest)، القرار 5 (TestResult).

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 11 ملف `.cs` بأسماء أعلاه.
> لكل ملف صرّح Class عامّ يرث من `BaseEntity` (باستثناء ما تنص المراجع صراحة على استثنائه، مثل جداول Master Data إذا وردت هكذا في `implementation_plan.md`).
> ضع الخصائص الملاحية على شكل `virtual` عند الحاجة لعلاقة EF Core Lazy Loading.
> لا تُنشئ أي علاقات M:N بدون كيان وسيط.

## المحظورات
- لا Logic، لا Validation، لا Services.
- لا تُضِف حقولاً غير مذكورة بالمراجع.
- لا تُنشئ Configurations هنا.
- لا تُعدّل أي ملف Enum أو BaseEntity.

## ناتج المرحلة المتوقع
11 كياناً في `Entities/Core/`، ويُبنى المشروع دون أخطاء.

## معايير القبول
- [ ] وجود 11 ملفاً في `Entities/Core/`.
- [ ] جميع الكيانات ترث من `BaseEntity` (باستثناء ما استُثني بالمرجع).
- [ ] الحقول مطابقة للمرجع (أسماء + أنواع).
- [ ] `VisitTest` موجود ككيان وسيط.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
ستُضاف كيانات Culture (Culture / Organism / Antibiotic / Sensitivity) في المرحلة 3.

---

# المرحلة 3 — Domain/Entities/Culture

## الهدف
إضافة كيانات المزارع الميكروبيولوجية والحساسية.

## المتطلبات السابقة
اكتمال المرحلة 2.

## النطاق
### سيتم:
داخل `MasrLab.Domain/Entities/Culture/`:
1. `Culture.cs`
2. `Organism.cs`
3. `Antibiotic.cs`
4. `Sensitivity.cs`  ← M:N بين Culture و Antibiotic + مستوى الحساسية

### لن يتم:
- إنشاء Configurations أو Repositories.
- إنشاء واجهات مزرعة (لاحقاً في المرحلة 7).

## الأقسام المرجعية
- `implementation_plan.md` → «Entities/Culture».
- `MasrLab_Specifications_and_Audit.md` → Module 3 والكيانات 11–14.

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 4 ملفات كيانات مع خصائصها كما في المرجع.
> `Sensitivity` تستخدم `SensitivityLevel` Enum المُنشأ في المرحلة 1.

## المحظورات
- لا Logic، لا Validation.
- لا تُضِف حقولاً غير مذكورة.
- لا تنشئ Configurations.

## ناتج المرحلة المتوقع
4 كيانات في `Entities/Culture/`، والمشروع يُبنى بنجاح.

## معايير القبول
- [ ] 4 ملفات موجودة.
- [ ] `Sensitivity` تحمل `CultureId` و`AntibioticId` و`SensitivityLevel`.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
كيانات Financial في المرحلة 4.

---

# المرحلة 4 — Domain/Entities/Financial

## الهدف
إضافة كيانات المالية (الإيصالات، الأدراج، المعاملات النقدية، والعينات المُرسَلة خارجاً).

## المتطلبات السابقة
اكتمال المرحلة 3.

## النطاق
### سيتم:
داخل `MasrLab.Domain/Entities/Financial/`:
1. `Receipt.cs`  ← العملة ثابتة `EGP` (القرار 4)
2. `Account.cs`
3. `CashTransaction.cs`
4. `OutsourcedSample.cs`

### لن يتم:
- إنشاء منطق حساب الجرد أو الأرباح.
- إنشاء واجهات مالية (المرحلة 7).

## الأقسام المرجعية
- `implementation_plan.md` → «Entities/Financial».
- `MasrLab_Specifications_and_Audit.md` → الكيانات 15–18، القرار 4.

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 4 كيانات مع كل حقولها المذكورة.
> `Receipt.Currency` يبقى Property بقيمة افتراضية `"EGP"` إن نصّ المرجع، وإلا مجرّد `string` بدون تهيئة (بدون Logic).

## المحظورات
- لا Business Logic، لا Services.
- لا تُنشِئ منطق تسوية العينات المرسلة.

## ناتج المرحلة المتوقع
4 كيانات مالية داخل المجلد المخصّص، والمشروع يُبنى بنجاح.

## معايير القبول
- [ ] 4 ملفات موجودة.
- [ ] الحقول مطابقة للمرجع.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
كيانات Administrative في المرحلة 5.

---

# المرحلة 5 — Domain/Entities/Administrative

## الهدف
إضافة كيانات المستخدمين والصلاحيات والحضور والتدقيق والأطباء وجهات الإحالة.

## المتطلبات السابقة
اكتمال المرحلة 4.

## النطاق
### سيتم:
داخل `MasrLab.Domain/Entities/Administrative/`:
1. `User.cs`
2. `Permission.cs`
3. `AttendanceLog.cs`
4. `AuditLog.cs`
5. `Doctor.cs`
6. `ReferralEntity.cs`

### لن يتم:
- إنشاء منطق التحقق من كلمة المرور.
- إنشاء منطق التدقيق التلقائي (سيأتي في Interceptors).

## الأقسام المرجعية
- `implementation_plan.md` → «Entities/Administrative».
- `MasrLab_Specifications_and_Audit.md` → الكيانات 19–24، Module 15–16، القرار 2.

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 6 كيانات إدارية بالخصائص الواردة (بما فيها `IsAdmin`, `IsActive`, `ScreenId/OperationId`, `Allowed`, `LoginTime`, `LogoutTime`, `ActionType`, `EntityType`, …).

## المحظورات
- لا Logic، لا Validation.

## ناتج المرحلة المتوقع
6 كيانات إدارية داخل المجلد، والمشروع يُبنى.

## معايير القبول
- [ ] 6 ملفات موجودة.
- [ ] `User.IsAdmin` من نوع `bool`.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
كيانات Settings في المرحلة 6.

---

# المرحلة 6 — Domain/Entities/Settings

## الهدف
إضافة كيانات الإعدادات وقوائم الأسعار وقوالب التقارير والطابعات وأوراق العمل.

## المتطلبات السابقة
اكتمال المرحلة 5.

## النطاق
### سيتم:
داخل `MasrLab.Domain/Entities/Settings/`:
1. `PriceList.cs`
2. `PriceListItem.cs`
3. `SystemSetting.cs`  ← Key/Value
4. `Printer.cs`
5. `ReportTemplate.cs`
6. `WorkSheet.cs`

### لن يتم:
- إنشاء منطق قراءة/كتابة الإعدادات.
- إنشاء ملفات Fluent API.

## الأقسام المرجعية
- `implementation_plan.md` → «Entities/Settings».
- `MasrLab_Specifications_and_Audit.md` → الكيانات 25–30، Module 21.

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 6 ملفات كيانات مع الحقول الواردة.
> `SystemSetting` يحمل `SettingKey` و`SettingValue`.

## المحظورات
- لا Logic، لا Services.

## ناتج المرحلة المتوقع
6 كيانات إعدادات، وطبقة Domain اكتملت من حيث الكيانات (30 كياناً).

## معايير القبول
- [ ] المجموع الكلي للكيانات في Domain = 30 (تحقّق من العدد).
- [ ] كل الكيانات ترث `BaseEntity` (أو تنصّ عليها المراجع).
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
واجهات المستودعات (Repository Interfaces) في المرحلة 7.

---

# المرحلة 7 — Domain/Interfaces (Repository Interfaces)

## الهدف
إنشاء عقود المستودعات والوحدة العمل داخل طبقة النطاق.

## المتطلبات السابقة
اكتمال المرحلة 6 (كل الكيانات جاهزة).

## النطاق
### سيتم:
داخل `MasrLab.Domain/Interfaces/`:
1. `IRepository.cs`  (generic contract)
2. `IUnitOfWork.cs`
3. `IPatientRepository.cs`
4. `IVisitRepository.cs`
5. `ITestResultRepository.cs`
6. `ICultureRepository.cs`
7. `IAccountingRepository.cs`
8. `IStatisticsRepository.cs`
9. `IAuditLogRepository.cs`

كل واجهة **بدون** تفاصيل تنفيذ ولا Body — فقط توقيعات (Signatures) بالحد الأدنى المذكور بالمراجع؛ إن لم يُذكر عضو محدد فتُترك الواجهة **فارغة** كما هي.

### لن يتم:
- إنشاء أي Implementation (سيتم في Infrastructure).
- إنشاء واجهات لخدمات (Authentication / Barcode / Print / Backup) — تلك تخصّ Application/Infrastructure.

## الأقسام المرجعية
- `implementation_plan.md` → «Interfaces/».

## تعليمات التنفيذ للوكيل المحلي
> صرّح 9 واجهات ضمن `namespace MasrLab.Domain.Interfaces`.
> إذا لم يذكر المرجع طرقاً بعينها لواجهة، اتركها كواجهة تعليمية Marker Interface — لا تخترع طرقاً.

## المحظورات
- لا تُضِف طرقاً غير مذكورة.
- لا تنفّذ أي واجهة هنا.
- لا Business Logic.

## ناتج المرحلة المتوقع
9 واجهات مستودعات في Domain، والمشروع يُبنى.

## معايير القبول
- [ ] 9 ملفات موجودة.
- [ ] كل الواجهات في namespace صحيح.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
البدء في طبقة Application: `Common` أولاً (المرحلة 8) ثم `Features` تتابعياً (9 → 14).

---

# المرحلة 8 — مشروع Application: طبقة Common

## الهدف
إنشاء مشروع `MasrLab.Application` مع مجلد `Common/` (Interfaces / DTOs / Mappings / Behaviors) وربطه بـ Domain، بدون أي Feature بعد.

## المتطلبات السابقة
اكتمال طبقة Domain (المراحل 1–7).

## النطاق
### سيتم:
- إنشاء `src/MasrLab.Application/MasrLab.Application.csproj` (Class Library — net8.0).
- إضافة **مرجع مشروع** إلى `MasrLab.Domain`.
- تثبيت حزم NuGet المذكورة في `implementation_plan.md` (كمرجع، دون كتابة أي Handler):
  - `MediatR` (الإصدار 12.0+ يحتوي على `AddMediatR()` extension مدمج)
  - `AutoMapper` (الإصدار 12.0+ يحتوي على `AddAutoMapper()` extension مدمج)
  - `FluentValidation`
  - `FluentValidation.DependencyInjectionExtensions`
- إنشاء المجلدات:
  - `Common/Interfaces/`
  - `Common/DTOs/`
  - `Common/Mappings/`
  - `Common/Behaviors/`
- إنشاء ملفات الهياكل داخل `Common/` (فقط ما تنصّ عليه الخطة صراحة، ودون Body):
  - `Common/Mappings/MappingProfile.cs`
  - `Common/Behaviors/ValidationBehavior.cs`
  - (إن ذكر المرجع DTOs بأسماء محددة أنشئها فارغة، مثل `PatientDto.cs`، وإلا اترك المجلد فارغاً بـ`.gitkeep`).
- إنشاء `DependencyInjection.cs` بمِثل واجهة `AddApplication(IServiceCollection)` بدون تنفيذ (Method stub).

### لن يتم:
- كتابة أي Feature (سيتم في المراحل 9–14).
- كتابة أي Validator ملموس.
- كتابة Body للـ MappingProfile.

## الأقسام المرجعية
- `implementation_plan.md` → «MasrLab.Application → Common/».

## تعليمات التنفيذ للوكيل المحلي
> أنشئ المشروع، أضف مرجع Domain، ثبّت الحزم المذكورة (4 حزم فقط — لا حاجة لحزم DI Extensions المهجورة)، ثم أنشئ ملفات Common كما في القائمة.
> استخدم `namespace MasrLab.Application.Common.*` وفق المجلد.

## المحظورات
- لا Logic، لا Validation ملموسة.
- لا Handlers.
- لا Commands/Queries هنا.

## ناتج المرحلة المتوقع
مشروع `MasrLab.Application` قابل للبناء ومربوط بـ Domain.

## معايير القبول
- [ ] `MasrLab.Application.csproj` موجود ومربوط بالحل.
- [ ] مرجع Domain مضاف.
- [ ] الحزم مثبّتة.
- [ ] `Common/Mappings/MappingProfile.cs` موجود.
- [ ] `DependencyInjection.cs` موجود.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
تبدأ إضافة `Features/` بموديولات 1–5 في المرحلة 9.

---

# المرحلة 9 — Application/Features (Modules 1–5)

## الهدف
إنشاء **هياكل** Commands/Queries/Handlers/Validators/DTOs للموديولات:
1. PatientManagement — 2. ResultsEntry — 3. Cultures — 4. PatientSearch — 5. CasesFollowUp.

## المتطلبات السابقة
اكتمال المرحلة 8.

## النطاق
### سيتم:
داخل `MasrLab.Application/Features/` أنشئ المجلدات وملفات الهيكل التالية (كلاسات مصرّحة بدون Body):

**PatientManagement/**
- `Commands/RegisterPatient/RegisterPatientCommand.cs`
- `Commands/RegisterPatient/RegisterPatientCommandHandler.cs`
- `Commands/RegisterPatient/RegisterPatientCommandValidator.cs`
- `Commands/UpdatePatientAccount/…` (3 ملفات بنفس النمط)
- `Commands/DeliverResults/…`
- `Commands/UpdatePatientData/…`
- `Queries/GetPatientById/GetPatientByIdQuery.cs` + Handler
- `Queries/GenerateLabId/GenerateLabIdQuery.cs` + Handler

**ResultsEntry/**
- `Commands/EnterTestResult/…`
- `Commands/CreateCombinedReport/…`
- `Commands/CreateBlankReport/…`
- `Queries/GetTestResultForVisit/…`
- `Queries/CalculateHighLowStatus/…`

**Cultures/**
- `Commands/EnterCultureResult/…`
- `Commands/AddNewCulture/…`
- `Commands/AddAntibioticToCulture/…`
- `Queries/GetCultureResult/…`
- `Queries/FilterAntibiotics/…`

**PatientSearch/**
- `Queries/SearchPatients/…`
- `Queries/GetPatientVisitHistory/…`

**CasesFollowUp/**
- `Queries/GetCasesByPeriod/…`
- `Queries/GetCaseUserTracking/…`

لكل Command/Query:
- Record فارغ يطبّق `IRequest<Unit>` أو `IRequest<T>` (بدون خصائص إن لم يذكر المرجع).
- Handler يطبّق `IRequestHandler<TRequest, TResponse>` مع Method `Handle` **رميّة** (throw NotImplementedException).
- Validator يطبّق `AbstractValidator<TRequest>` مع Constructor فارغ.

### لن يتم:
- كتابة قواعد Validation ملموسة.
- كتابة أي Body لـ Handle.
- إضافة DTOs غير مذكورة صراحة (يمكن ترك مجلد `DTOs/` فارغاً حتى يذكرها المرجع).

## الأقسام المرجعية
- `implementation_plan.md` → «Features/PatientManagement, ResultsEntry, Cultures, PatientSearch, CasesFollowUp».
- `MasrLab_Specifications_and_Audit.md` → Modules 1–5.

## تعليمات التنفيذ للوكيل المحلي
> ولّد مجموعات ملفات (Command + Handler + Validator) في مجلد فرعي لكل عملية.
> استعمل namespace `MasrLab.Application.Features.<Module>.<Commands|Queries>.<Operation>`.

## المحظورات
- لا Logic في Handlers.
- لا Rules في Validators.
- لا تضِف Operations غير مذكورة.

## ناتج المرحلة المتوقع
هياكل موديولات 1–5 داخل Features، والمشروع يُبنى.

## معايير القبول
- [ ] وجود المجلدات الخمسة.
- [ ] كل عملية تحوي 3 ملفات (أو 2 للـQueries + Handler) على الأقل.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
موديولات 6–9 في المرحلة 10.

---

# المرحلة 10 — Application/Features (Modules 6–9)

## الهدف
هياكل موديولات: PatientHistory، WorkSheets، SampleCollection، OutsourcedSamples.

## المتطلبات السابقة
اكتمال المرحلة 9.

## النطاق
### سيتم:
- **PatientHistory/**
  - `Queries/GetPatientHistory/…`
- **WorkSheets/**
  - `Queries/GeneratePatientWorkSheet/…`
  - `Queries/GenerateTestWorkSheet/…`
  - `Queries/GenerateTestLog/…`
- **SampleCollection/**
  - `Commands/MarkSampleCollected/…`
  - `Queries/GetPendingSamples/…`
- **OutsourcedSamples/**
  - `Commands/MarkTestAsOutsourced/…`
  - `Commands/SettleOutsourcedAccount/…`
  - `Queries/GetOutsourcedSamples/…`

### لن يتم:
- كتابة SQL View للـPatientHistory (Infrastructure).
- منطق تسوية العينات.

## الأقسام المرجعية
- `implementation_plan.md` → Features المذكورة.
- `MasrLab_Specifications_and_Audit.md` → Modules 6–9، القرار 1 (PatientHistory View).

## تعليمات التنفيذ للوكيل المحلي
نفس نمط المرحلة 9 (Command/Handler/Validator كهياكل فارغة).

## المحظورات
- لا SQL هنا.
- لا Logic.

## ناتج المرحلة المتوقع
هياكل الموديولات 6–9 قائمة، والمشروع يُبنى.

## معايير القبول
- [ ] المجلدات الأربعة موجودة.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Master Data (Modules 10–14).

---

# المرحلة 11 — Application/Features (Modules 10–14): Master Data

## الهدف
هياكل Features للموديولات: TestsMasterData، PriceLists، FixedComments، TestGroups، DoctorsAndReferrals.

## المتطلبات السابقة
اكتمال المرحلة 10.

## النطاق
### سيتم إنشاء المجلدات بأزواج Commands/Queries كالمعتاد:
- `TestsMasterData/`
- `PriceLists/`
- `FixedComments/`
- `TestGroups/`
- `DoctorsAndReferrals/`

كل Feature يحوي عمليات CRUD الرئيسية كهياكل (Add/Update/Delete/Get) بحسب ما ورد في `implementation_plan.md` وأسماء العمليات في `MasrLab_Specifications_and_Audit.md` (Modules 10–14) — دون اختراع عمليات إضافية.

### لن يتم:
- تنفيذ منطق CRUD.
- استنتاج عمليات غير مذكورة.

## الأقسام المرجعية
- `implementation_plan.md` → Features/TestsMasterData … DoctorsAndReferrals.
- `MasrLab_Specifications_and_Audit.md` → Modules 10–14.

## تعليمات التنفيذ للوكيل المحلي
نفس النمط: Command + Handler + Validator، أو Query + Handler.

## المحظورات
- لا Logic ولا Validation.
- لا ملفات إضافية.

## ناتج المرحلة المتوقع
5 موديولات Master Data كهياكل جاهزة، والمشروع يُبنى.

## معايير القبول
- [ ] المجلدات الخمسة موجودة.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Users/Permissions + Attendance/Audit (Modules 15–16).

---

# المرحلة 12 — Application/Features (Modules 15–16): Users / Attendance / Audit

## الهدف
هياكل Features لإدارة المستخدمين والصلاحيات، والحضور وسجل التدقيق.

## المتطلبات السابقة
اكتمال المرحلة 11.

## النطاق
### سيتم:
- `UsersAndPermissions/`
  - Commands: AddUser، UpdateUser، AssignPermission، ResetPassword.
  - Queries: GetUsers، GetUserPermissions.
- `AttendanceAndAudit/`
  - Commands: LogLogin، LogLogout.
  - Queries: GetAttendanceReport، GetAuditLog.

(الأسماء تُطابق ما تنص عليه المراجع؛ لا تخترع عمليات غير مذكورة.)

## الأقسام المرجعية
- `implementation_plan.md` → Features/UsersAndPermissions، AttendanceAndAudit.
- `MasrLab_Specifications_and_Audit.md` → Modules 15–16 + مصفوفة الصلاحيات الـ13.

## تعليمات التنفيذ للوكيل المحلي
هياكل CQRS الفارغة كالنمط السابق.

## المحظورات
- لا منطق تشفير أو تجزئة كلمات مرور هنا.
- لا Logic.

## ناتج المرحلة المتوقع
موديولا المستخدمين والحضور جاهزان كهياكل.

## معايير القبول
- [ ] المجلدان موجودان.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Drawers & Accounting (Modules 17–19).

---

# المرحلة 13 — Application/Features (Modules 17–19): Accounting / Drawers

## الهدف
هياكل Features لـ: جرد الفترة (Period Drawer)، جرد الطبيب/الجهة، جرد نوع الحساب/العينات.

## المتطلبات السابقة
اكتمال المرحلة 12.

## النطاق
### سيتم:
- `Accounting/`
  - Commands: `RecordCashTransaction`.
  - Queries: `GetPeriodDrawer`, `GetDoctorReferralDrawer`, `GetAccountTypeDrawer`.

(يُبقى التنظيم موحّداً تحت `Accounting/`، وإن نصّت الخطة على تفصيل أكثر يُتّبع.)

### لن يتم:
- منطق حساب الأرباح/الخصم.
- منطق كلمة مرور الجرد.

## الأقسام المرجعية
- `implementation_plan.md` → Features/Accounting.
- `MasrLab_Specifications_and_Audit.md` → Modules 17–19.

## تعليمات التنفيذ للوكيل المحلي
هياكل CQRS الفارغة.

## المحظورات
- لا Logic حسابي.
- لا Validators ملموسة.

## ناتج المرحلة المتوقع
موديولات الحسابات كهيكل جاهز.

## معايير القبول
- [ ] المجلد `Accounting/` موجود ومكتمل الهياكل.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Statistics + SystemSettings (Modules 20–21) وبها تكتمل طبقة Application.

---

# المرحلة 14 — Application/Features (Modules 20–21): Statistics + Settings

## الهدف
هياكل Features للإحصائيات وإعدادات النظام، وبها تكتمل Application layer.

## المتطلبات السابقة
اكتمال المرحلة 13.

## النطاق
### سيتم:
- `Statistics/`
  - Queries: AnnualPatientsStats، MonthlyPatientsStats، TestRequestRateStats، SamplesYearStats، GenderStats، MonthsOfYearStats.
- `SystemSettings/`
  - Commands: UpdateSetting، ConfigurePrinter، UpdateReportTemplate.
  - Queries: GetSetting، GetPrinters، GetReportTemplate.

## الأقسام المرجعية
- `implementation_plan.md` → Features/Statistics، SystemSettings.
- `MasrLab_Specifications_and_Audit.md` → Modules 20–21.

## تعليمات التنفيذ للوكيل المحلي
هياكل CQRS الفارغة.

## المحظورات
- لا Logic إحصائي.
- لا قراءة/كتابة إعدادات فعلية.

## ناتج المرحلة المتوقع
هياكل Statistics و SystemSettings جاهزة، ومشروع Application يبني بنجاح كامل.

## معايير القبول
- [ ] المجلدان موجودان.
- [ ] `dotnet build` ينجح.
- [ ] عدد مجلدات `Features/` = 19 موديولاً (مطابقاً للخطة).

## ملاحظات للمرحلة التالية
البدء في Infrastructure: DbContext + Interceptors + Migrations skeleton (المرحلة 15).

---

# المرحلة 15 — مشروع Infrastructure: DbContext + Interceptors + Migrations skeleton

## الهدف
إنشاء مشروع `MasrLab.Infrastructure` مع الهيكل الرئيسي لـ `Persistence/`، بما فيه `MasrLabDbContext`، وInterceptors فارغة، ومجلدات Migrations/Seeding/Views، بدون أي Fluent API بعد.

## المتطلبات السابقة
اكتمال طبقات Domain وApplication (المراحل 1–14).

## النطاق
### سيتم:
- إنشاء `src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj` (Class Library — net8.0).
- إضافة مراجع مشاريع إلى `MasrLab.Domain` و`MasrLab.Application`.
- تثبيت حزم NuGet:
  - `Microsoft.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Design`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.DependencyInjection.Abstractions`
- إنشاء مجلدات:
  - `Persistence/`
  - `Persistence/Configurations/{Core,Culture,Financial,Administrative,Settings}/`
  - `Persistence/Repositories/`
  - `Persistence/Views/`
  - `Persistence/Interceptors/`
  - `Persistence/Migrations/` (فارغ)
  - `Persistence/Seeding/`
  - `Services/`
- إنشاء الملفات:
  - `Persistence/MasrLabDbContext.cs` مع DbSets لكل 30 كياناً (بدون OnModelCreating body — فقط `base.OnModelCreating(modelBuilder);`).
  - `Persistence/UnitOfWork.cs` (Class فارغ ينفّذ `IUnitOfWork`، مع NotImplementedException).
  - `Persistence/Interceptors/AuditableEntityInterceptor.cs` (Class فارغ يرث `SaveChangesInterceptor`).
  - `Persistence/Interceptors/SoftDeleteInterceptor.cs` (Class فارغ يرث `SaveChangesInterceptor`).
  - `DependencyInjection.cs` مع `AddInfrastructure` stub.

### لن يتم:
- كتابة Fluent API (المرحلتان 16–17).
- كتابة Repositories (المرحلة 18).
- كتابة أي Service (المرحلة 19).
- تشغيل Migrations فعلياً.

## الأقسام المرجعية
- `implementation_plan.md` → «MasrLab.Infrastructure → Persistence/».
- `MasrLab_Specifications_and_Audit.md` → القسمان 7.3 و7.4.

## تعليمات التنفيذ للوكيل المحلي
> استخدم `DbContext` مع Constructor يستقبل `DbContextOptions<MasrLabDbContext>`.
> DbSets: صنّفها بترتيب المجلدات (Core, Culture, Financial, Administrative, Settings).
> مجلد `Migrations/` يبقى فارغاً في هذه المرحلة.

## المحظورات
- لا OnModelCreating فعليّ.
- لا Interceptor logic.
- لا Seeder logic.
- لا Repositories.

## ناتج المرحلة المتوقع
مشروع Infrastructure قابل للبناء، مع 30 DbSet داخل `MasrLabDbContext` وهياكل مجلدات كاملة.

## معايير القبول
- [ ] المشروع موجود في الحل.
- [ ] المراجع لـ Domain و Application مضافة.
- [ ] حزم EF Core مثبّتة.
- [ ] `MasrLabDbContext` يحوي 30 `DbSet<T>`.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Fluent API Configurations للنواة والمزارع (المرحلة 16).

---

# المرحلة 16 — Fluent API Configurations (Core + Culture)

## الهدف
إنشاء ملفات إعدادات Fluent API لكل كيان في Core (11) وCulture (4)، كهياكل فارغة تنفّذ `IEntityTypeConfiguration<T>` مع body فارغ (`{ }`).

## المتطلبات السابقة
اكتمال المرحلة 15.

## النطاق
### سيتم:
داخل `Persistence/Configurations/Core/`: 11 ملف Configuration مقابلاً لكل كيان في Core.
داخل `Persistence/Configurations/Culture/`: 4 ملفات Configuration.

اسم كل ملف: `<EntityName>Configuration.cs`، الكلاس يطبّق `IEntityTypeConfiguration<TEntity>` مع Method `Configure` فارغة.

### لن يتم:
- كتابة قواعد HasKey/HasOne/HasMany.
- كتابة قواعد الفهارس.
- تفعيل Query Filters للـ Soft Delete هنا.

## الأقسام المرجعية
- `implementation_plan.md` → «Configurations/Core & Culture».

## تعليمات التنفيذ للوكيل المحلي
> أنشئ الملفات بأسماء دقيقة (PatientConfiguration، VisitTestConfiguration، …).
> Method `Configure(EntityTypeBuilder<T> builder) { }` — فقط.

## المحظورات
- لا Logic في Configure.
- لا حقول جديدة.

## ناتج المرحلة المتوقع
15 ملف Configuration فارغاً (Core + Culture).

## معايير القبول
- [ ] عدد الملفات الجديدة = 15.
- [ ] كل ملف يطبّق `IEntityTypeConfiguration<T>` الصحيح.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
باقي الـ Configurations (Financial + Administrative + Settings) في المرحلة 17.

---

# المرحلة 17 — Fluent API Configurations (Financial + Administrative + Settings)

## الهدف
استكمال ملفات Fluent API للأقسام المتبقية (Financial 4 + Administrative 6 + Settings 6 = 16 ملفاً).

## المتطلبات السابقة
اكتمال المرحلة 16.

## النطاق
### سيتم:
- `Persistence/Configurations/Financial/` (4 ملفات)
- `Persistence/Configurations/Administrative/` (6 ملفات)
- `Persistence/Configurations/Settings/` (6 ملفات)

جميعها ملفات هياكل فارغة كما في المرحلة 16.

## الأقسام المرجعية
- `implementation_plan.md` → «Configurations/Financial, Administrative, Settings».

## تعليمات التنفيذ للوكيل المحلي
نفس نمط المرحلة 16.

## المحظورات
- لا Logic في Configure.

## ناتج المرحلة المتوقع
16 ملف Configuration إضافياً، والمشروع يُبنى.

## معايير القبول
- [ ] إجمالي ملفات Configurations في Infrastructure = 31 (30 لكل كيان + أي ملفات ربط).
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Repositories + UnitOfWork + Views + Seeders في المرحلة 18.

---

# المرحلة 18 — Repositories وViews وSeeders (هياكل)

## الهدف
إنشاء تطبيقات هياكل للمستودعات (GenericRepository + 7 مستودعات مخصصة)، وSeeders الافتراضية، وSQL View المشتق.

## المتطلبات السابقة
اكتمال المرحلة 17.

## النطاق
### سيتم:
داخل `Persistence/Repositories/`:
- `GenericRepository.cs`  ← يطبّق `IRepository<T>`.
- `PatientRepository.cs`  ← يطبّق `IPatientRepository`.
- `VisitRepository.cs`
- `TestResultRepository.cs`
- `CultureRepository.cs`
- `AccountingRepository.cs`
- `StatisticsRepository.cs`
- `AuditLogRepository.cs`

داخل `Persistence/Views/`:
- `PatientHistoryView.cs`  ← Keyless Entity Type (Skeleton) مطابقاً لقرار الفجوة 1.

داخل `Persistence/Seeding/`:
- `DefaultAdminSeeder.cs`
- `DefaultSettingsSeeder.cs`

**كلها هياكل**: Constructors تستقبل `MasrLabDbContext`، وأي Method مصرَّح ترمي `NotImplementedException()`.

### لن يتم:
- كتابة LINQ/EF Queries.
- كتابة كلمات مرور فعلية.
- كتابة سكربت SQL للـ View.

## الأقسام المرجعية
- `implementation_plan.md` → «Repositories/, Views/, Seeding/».
- `MasrLab_Specifications_and_Audit.md` → القرار 1، القرار 4 (EGP).

## تعليمات التنفيذ للوكيل المحلي
> أنشئ الكلاسات بأسماء ملفاتها المطابقة، مع Constructors المذكورة.
> `PatientHistoryView` يُعلَن ككلاس Keyless (لا يرث BaseEntity).

## المحظورات
- لا Logic فعلية.
- لا كلمات مرور نصّية غير `"123"` (وحتى هذه لا تُطبَّق كتجزئة الآن — تُترك كـ TODO في Seeder).

## ناتج المرحلة المتوقع
هياكل المستودعات والـ View والـ Seeders جاهزة.

## معايير القبول
- [ ] 8 ملفات Repositories.
- [ ] `PatientHistoryView.cs` موجود.
- [ ] 2 ملفَي Seeders موجودَان.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
خدمات Infrastructure (Auth/Print/Barcode/Backup) في المرحلة 19.

---

# المرحلة 19 — خدمات Infrastructure (Services)

## الهدف
إنشاء هياكل خدمات البنية التحتية: المصادقة، التاريخ/الوقت، المستخدم الحالي، النسخ الاحتياطي، الباركود، الطباعة.

## المتطلبات السابقة
اكتمال المرحلة 18.

## النطاق
### سيتم:
داخل `MasrLab.Infrastructure/Services/`:
- `AuthenticationService.cs`
- `DateTimeService.cs`
- `CurrentUserService.cs`
- `BackupService.cs`
- `BarcodeService.cs`
- `PrintService.cs`

كل خدمة:
- كلاس عام.
- تنفذ واجهة إن ذكرت الخطة اسمها (وإلا Class مباشر).
- Methods مصرَّحة فقط بترمي `NotImplementedException()`.

### لن يتم:
- كتابة تجزئة (bcrypt/SHA-256) فعلية.
- كتابة تكامل حقيقي مع الطابعات أو الباركود.
- تنفيذ منطق النسخ الاحتياطي.

## الأقسام المرجعية
- `implementation_plan.md` → «Infrastructure/Services».
- `MasrLab_Specifications_and_Audit.md` → القسم 7.5 (مصادقة)، 7.6 (طباعة)، 7.7 (باركود)، 7.9 (نسخ احتياطي).

## تعليمات التنفيذ للوكيل المحلي
> صرّح الكلاسات في namespace `MasrLab.Infrastructure.Services`.
> إن كانت الخدمة لها Interface في Application (وفق الخطة) أضِف المرجع اللازم دون كتابة Logic.

## المحظورات
- لا تطبّق أي خوارزمية.
- لا تفتح ملفات النظام أو منافذ الطابعات.

## ناتج المرحلة المتوقع
6 ملفات خدمات كهياكل، والمشروع يُبنى.

## معايير القبول
- [ ] 6 ملفات موجودة.
- [ ] `AddInfrastructure` في `DependencyInjection.cs` تسجّل الأنواع (كـ stub) أو تُترك فارغة إن كانت الخطة تفصلها لاحقاً.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
البدء في Presentation (WPF) في المرحلة 20.

---

# المرحلة 20 — مشروع Presentation (WPF) + Resources + Navigation

## الهدف
إنشاء مشروع `MasrLab.Presentation` كتطبيق WPF مع الموارد الأساسية (Styles/Icons/Fonts/Converters) وخدمة التنقّل.

## المتطلبات السابقة
اكتمال طبقات Domain / Application / Infrastructure (المراحل 1–19).

## النطاق
### سيتم:
- إنشاء `src/MasrLab.Presentation/MasrLab.Presentation.csproj` كـ WPF App (net8.0-windows، UseWPF=true).
- إضافة مراجع مشاريع: `Application` و`Infrastructure`.
- إضافة الحزم:
  - `CommunityToolkit.Mvvm`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.Configuration.Json`
- الملفات الجذرية:
  - `App.xaml` + `App.xaml.cs`
  - `MainWindow.xaml` + `MainWindow.xaml.cs`
  - `appsettings.json` (ConnectionString، Currency=EGP، LabName، LabLogoPath) — قيم فارغة/افتراضية فقط.
  - `DependencyInjection.cs` مع `AddPresentation` stub.
- المجلدات:
  - `Resources/Styles/GlobalStyles.xaml` (فارغ ResourceDictionary).
  - `Resources/Icons/` (فارغ + `.gitkeep`).
  - `Resources/Images/` (فارغ + `.gitkeep`).
  - `Resources/Converters/` (بها Class واحد فارغ `BooleanToVisibilityConverter.cs` إذا نصّت الخطة).
  - `Resources/Fonts/` (فارغ).
  - `Navigation/INavigationService.cs`
  - `Navigation/NavigationService.cs`
  - `Navigation/NavigationStore.cs`
  - `ViewModels/` (فارغ)
  - `Views/` (فارغ)
  - `Controls/` (فارغ)
  - `Printing/Reports/` (فارغ)
  - `Behaviors/` (فارغ)
  - `Helpers/` (فارغ)
- ضبط `App.xaml` لدمج `GlobalStyles.xaml`.
- ضبط `MainWindow` FlowDirection=`RightToLeft` وLanguage=`ar-EG`.

### لن يتم:
- كتابة أي ViewModel أو View فعلي (المراحل 21–23).
- كتابة تقارير Printing (المرحلة 24).
- تنفيذ `NavigationService` (Skeleton فقط).

## الأقسام المرجعية
- `implementation_plan.md` → «MasrLab.Presentation».
- `MasrLab_Specifications_and_Audit.md` → القسم 8 (UI/Navigation)، RTL.

## تعليمات التنفيذ للوكيل المحلي
> تأكّد أن المشروع من نوع WPF ويستهدف `net8.0-windows`.
> `App.xaml.cs` يحتوي `OnStartup` فارغة الآن.
> `INavigationService` واجهة بدون أعضاء (Marker) إن لم تنص الخطة على أعضاء بعينها.

## المحظورات
- لا Logic للتنقّل.
- لا Bindings لـ ViewModels غير موجودة.

## ناتج المرحلة المتوقع
مشروع WPF يعمل ويُظهر MainWindow فارغة بـ RTL، مع الموارد وBنية Navigation جاهزة.

## معايير القبول
- [ ] المشروع في الحل، ومرجعا Application/Infrastructure مضافان.
- [ ] `appsettings.json` موجود.
- [ ] `MainWindow` FlowDirection = RightToLeft.
- [ ] `dotnet build` ينجح.
- [ ] التشغيل يُظهر نافذة فارغة (اختياري للتحقق البصري).

## ملاحظات للمرحلة التالية
هياكل ViewModels و Views للموديولات 1–9 في المرحلة 21.

---

# المرحلة 21 — Presentation/ViewModels + Views (Modules 1–9)

## الهدف
إنشاء هياكل ViewModels و Views للموديولات الأساسية 1–9.

## المتطلبات السابقة
اكتمال المرحلة 20.

## النطاق
### سيتم:
داخل `ViewModels/` و`Views/` (بمجلدات مطابقة لأسماء الـFeatures):

ViewModels:
- `MainViewModel.cs`
- `LoginViewModel.cs`
- `PatientManagement/RegisterPatientViewModel.cs`, `UpdatePatientAccountViewModel.cs`, `DeliverResultsViewModel.cs`, `UpdatePatientDataViewModel.cs`
- `ResultsEntry/EnterResultsViewModel.cs`, `CombinedReportViewModel.cs`, `BlankReportViewModel.cs`
- `Cultures/CultureResultViewModel.cs`, `AddCultureViewModel.cs`, `AddAntibioticViewModel.cs`
- `PatientSearch/SearchPatientsViewModel.cs`, `VisitHistoryViewModel.cs`
- `CasesFollowUp/CasesFollowUpViewModel.cs`
- `PatientHistory/PatientHistoryViewModel.cs`
- `WorkSheets/WorkSheetsViewModel.cs`
- `SampleCollection/SampleCollectionViewModel.cs`
- `OutsourcedSamples/OutsourcedSamplesViewModel.cs`

Views (لكل ViewModel):
- `<Name>View.xaml` + `<Name>View.xaml.cs` مع Grid فارغ وFlowDirection RTL.

كل ViewModel: يرث من `ObservableObject` (CommunityToolkit.Mvvm) بدون أي Property/Command.

### لن يتم:
- Bindings فعلية.
- Commands ملموسة.

## الأقسام المرجعية
- `implementation_plan.md` → «Presentation/ViewModels & Views».

## تعليمات التنفيذ للوكيل المحلي
> ضع كل ViewModel مع View الخاصة به داخل مجلد المو­ديول.
> `MainWindow.xaml` يستضيف ContentControl واحد لعرض الـView الحالية.

## المحظورات
- لا Logic ولا Commands.

## ناتج المرحلة المتوقع
هياكل UI للـ 9 موديولات الأساسية جاهزة.

## معايير القبول
- [ ] كل ViewModel مقابل View XAML.
- [ ] `dotnet build` ينجح.
- [ ] التشغيل لا يُنتج Exception عند فتح أي View.

## ملاحظات للمرحلة التالية
هياكل UI لـ Master Data (10–14) في المرحلة 22.

---

# المرحلة 22 — Presentation ViewModels/Views (Modules 10–14): Master Data

## الهدف
هياكل ViewModels و Views لموديولات Master Data.

## المتطلبات السابقة
اكتمال المرحلة 21.

## النطاق
### سيتم إنشاء:
- `TestsMasterData/TestsMasterDataViewModel.cs` + View.
- `PriceLists/PriceListsViewModel.cs` + View.
- `FixedComments/FixedCommentsViewModel.cs` + View.
- `TestGroups/TestGroupsViewModel.cs` + View.
- `DoctorsAndReferrals/DoctorsReferralsViewModel.cs` + View.

## الأقسام المرجعية
- `implementation_plan.md` → Views/ViewModels للموديولات 10–14.

## تعليمات التنفيذ للوكيل المحلي
نفس نمط المرحلة 21 (Skeleton).

## المحظورات
- لا Bindings، لا Logic.

## ناتج المرحلة المتوقع
5 مجموعات UI جاهزة كهياكل.

## معايير القبول
- [ ] 5 ViewModels و 5 Views في المجلدات الصحيحة.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
Administrative + Financial + Statistics + Settings UI في المرحلة 23.

---

# المرحلة 23 — Presentation ViewModels/Views (Modules 15–21)

## الهدف
هياكل UI لموديولات المستخدمين والحضور والحسابات والإحصائيات والإعدادات.

## المتطلبات السابقة
اكتمال المرحلة 22.

## النطاق
### سيتم:
- `UsersAndPermissions/UsersPermissionsViewModel.cs` + View.
- `AttendanceAndAudit/AttendanceAuditViewModel.cs` + View.
- `Accounting/PeriodDrawerViewModel.cs` + View.
- `Accounting/DoctorReferralDrawerViewModel.cs` + View.
- `Accounting/AccountTypeDrawerViewModel.cs` + View.
- `Statistics/StatisticsViewModel.cs` + View.
- `SystemSettings/SystemSettingsViewModel.cs` + View.

## الأقسام المرجعية
- `implementation_plan.md` → «Presentation/ViewModels لـ الموديولات 15–21».

## تعليمات التنفيذ للوكيل المحلي
نفس نمط المرحلتين 21 و22.

## المحظورات
- لا Logic ولا Bindings.

## ناتج المرحلة المتوقع
باقي واجهات الموديولات كهياكل، وطبقة Presentation اكتملت من ناحية Views/ViewModels.

## معايير القبول
- [ ] كل ViewModel له View مقابل.
- [ ] عدد المجلدات في `ViewModels/` = 19 موديولاً (مع Login/Main).
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
هياكل تقارير الطباعة العشرين في المرحلة 24.

---

# المرحلة 24 — Presentation/Printing (Reports Skeleton)

## الهدف
إنشاء هياكل الفئات المسؤولة عن التقارير العشرين المذكورة في المواصفات.

## المتطلبات السابقة
اكتمال المرحلة 23.

## النطاق
### سيتم داخل `MasrLab.Presentation/Printing/Reports/`:
- `IndividualResultReport.cs`
- `CombinedReport.cs`
- `BlankReport.cs`
- `CultureReport.cs`
- `PatientHistoryReport.cs`
- `WorkSheetReport.cs`
- `PriceListReport.cs`
- `ReceiptReport.cs`
- `DrawerReport.cs`
- `StatisticsReport.cs`
- `AttendanceReport.cs`

(هذه هي الفئات الأساسية التي تغطي التقارير الـ20 وفق تجميع الخطة؛ لا تُخلق فئات إضافية غير مذكورة.)

كل ملف:
- كلاس عام بلا أعضاء منطقية.
- Constructor فارغ.

### لن يتم:
- توليد قوالب FlowDocument أو تخطيط A4/A5 فعلي.
- ربط بالطابعات.

## الأقسام المرجعية
- `implementation_plan.md` → «Printing/Reports».
- `MasrLab_Specifications_and_Audit.md` → القسم 6 (قائمة التقارير 20).

## تعليمات التنفيذ للوكيل المحلي
> أنشئ 11 ملفاً بأسمائها الحرفية.
> لا تربط أياً منها بالطابعات أو قوالب التقرير.

## المحظورات
- لا Logic طباعة.
- لا فتح ملفات موارد.
- لا تعديل خارج مجلد Printing.

## ناتج المرحلة المتوقع
هياكل تقارير الطباعة جاهزة داخل Printing/Reports.

## معايير القبول
- [ ] 11 ملفاً موجوداً.
- [ ] `dotnet build` ينجح.

## ملاحظات للمرحلة التالية
مشاريع الاختبارات الثلاثة في المرحلة 25.

---

# المرحلة 25 — مشاريع الاختبارات (Test Projects Skeleton)

## الهدف
إنشاء مشاريع الاختبار الثلاثة كهياكل، دون كتابة اختبارات فعلية.

## المتطلبات السابقة
اكتمال المرحلة 24.

## النطاق
### سيتم:
- `tests/MasrLab.Domain.Tests/MasrLab.Domain.Tests.csproj`.
- `tests/MasrLab.Application.Tests/MasrLab.Application.Tests.csproj`.
- `tests/MasrLab.Infrastructure.Tests/MasrLab.Infrastructure.Tests.csproj`.

لكل مشروع:
- Framework: xUnit (`xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- مرجع مشروع للطبقة المقابلة.
- ملف واحد `PlaceholderTests.cs` بكلاس فارغ (بدون [Fact]).

### لن يتم:
- كتابة أي اختبار فعلي.
- إضافة FluentAssertions أو Moq (إلا إذا نصت الخطة صراحة).

## الأقسام المرجعية
- `implementation_plan.md` → «tests/».

## تعليمات التنفيذ للوكيل المحلي
> أنشئ المشاريع باستخدام قالب xUnit (`dotnet new xunit`).
> أضِف مرجع المشروع المستهدَف.
> اترك `PlaceholderTests.cs` بلا Facts.

## المحظورات
- لا اختبارات فعلية.
- لا Mocks.

## ناتج المرحلة المتوقع
3 مشاريع اختبار قابلة للبناء، مضافة إلى الحل.

## معايير القبول
- [ ] 3 ملفات `.csproj` جديدة داخل `tests/`.
- [ ] كل منها يرجع للمشروع المقابل.
- [ ] `dotnet test` يُنفَّذ دون أخطاء (0 اختبارات مقبول).

## ملاحظات للمرحلة التالية
التحقق النهائي وضبط تبعيات الحل (المرحلة 26).

---

# المرحلة 26 — التحقق النهائي وضبط تبعيات الحل

## الهدف
مراجعة الحل بالكامل، تصحيح مراجع المشاريع، تسجيل الخدمات في `DependencyInjection.cs` بكل طبقة (كـ stubs)، والتأكد من أن الحل يبني ويعمل نظرياً.

## المتطلبات السابقة
اكتمال المراحل 0–25.

## النطاق
### سيتم:
- التأكد من مراجع المشاريع:
  - Application → Domain.
  - Infrastructure → Domain, Application.
  - Presentation → Application, Infrastructure.
  - كل مشروع اختبار → المشروع المقابل.
- التأكد من تسجيل مبدئي في:
  - `MasrLab.Application/DependencyInjection.cs` (AddApplication).
  - `MasrLab.Infrastructure/DependencyInjection.cs` (AddInfrastructure) — تسجيل `DbContext` بـ SqlServer وإن كان بسلسلة اتصال من `appsettings.json`.
  - `MasrLab.Presentation/DependencyInjection.cs` (AddPresentation).
- التأكد من أن `App.xaml.cs` في Presentation يستدعي `AddApplication + AddInfrastructure + AddPresentation` عند بدء التطبيق.
- تشغيل `dotnet restore` ثم `dotnet build` على مستوى الحل.

### لن يتم:
- كتابة Business Logic.
- تفعيل Migrations فعلياً.
- تشغيل قاعدة بيانات حقيقية.

## الأقسام المرجعية
- `implementation_plan.md` → «Solution Structure», «DependencyInjection».

## تعليمات التنفيذ للوكيل المحلي
> راجع كل `.csproj` وتأكد من `ProjectReference`s.
> نسّق `App.xaml.cs` ليستدعي التسجيلات الثلاثة (Skeleton).
> شغّل `dotnet build MasrLab.sln` وتأكد من نجاح البناء بدون أي Compilation Error.

## المحظورات
- لا تُدخل Logic في المسجّلات.
- لا تحذف ملفات.
- لا تنشئ ملفات جديدة خارج ما ذُكر.

## ناتج المرحلة المتوقع
حل MasrLab كامل الهيكل، يبني بنجاح، بلا Business Logic، جاهز لبدء مراحل التطبيق الفعلي في مسارات لاحقة.

## معايير القبول
- [ ] كل المشاريع الأربعة الأساسية + 3 مشاريع اختبار في الحل.
- [ ] المراجع بين المشاريع سليمة.
- [ ] `dotnet build MasrLab.sln` ينجح بلا أخطاء.
- [ ] `dotnet test MasrLab.sln` ينجح (0 اختبارات مقبول).
- [ ] `App.xaml.cs` يستدعي التسجيلات الثلاثة كـ stubs.
- [ ] هيكل المجلدات مطابق تماماً لخطة `implementation_plan.md`.

## ملاحظات للمرحلة التالية
انتهى نطاق «بناء الهيكل». المراحل التالية (خارج هذه الوثيقة) ستتناول: تطبيق Fluent API الفعلي، Interceptors، Handlers، Bindings، Reports Rendering، وMigrations أولى.

---

# قواعد شاملة تسري على جميع المراحل

1. **مصدر الحقيقة الوحيد**: `implementation_plan.md` + `MasrLab_Specifications_and_Audit.md`.
2. **الهيكل فقط**: لا Business Logic، لا Validation، لا Services، لا Algorithms.
3. **عدم التكرار**: لا يُنشأ ملف مرتين، ولا يُعدَّل نفس الملف في مرحلتين إلا للضرورة القصوى (`App.xaml.cs`، `DependencyInjection.cs`، `MasrLabDbContext.cs` حين تُضاف DbSets تدريجياً — وفي هذه الوثيقة كلها أنشئت مرة واحدة).
4. **احترام التبعيات**: Domain ← Application ← Infrastructure ← Presentation ← Tests.
5. **قواعد التسمية**:
   - Types & Public Members: PascalCase.
   - Interfaces: تسبق بـ `I`.
   - Enums: PascalCase مفرد.
   - أسماء View تطابق ViewModel (View suffix).
   - أسماء Configuration تطابق Entity + `Configuration`.
6. **RTL دائماً** في XAML، `FlowDirection="RightToLeft"`.
7. **عدم استنتاج أعضاء** غير موجودة في المراجع.
8. **البناء إلزامي** في نهاية كل مرحلة.

---

# النتيجة النهائية

بتنفيذ المراحل 0–26 بالتتابع دون قفزات:
- **4 مشاريع أساسية** (Domain / Application / Infrastructure / Presentation).
- **3 مشاريع اختبار**.
- **30 كياناً** موزّعة على 5 أقسام.
- **9 واجهات مستودعات**.
- **19 موديولاً وظيفياً** في Features (كلها هياكل CQRS).
- **30 ملف Configuration** فارغاً لـ Fluent API.
- **8 مستودعات + UnitOfWork + View + Seeders** كهياكل.
- **6 خدمات Infrastructure** كهياكل.
- **~30 ViewModel و View** بواجهات RTL فارغة.
- **11 كلاس Report** كهياكل لتقارير الطباعة الـ20.
- **حل يبنى بنجاح كامل** بلا أي Business Logic — جاهز لمراحل التطبيق الفعلي التالية.
