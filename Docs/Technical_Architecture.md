# MasrLab — Technical Architecture Documentation

> **الغرض من هذا الملف:** توثيق شامل للتقنيات، الحزم، الإصدارات، الهيكل المعماري، والأنماط المعتمدة في مشروع MasrLab. يُعدّ هذا الملف **المرجع الأول** لأي وكيل برمجي أو مطوّر يعمل على المشروع.
>
> **تاريخ الإنشاء:** 2026-07-26
>
> **آخر تحديث:** 2026-07-26
>
> **حالة المشروع:** هيكل مكتمل — جاهز لمرحلة كتابة أكواد منطق الأعمال (Business Logic)

---

## 1. نظرة عامة على المشروع

**MasrLab** هو نظام إدارة معمل تحاليل طبية مبني بتقنية **WPF** على منصة **.NET 8**، يستهدف بيئة **الشبكات المحلية (LAN)** مع واجهة عربية **RTL** بالكامل.

### الميزات الرئيسية:
- إدارة المرضى والزيارات والتحاليل
- إدخال النتائج والتقارير (فردية + مجمعة)
- المزارع والحساسية (Microbiology)
- البحث المتعدد المعايير
- سجل التاريخ المرضي (SQL View)
- المحاسبة والجرد (فترة / طبيب / نوع حساب)
- الإحصائيات
- إعدادات النظام
- نظام طباعة بـ 4 طابعات مستقلة + باركود
- نسخ احتياطي يومي
- مصادقة بمستويين (Admin / User)

---

## 2. البيئة التقنية

| البند | القيمة |
|---|---|
| **إطار العمل** | .NET 8.0 |
| **إصدار SDK** | 8.0.0 (مع `rollForward: latestFeature`) |
| **لغة البرمجة** | C# (latest) |
| **نوع الإخراج** | WinExe (تطبيق مكتبي) |
| **منصة الواجهة** | WPF (Windows Presentation Foundation) |
| **قاعدة البيانات** | SQL Server |
| **ORM** | Entity Framework Core 8 |
| **نمط الهيكل** | Clean Architecture + MVVM |
| **نظام التشغيل المستهدف** | Windows فقط |

### إعدادات عامة عبر `Directory.Build.props`:
```xml
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<TreatWarningsAsErrors>false</TreatWarningsAsErrors>
```

---

## 3. هيكل الحل (Solution Structure)

```
MasrLab.sln
├── src/
│   ├── MasrLab.Domain/              ← طبقة النطاق (الأكثر استقلالية)
│   ├── MasrLab.Application/         ← طبقة التطبيق (Use Cases)
│   ├── MasrLab.Infrastructure/      ← طبقة البنية التحتية (EF Core + Services)
│   └── MasrLab.Presentation/        ← طبقة العرض (WPF)
│
└── tests/
    ├── MasrLab.Domain.Tests/        ← اختبارات طبقة النطاق
    ├── MasrLab.Application.Tests/   ← اختبارات طبقة التطبيق
    └── MasrLab.Infrastructure.Tests/ ← اختبارات طبقة البنية التحتية
```

---

## 4. تفاصيل كل طبقة

### 4.1 MasrLab.Domain — طبقة النطاق

> **الغرض:** الكيانات (Entities)، العلاقات، القواعد التجارية، والعقود (Interfaces). **لا تعتمد على أي مشروع آخر.**

| البند | التفاصيل |
|---|---|
| **Target Framework** | `net8.0` |
| **الحزم الخارجية** | لا توجد (طبقة نقية) |
| **المجلدات** | `Common/`, `Entities/`, `Interfaces/`, `Exceptions/` |

#### المحتويات:
- **`Common/`**: `BaseEntity.cs`، `IAuditableEntity.cs`، `ISoftDeletable.cs`
- **`Common/Enums/`**: 14 تعداداً (Gender, AccountType, VisitStatus, SampleStatus, ResultStatus, SensitivityLevel, ReferralEntityType, TransactionType, AuditActionType, AuditEntityType, PrinterPurposeType, PaperSize, AgeUnit, WorkSheetType)
- **`Entities/`**: 30 كياناً مقسمة لـ 5 مجلدات فرعية (Core, Culture, Financial, Administrative, Settings)
- **`Interfaces/`**: 9 عقود مستودعات (IRepository, IUnitOfWork, IPatientRepository, إلخ)
- **`Exceptions/`**: 4 استثناءات مخصصة

---

### 4.2 MasrLab.Application — طبقة التطبيق

> **الغرض:** منطق الاستخدام (Use Cases) عبر نمط CQRS. تنسيق بين الكيانات والمستودعات. **تعتمد على Domain فقط.**

| البند | التفاصيل |
|---|---|
| **Target Framework** | `net8.0` |
| **المجلدات** | `Common/`, `Features/` |
| **عدد الموديولات** | 19 موديولاً في `Features/` |

#### الحزم المثبته:

| الحزمة | الإصدار | الغرض |
|---|---|---|
| **MediatR** | 12.5.0 | نمط CQRS — توجيه Commands و Queries إلى Handlers المناسبة |
| **AutoMapper** | 15.1.3 | تحويل الكيانات (Entities) إلى كائنات النقل (DTOs) والعكس |
| **FluentValidation** | 12.1.1 | التحقق من صحة مدخلات Commands قبل التنفيذ |
| **FluentValidation.DI.Extensions** | 12.1.1 | تسجيل validators تلقائياً في حاوية Dependency Injection |

#### المحتويات:
- **`Common/Interfaces/`**: 4 واجهاتخدمات (ICurrentUserService, IDateTimeService, IAuthenticationService, IPrintService)
- **`Common/DTOs/`**: 11 كائات نقل بيانات
- **`Common/Mappings/`**: AutoMapper Profile
- **`Common/Behaviors/`**: MediatR Pipeline Behaviors (Validation, Audit)
- **`Common/Helpers/`**: LabIdGenerator (يعتمد على IPatientRepository)
- **`Features/`**: 19 موديولاً بنمط CQRS (Command + Handler + Validator لكل عمليات الكتابة، Query + Handler للقراءة)

---

### 4.3 MasrLab.Infrastructure — طبقة البنية التحتية

> **الغرض:** التطبيق الفعلي للعقود المعرّفة في Domain و Application. ربط النظام بـ SQL Server وباقي الخدمات الخارجية.

| البند | التفاصيل |
|---|---|
| **Target Framework** | `net8.0` |
| **المجلدات** | `Persistence/`, `Services/` |
| **التبعيات** | Domain + Application |

#### الحزم المثبته:

| الحزمة | الإصدار | الغرض |
|---|---|---|
| **Microsoft.EntityFrameworkCore** | 8.0.* | ORM — مقارنة الكيانات بالجداول |
| **Microsoft.EntityFrameworkCore.SqlServer** | 8.0.* | موفّر SQL Server لـ EF Core |
| **Microsoft.EntityFrameworkCore.Design** | 8.0.* | أدوات التصميم في وقت التطوير (Design-time) |
| **Microsoft.EntityFrameworkCore.Tools** | 8.0.* | أوامر CLI (migrations, database update) |
| **Microsoft.Extensions.Configuration** | 8.0.* | قراءة ملفات التهيئة (appsettings.json) |
| **Microsoft.Extensions.DependencyInjection.Abstractions** | 8.0.* | واجهات DI لتسجيل الخدمات |

#### المحتويات:
- **`Persistence/MasrLabDbContext.cs`**: سياق EF Core الرئيسي
- **`Persistence/Configurations/`**: 30 إعداد Fluent API (مقسمة حسب تصنيف الكيانات)
- **`Persistence/Repositories/`**: 8 مستودعات (Generic, Patient, Visit, TestResult, Culture, Accounting, Statistics, AuditLog)
- **`Persistence/UnitOfWork.cs`**: تنفيذ وحدة العمل
- **`Persistence/Interceptors/`**: AuditableEntityInterceptor + SoftDeleteInterceptor
- **`Persistence/Seeding/`**: DefaultAdminSeeder + DefaultSettingsSeeder
- **`Persistence/Views/`**: PatientHistoryView.cs (keyless entity) + PatientHistoryView.sql (Placeholder)
- **`Persistence/Migrations/`**: مجلد محجوز (يحتوي .gitkeep فقط — لم يُولَّد أي Migration بعد بقرار DD-06)
- **`Services/`**: 6 خدمات (Authentication, DateTime, CurrentUser, Backup, Barcode, Print)

---

### 4.4 MasrLab.Presentation — طبقة العرض

> **الغرض:** تطبيق WPF الرئيسي (نقطة الدخول). واجهة المستخدم، نماذج العرض، والموارد.

| البند | التفاصيل |
|---|---|
| **Target Framework** | `net8.0-windows` |
| **نوع الإخراج** | WinExe |
| **UseWPF** | true |
| **اسم Assembly** | MasrLab (قرار DD-01) |
| **التبعيات** | Application + Infrastructure |

#### الحزم المثبته:

| الحزمة | الإصدار | الغرض |
|---|---|---|
| **CommunityToolkit.Mvvm** | 8.4.2 | أدوات MVVM — ObservableObject, RelayCommand, ObservableProperty |
| **Microsoft.Extensions.DependencyInjection** | 8.0.* | حاوية DI لتسجيل وحل الخدمات |
| **Microsoft.Extensions.Configuration** | 8.0.* | قراءة التهيئة |
| **Microsoft.Extensions.Configuration.Json** | 8.0.* | قراءة ملفات JSON |

#### المحتويات:
- **`App.xaml`**: نقطة الدخول + تسجيل 5 ResourceDictionaries (Colors → Global → Button → TextBox → DataGrid)
- **`MainWindow.xaml`**: النافذة الرئيسية
- **`Resources/Styles/`**: 5 ملفات XAML (Skeleton فارغة بقرار DD-07)
- **`Resources/Converters/`**: 5 محولات (BooleanToVisibilityConverter, GenderConverter, AccountTypeConverter, VisitStatusConverter, HighLowStatusConverter)
- **`Resources/{Icons,Images,Fonts}/`**: مجلدات محجوزة (.gitkeep)
- **`Behaviors/RtlBehavior.cs`**: Attached Property لدعم RTL
- **`ViewModels/`**: 19 مجلداً بنمط Feature-per-Module (قرار DD-05)
- **`Views/`**: 19 مجلداً مطابقاً لـ ViewModels
- **`Controls/`**: 5 عناصر تحكم مخصصة (IconButton, PatientInfoCard, TestSelector, SearchFilterBar, PrintPreviewControl)
- **`Navigation/`**: نظام التنقل (INavigationService, NavigationService, NavigationStore)
- **`Printing/`**: 11 تقريراً + ReportBaseTemplate + BarcodeGenerator + EnvelopePrinter
- **`appsettings.json`**: ملف التهيئة (يبقى في طبقة العرض بقرار DD-09)
- **`DependencyInjection.cs`**: تسجيل خدمات العرض

---

### 4.5 مشاريع الاختبار

| المشروع | الحزم | الغرض |
|---|---|---|
| **MasrLab.Domain.Tests** | xunit 2.5.3, coverlet 6.0.0 | اختبارات وحدة لطبقة النطاق |
| **MasrLab.Application.Tests** | xunit 2.5.3, coverlet 6.0.0 | اختبارات وحدة لطبقة التطبيق |
| **MasrLab.Infrastructure.Tests** | xunit 2.5.3, coverlet 6.0.0 | اختبارات وحدة لطبقة البنية التحتية |

**ملاحظة:** مشاريع الاختبار تحتوي حالياً على `PlaceholderTests.cs` فقط — ستُستبدل ب اختبارات واقعية في مرحلة Business Logic.

---

## 5. أنماط معمارية (Architectural Patterns)

| النمط | التطبيق | المبرر |
|---|---|---|
| **Clean Architecture** | فصل الطبقات (Domain → Application → Infrastructure → Presentation) | استقلالية maximal + سهولة الاستبدال والاختبار |
| **MVVM** | View ↔ ViewModel ↔ Model مع CommunityToolkit.Mvvm | النمط المطلوب لـ WPF + فصل واجهة المستخدم عن المنطق |
| **CQRS** | Commands (كتابة) منفصلة عن Queries (قراءة) عبر MediatR | تطابق مع طبيعة النظام (إدخال بيانات + تقارير) |
| **Repository Pattern** | عقود في Domain + تطبيق في Infrastructure | فصل الوصول للبيانات عن المنطق |
| **Unit of Work** | UnitOfWork.cs | ضمان أن العمليات المتعددة تتم في معاملة واحدة |
| **Feature-per-Module** | مجلد مستقل لكل موديول في ViewModels/Views/Features (قرار DD-05) | الانعكاس الصحيح لسلوك النافذة الرئيسية (استقلال كل موديول) |

---

## 6. قرارات التصميم المعتمدة (Design Decisions)

| # | القرار | الخلاصة |
|---|---|---|
| **DD-01** | اسم مشروع Presentation | الإبقاء على `MasrLab.csproj` — لا إعادة تسمية |
| **DD-02** | SampleDto.cs | الإبقاء على الملف + إضافته رسمياً للخطة |
| **DD-03** | PatientHistoryView | اعتماد طبقتي SQL + C# معاً (keyless entity) |
| **DD-04** | اسم Converter | `BooleanToVisibilityConverter.cs` (بلا اختصار) |
| **DD-05** | تنظيم ViewModels/Views | Feature-per-Module — إلغاء أي تجميع موضوعي |
| **DD-06** | مجلد Migrations | `.gitkeep` فقط — لا Migration الآن |
| **DD-07** | ملفات Styles | 4 ملفات Skeleton فارغة (ResourceDictionary بدون محتوى) |
| **DD-08** | LabIdGenerator / AgeCalculator | LabIdGenerator → Application/Common/Helpers · AgeCalculator → Domain/Common (مؤجل) |
| **DD-09** | appsettings.json | يبقى في Presentation |
| **DD-10** | سياسة تحديث الخطة | تحديث `implementation_plan.md` فوراً مع كل تغيير في الهيكل |

---

## 7. خريطة التبعيات (Dependency Map)

```
MasrLab.Presentation (WPF)
    ├── → MasrLab.Application
    ├── → MasrLab.Infrastructure
    │
MasrLab.Infrastructure (EF Core + Services)
    ├── → MasrLab.Application
    ├── → MasrLab.Domain
    │
MasrLab.Application (Use Cases)
    ├── → MasrLab.Domain
    │
MasrLab.Domain (Entities + Interfaces)
    └── → (لا شيء — الطبقة الأكثر استقلالية)
```

**القاعدة:** التبعية تتبع اتجاه واحد فقط — من الأعلى إلى الأسفل. لا توجد تبعيات دائرية.

---

## 8. إحصائيات المشروع

| البند | العدد |
|---|---|
| **إجمالي الملفات المصدرية** | 424 ملف |
| **إجمالي المجلدات** | 184 مجلد |
| **كيانات Domain** | 30 كيان |
| **تعدادات** | 14 تعداداً |
| **عقود المستودعات** | 9 واجهات |
| **استثناءات مخصصة** | 4 استثناءات |
| **موديولات Features** | 19 موديولاً |
| **DTOs** | 11 كائات |
| **خدمات البنية التحتية** | 6 خدمات |
| **تقارير** | 11 تقريراً |
| **عناصر تحكم مخصصة** | 5 عناصر |
| **محولات** | 5 محولات |
| **مشاريع الاختبار** | 3 مشاريع |
| **الحزم الخارجية** | 16 حزمة |

---

## 9. ملاحظات للمرحلة القادمة (Business Logic)

1. **كتابة Handlers**: كل Handler في `Features/` ينفذ منطق استدعاء واحد أو أكثر من المستودعات.

2. **تسجيل DI**: ملفات `DependencyInjection.cs` الثلاثة فارغة حالياً — ستُملأ عند تهيئة DI.

3. **التحقق من المدخلات**: FluentValidation جاهز — كل Command يحتاج Validator خاص به.

4. **التحويلات**: AutoMapper مُهيأ — `MappingProfile.cs` يحتاج تكوين الأزواج (Entity ↔ DTO).

5. **الاختبارات**: xUnit جاهز — كل اختبار يُكتب في مشروع الاختبار المقابل للطبقة.

6. **قاعدة البيانات**: EF Core مُهيأ — لا توجد Migrations بعد (بقرار DD-06). التمديد يتم بعد اكتمال جميع Configurations.

7. **الطباعة**: `Printing/Reports/` содержит 11 تقريراً — كل تقرير يُكتب بتنسيق `ReportBaseTemplate`.

---

*نهاية التوثيق التقني — Technical Architecture Documentation*
