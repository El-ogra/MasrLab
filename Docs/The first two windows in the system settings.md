# وثيقة تخطيط تنفيذ نافذة "بيانات التحاليل" (TestsMasterDataWindow)

> **نطاق هذه الوثيقة:** تخطيط تنفيذ نافذة "بيانات التحاليل" فقط، والتي تُفتَح من زر "بيانات التحاليل" ضمن أزرار "اعدادات النظام" التسعة في النافذة الرئيسية.
>
> **مرجعية التحليل:** الكود الفعلي في المستودع `El-ogra/MasrLab` — Branch: `niamod` — Commit: `2705600e2b409c4a7bc84597667ab1e1056a4a71` ("المرحلة الأولي في إعدادات النظام"). لم يتم الاعتماد على أي ملف توثيقي؛ مصدر الحقيقة الوحيد هو كود الحل (`.cs` / `.xaml` / `.csproj` / Migrations / DbContext).
>
> **ملاحظة خارج النطاق:** زر "القيم المرجعية" الظاهر في النظام المرجعي سيفتح نافذة منفصلة ("تعديل القيم المرجعية للتحاليل") تُفعَّل في مهمة لاحقة مستقلة. لا يشمل هذا المخطط أي منطق داخلي لها، فقط موضع الزر وسلوكه كفتح نافذة مستقبلية.

---

## أولاً — تحليل الصورة المرجعية (Real Lab System) ومواءمتها مع الكود الفعلي

### 1. بنية النافذة المرجعية

النافذة المرجعية تتكون من أربع مناطق رئيسية:

**أ) شريط البحث العلوي:**
- `Search by test name` — بحث باسم التحليل
- `By group name` — بحث باسم المجموعة
- `By test ID` — بحث برقم التحليل
- عدّاد إجمالي: `Number Of Tests = 489 Tests`

**ب) جدول التحاليل (Grid) — الأعمدة:**
| العمود | الدلالة |
|---|---|
| `ID` | رقم التحليل التسلسلي |
| `Arrang` | رقم الترتيب داخل المجموعة |
| `Group Name` | اسم مجموعة التحليل (URINE EXAMINATION, BLOOD PICTURE, Blood Glucose, LIVER FUNCTIONS, KIDNEY FUNCTIONS, LIPIDS PROFILE...) |
| `Test Name` | اسم التحليل |
| `Pat. P` | سعر المريض (Patient Price) |
| `Lab P` | سعر معمل لمعمل (Lab-to-lab Price) |
| `Out Lab Name` | اسم المعمل الخارجي (عند الإرسال لخارج المعمل، بعضها عربي مثل "أبوغالا") |
| `Out P` | سعر المعمل الخارجي (Outsourced Price / Cost) |
| `Barcode` | نوع العينة/الباركود (Urine, Stool, EDTA Blood, Serum Fasting, Serum Postprandial...) |

**ج) قسم "Test Information" (تفاصيل التحليل المحدد):**
- `Test Name` — اسم التحليل
- `Test Code` — كود التحليل
- `Report Name` — الاسم في التقرير
- `Eill Name` — اسم ثالث (Receipt/Eill name)
- `History Name` — الاسم في السجل التاريخي
- `Arabic Name` — الاسم بالعربية
- `Group Name` — المجموعة (ComboBox)
- `Branch` — الفرع/القسم (ComboBox، مثل MICROBIOLOGY)
- `Log Group` — مجموعة السجل (ComboBox)
- `Collection` — نوع العينة (ComboBox، مثل Urine)
- خانات الاختيار الأربع: `See Report` (يظهر في التقرير)، `Print with other` (يُطبع مع غيره)، `Add with group` (يُضاف مع المجموعة)، `Main test` (تحليل رئيسي)
- `Test time (Day)` — زمن التنفيذ بالأيام
- `Arrange No.` — رقم الترتيب
- `Reference type` — نوع القيمة المرجعية (ComboBox، مثل "By sex and age")
- `Patient price` — سعر المريض
- `Lab to lab price` — سعر معمل لمعمل

**د) قسم "Barcode":**
- `Name` — اسم الباركود/العينة (مثل Urine Examination)
- `Tube 1` / `Tube 2` / `Tube 3` — أنواع الأنابيب الثلاثة المرتبطة بالتحليل (ComboBox لكل منها)

**هـ) قسم "Sent outside Lab":**
- CheckBox لتفعيل الإرسال لمعمل خارجي
- `Lab Name` — اسم المعمل الخارجي (ComboBox)
- `Cost price` — سعر التكلفة

**و) حقل "Patient Question":** سؤال نصي يُوجَّه للمريض عند هذا التحليل (TextBox متعدد الأسطر)

**ز) الأزرار السفلية (سبعة أزرار):**
| الزر | الوظيفة |
|---|---|
| القائمة الرئيسية | إغلاق النافذة والعودة للنافذة الرئيسية |
| القيم المرجعية | فتح نافذة القيم المرجعية (**خارج نطاق هذه المهمة** — تُخطَّط لاحقاً) |
| حذف | حذف التحليل المحدد |
| ترحيل | ترحيل/نقل بيانات التحاليل |
| حفظ | حفظ تحليل جديد |
| تعديل | تعديل التحليل المحدد |
| اضافة تحليل | تجهيز النموذج لإدخال تحليل جديد |

### 2. الوضع الفعلي في الكود (Commit `2705600`)

| العنصر | الوضع الفعلي |
|---|---|
| زر "بيانات التحاليل" | موجود فعلياً في `MainWindow.xaml` ضمن `WrapPanel` الخاص بـ "إعدادات النظام" (تسعة أزرار) ومربوط بـ `ShowTestsMasterDataCommand` في `MainViewModel` |
| آلية الفتح | `MainViewModel.OpenWindow<TWindow, TViewModel>()` يفتح النافذة عبر `ShowDialog()` ويخفي النافذة الرئيسية بالكامل (`mainWindow.Hide()`) ثم يعيد إظهارها عند الإغلاق — وهو السلوك المطلوب تماماً |
| `TestsMasterDataWindow` | موجود في `Views/SystemSettings/TestsMasterDataWindow.xaml` لكنه **Placeholder** يحتوي فقط على نص "لم يتم تنفيذ هذه النافذة حتى الآن" |
| `TestsMasterDataViewModel` | موجود في مكانين: `ViewModels/SystemSettings/TestsMasterDataViewModel.cs` (المستخدم فعلياً) و`ViewModels/TestsMasterData/TestsMasterDataViewModel.cs` (نسخة أخرى) — **كلاهما فارغ تماماً** (`ObservableObject` بلا أي أعضاء) ومسجَّلان في DI |
| الـ Entity `Test` | موجود في `Domain/Entities/Core/Test.cs` ويحتوي فقط على: `Name`, `ReportName`, `ReceiptName`, `Group`, `Barcode`, `Price`, `TurnaroundTime`, `LabToLabFlag`, `Unit` + العلاقات `ReferenceValues` و`Comments` |
| جدول `Tests` | موجود في قاعدة البيانات عبر Migration `20260803173749_InitialCreate` بنفس أعمدة الـ Entity فقط |
| Commands/Queries | موجودة: `AddTestCommand` + Handler + Validator، `UpdateTestCommand` + Handler + Validator، `GetTestWithReferencesQuery` + Handler — لكن **لا يوجد** أمر حذف، ولا استعلام قائمة/بحث التحاليل |
| `TestDto` / `TestWithReferencesDto` | موجودان في `Common/DTOs/` ويطابقان حقول الـ Entity الحالية فقط |
| `ExternalLab` | موجود في `Domain/Entities/Financial/ExternalLab.cs` (`Name`, `Address`, `Phone`, `ContactPerson`) مع `ExternalLabDto` — يصلح مصدراً لقائمة "Lab Name" في قسم Sent outside Lab |
| `GenericRepository<T>` | موجود ويوفر `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete` |
| `ReferenceValueRepository` | موجود بـ `GetByTestIdAsync` |
| DI | `AddApplication()` يسجّل MediatR/AutoMapper/FluentValidation بالتجميع التلقائي من الـ Assembly — أي Command/Query/Validator/Profile جديد يُسجَّل تلقائياً دون تعديل يدوي |

### 3. الفجوة (Gap) بين النظام المرجعي والكود الفعلي

الحقول التالية موجودة في النافذة المرجعية و**غير موجودة** في الـ Entity `Test` الحالي، وتستلزم توسيعه:
- `TestCode` (كود التحليل)
- `HistoryName` (اسم السجل)
- `ArabicName` (الاسم العربي)
- `Branch` (الفرع/القسم)
- `LogGroup` (مجموعة السجل)
- `Collection` / `SampleType` (نوع العينة)
- `SeeReport`, `PrintWithOther`, `AddWithGroup`, `MainTest` (خانات الاختيار الأربع)
- `TestTimeDays` رقمي بدلاً من `TurnaroundTime` النصي الحالي (مع الحفاظ على التوافق)
- `ArrangeNo` (رقم الترتيب)
- `ReferenceType` (نوع القيمة المرجعية)
- `LabToLabPrice` (سعر معمل لمعمل) — إضافة إلى `Price` الحالي (سعر المريض)
- `BarcodeName`, `Tube1`, `Tube2`, `Tube3` (قسم الباركود)
- `SentOutsideLab`, `OutsourcedLabName` (أو `ExternalLabId`), `OutsourcedCostPrice` (قسم الإرسال الخارجي)

---

## الخطوة الرئيسية 1 — طبقة Domain

### الوصف
توسيع الـ Entity `Test` الموجود فعلياً في `src/MasrLab.Domain/Entities/Core/Test.cs` ليستوعب جميع حقول نافذة "بيانات التحاليل" الظاهرة في النظام المرجعي، مع إضافة الـ Enum الخاص بـ `ReferenceType` في `Domain/Common/Enums/` بنفس نمط الـ Enums الموجودة فعلياً (مثل `AgeUnit`, `Gender`). لا تُنشأ كيانات جديدة: المعمل الخارجي ممثَّل فعلياً بـ `ExternalLab` الموجود، والقيم المرجعية ممثَّلة بـ `ReferenceValue` الموجود، وأنواع الأنابيب/العينات تبقى نصوصاً اختيارية مطابقةً لنمط `Sample.SampleType` الموجود فعلياً كنص.

### الخطوات الفرعية
1.1 إنشاء ملف `src/MasrLab.Domain/Common/Enums/ReferenceType.cs` يحتوي `public enum ReferenceType` بالقيم التي تغطي خيارات النظام المرجعي: `General` (قيمة عامة ثابتة)، `BySex` (حسب النوع)، `ByAge` (حسب السن)، `BySexAndAge` (حسب النوع والسن — الخيار الظاهر في الصورة).
1.2 تعديل `src/MasrLab.Domain/Entities/Core/Test.cs` بإضافة الـ Properties التالية، مع الحفاظ التام على الـ Properties الحالية وعدم إعادة تسمية أي منها:
- `public string? TestCode { get; set; }`
- `public string? HistoryName { get; set; }`
- `public string? ArabicName { get; set; }`
- `public string? Branch { get; set; }`
- `public string? LogGroup { get; set; }`
- `public string? SampleType { get; set; }` (يقابل حقل Collection في الواجهة)
- `public bool SeeReport { get; set; }`
- `public bool PrintWithOther { get; set; }`
- `public bool AddWithGroup { get; set; }`
- `public bool IsMainTest { get; set; }`
- `public int TestTimeDays { get; set; }` (يقابل "Test time (Day)")
- `public int ArrangeNo { get; set; }`
- `public ReferenceType ReferenceType { get; set; }`
- `public decimal? LabToLabPrice { get; set; }`
- `public string? BarcodeName { get; set; }`
- `public string? Tube1 { get; set; }`
- `public string? Tube2 { get; set; }`
- `public string? Tube3 { get; set; }`
- `public bool SentOutsideLab { get; set; }`
- `public string? OutsourcedLabName { get; set; }`
- `public decimal? OutsourcedCostPrice { get; set; }`
- `public string? PatientQuestion { get; set; }`
1.3 الحفاظ على `TurnaroundTime` الحالي كما هو دون حذف (مستخدم فعلياً في `TestDto` و`AddTestCommand` و`UpdateTestCommand` و`VisitTest`-related flows)، مع اعتبار `TestTimeDays` هو الحقل التشغيلي الجديد للنافذة.
1.4 التحقق من أن `Test` يرث `BaseEntity` (الموجود فعلياً) وبالتالي يكتسب `Id`, `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `IsDeleted` تلقائياً — لا حاجة لأي تعديل في هذا الشأن.

### Migration قاعدة البيانات
لا يلزم migration في هذه الخطوة — التعديل هنا على مستوى كود الـ Domain فقط، والـ Migration يُنشأ في الخطوة الرئيسية التالية (طبقة Infrastructure) بعد اكتمال الـ Configuration.

### نقاط التحقق
- [ ] ملف `ReferenceType.cs` موجود تحت `Domain/Common/Enums/` ويُبنى دون أخطاء.
- [ ] `Test.cs` يحتوي جميع الـ Properties الجديدة الـ 21 المذكورة أعلاه مع الاحتفاظ الكامل بالـ Properties العشرة الأصلية.
- [ ] مشروع `MasrLab.Domain` يُبنى (`dotnet build`) دون أي خطأ أو تحذير كسر مرجعي.
- [ ] لم يتم تعديل أي Entity آخر غير `Test`، ولم تُقرأ أو تُعدَّل أي ملفات داخل مجلد `Docs`.

---

## الخطوة الرئيسية 2 — طبقة Infrastructure (Persistence)

### الوصف
تحديث `TestConfiguration` الموجود فعلياً في `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestConfiguration.cs` ليشمل الأعمدة الجديدة بنفس نمط القيود المطبَّق (MaxLength / IsRequired / decimal(18,2) / HasIndex)، ثم إنشاء Migration جديد يضيف هذه الأعمدة إلى جدول `Tests` الموجود فعلياً منذ Migration `20260803173749_InitialCreate`. الـ `MasrLabDbContext` لا يحتاج أي تعديل لأن `DbSet<Test> Tests` موجود فعلياً، والتكوينات تُطبَّق تلقائياً عبر `ApplyConfigurationsFromAssembly`.

### الخطوات الفرعية
2.1 تعديل `TestConfiguration.Configure` بإضافة تكوينات الأعمدة الجديدة:
- `TestCode`, `BarcodeName`, `Tube1`, `Tube2`, `Tube3`: `HasMaxLength(200)` اختيارية (nullable).
- `HistoryName`, `ArabicName`, `Branch`, `LogGroup`, `SampleType`, `OutsourcedLabName`: `HasMaxLength(200)` اختيارية.
- `SeeReport`, `PrintWithOther`, `AddWithGroup`, `IsMainTest`, `SentOutsideLab`: `IsRequired()` (bit غير nullable).
- `TestTimeDays`, `ArrangeNo`: `IsRequired()` (int غير nullable).
- `ReferenceType`: `IsRequired()` مع التخزين كـ int (النمط الافتراضي المطبَّق فعلياً على باقي الـ Enums في الجداول الحالية مثل `ReferenceValues.Gender` و`ReferenceValues.AgeUnit` المخزّنة `int`).
- `LabToLabPrice`, `OutsourcedCostPrice`: `HasColumnType("decimal(18,2)")` اختيارية (nullable) — نفس نمط `Price` الحالي.
- `PatientQuestion`: `HasMaxLength(2000)` اختياري (نفس نمط `Comment.CommentText` ذي الطول 2000).
2.2 إضافة `builder.HasIndex(e => e.ArrangeNo);` لأن الجدول مرتَّب عرضياً بهذا العمود، مع الحفاظ على الـ Indexes الحالية (`Group`, `Barcode`, `IsDeleted`) دون تعديل.
2.3 التأكد من عدم الحاجة لأي تعديل في `MasrLabDbContext.cs` (الـ `DbSet<Test>` موجود، و`OnModelCreating` يلتقط التكوين الجديد تلقائياً من الـ Assembly).
2.4 إنشاء Migration جديد باسم مقترح `ExtendTestEntityWithMasterDataFields` عبر `dotnet ef migrations add ExtendTestEntityWithMasterDataFields` باستخدام `MasrLabDbContextFactory` الموجود فعلياً كـ design-time factory.
2.5 مراجعة ملف الـ Migration المُولَّد للتأكد من أنه يحتوي فقط على `AddColumn` للأعمدة الجديدة على جدول `Tests`، ولا يمس أي جدول آخر، وأن `MasrLabDbContextModelSnapshot.cs` تحدَّث تلقائياً بما يطابق الـ Entity الموسَّع.

### Migration قاعدة البيانات
**الاسم المقترح:** `ExtendTestEntityWithMasterDataFields`
**النطاق:** جدول `Tests` فقط — إضافة الأعمدة: `TestCode` (nvarchar(200), null)، `HistoryName` (nvarchar(200), null)، `ArabicName` (nvarchar(200), null)، `Branch` (nvarchar(200), null)، `LogGroup` (nvarchar(200), null)، `SampleType` (nvarchar(200), null)، `SeeReport` (bit, not null, default 0)، `PrintWithOther` (bit, not null, default 0)، `AddWithGroup` (bit, not null, default 0)، `IsMainTest` (bit, not null, default 0)، `TestTimeDays` (int, not null, default 0)، `ArrangeNo` (int, not null, default 0)، `ReferenceType` (int, not null, default 0)، `LabToLabPrice` (decimal(18,2), null)، `BarcodeName` (nvarchar(200), null)، `Tube1` (nvarchar(200), null)، `Tube2` (nvarchar(200), null)، `Tube3` (nvarchar(200), null)، `SentOutsideLab` (bit, not null, default 0)، `OutsourcedLabName` (nvarchar(200), null)، `OutsourcedCostPrice` (decimal(18,2), null)، `PatientQuestion` (nvarchar(2000), null) + Index جديد على `ArrangeNo`.

### نقاط التحقق
- [ ] `TestConfiguration.cs` يغطي جميع الأعمدة الجديدة بنفس أنماط القيود المستخدمة في الملف نفسه.
- [ ] الـ Migration المُولَّد يضيف أعمدة على `Tests` فقط ولا يحتوي أي `DropColumn` أو `DropTable`.
- [ ] `MasrLabDbContextModelSnapshot` يعكس الـ Entity الموسَّع بالكامل بعد التوليد.
- [ ] `dotnet ef database update` يُطبَّق بنجاح على قاعدة بيانات تحتوي البيانات الموجودة دون فقدان أي سجل في `Tests`.
- [ ] `MasrLabDbContext.cs` لم يتغيّر إطلاقاً في هذه الخطوة.

---

## الخطوة الرئيسية 3 — طبقة Application

### الوصف
توسيع الـ DTOs والـ Commands الموجودة فعلياً في `Features/TestsMasterData/`، وإضافة العمليات الناقصة التي تتطلبها أزرار النافذة: حذف تحليل (`DeleteTestCommand`) واستعلام قائمة التحاليل مع البحث الثلاثي (`GetTestsListQuery`). جميع العمليات تتبع النمط المطبَّق فعلياً: MediatR `IRequest`/`IRequestHandler` + FluentValidation `AbstractValidator` + اعتماد على `IRepository<T>` و`IUnitOfWork`، والتسجيل في DI يتم تلقائياً عبر `AddApplication()` (تجميع MediatR/AutoMapper/FluentValidation من الـ Assembly) دون أي تعديل يدوي في `DependencyInjection.cs`. كما تُضاف خريطة `Test → TestDto` إلى `TestsMasterDataMappingProfile` الموجود فعلياً (يحتوي حالياً فقط `ReferenceValue → ReferenceValueDto` و`PriceListItem → PriceListItemDto`).

### الخطوات الفرعية
3.1 توسيع `src/MasrLab.Application/Common/DTOs/TestDto.cs` بإضافة الـ Properties الجديدة الـ 21 المطابقة لتوسعة الـ Entity (بنفس الأسماء: `TestCode`, `HistoryName`, `ArabicName`, `Branch`, `LogGroup`, `SampleType`, `SeeReport`, `PrintWithOther`, `AddWithGroup`, `IsMainTest`, `TestTimeDays`, `ArrangeNo`, `ReferenceType`, `LabToLabPrice`, `BarcodeName`, `Tube1`, `Tube2`, `Tube3`, `SentOutsideLab`, `OutsourcedLabName`, `OutsourcedCostPrice`, `PatientQuestion`) مع الحفاظ على الحقول الحالية.
3.2 توسيع `src/MasrLab.Application/Common/DTOs/TestWithReferencesDto.cs` بنفس الحقول الجديدة، لأنه يُجمَّع يدوياً في `GetTestWithReferencesQueryHandler`.
3.3 إضافة `CreateMap<Test, TestDto>();` داخل `TestsMasterDataMappingProfile` الموجود في `Common/Mappings/Profiles/TestsMasterDataMappingProfile.cs`.
3.4 توسيع `AddTestCommand` و`AddTestCommandHandler` و`AddTestCommandValidator` في `Features/TestsMasterData/Commands/AddTest/` لتشمل جميع الحقول الجديدة: تمريرها في الـ record، وتعبئتها على الـ Entity في الـ Handler، وإضافة قواعد Validation مثل: `TestTimeDays >= 0`، `ArrangeNo >= 0`، `LabToLabPrice >= 0` عند وجوده، `OutsourcedCostPrice >= 0` عند تفعيل `SentOutsideLab`، و`OutsourcedLabName` غير فارغ عند `SentOutsideLab == true`.
3.5 توسيع `UpdateTestCommand` و`UpdateTestCommandHandler` و`UpdateTestCommandValidator` في `Features/TestsMasterData/Commands/UpdateTest/` بنفس الحقول والقواعد، مع الحفاظ على نمط الـ Handler الحالي (جلب عبر `GetByIdAsync` ثم `EntityNotFoundException` عند الغياب ثم تحديث الحقول ثم `Update` + `SaveChangesAsync`).
3.6 إنشاء `Features/TestsMasterData/Commands/DeleteTest/DeleteTestCommand.cs` كـ `record DeleteTestCommand(int Id) : IRequest<Unit>`، و`DeleteTestCommandHandler` يجلب الـ Test عبر `IRepository<Test>.GetByIdAsync`، يرمي `EntityNotFoundException` عند الغياب، ثم يستدعي `Delete(test)` (الحذف الفعلي Soft Delete عبر `SoftDeleteInterceptor` المسجَّل فعلياً في Infrastructure DI مع `IsDeleted` وQuery Filter المطبَّق في `OnModelCreating`)، ثم `SaveChangesAsync`، مع `DeleteTestCommandValidator` يتحقق `Id > 0`.
3.7 إنشاء `Features/TestsMasterData/Queries/GetTestsList/GetTestsListQuery.cs` كـ `record GetTestsListQuery(string? NameFilter, string? GroupFilter, int? IdFilter) : IRequest<IReadOnlyList<TestDto>>` لتغذية الـ Grid وحقول البحث الثلاثة (Search by test name / By group name / By test ID)، و`GetTestsListQueryHandler` يجلب عبر `IRepository<Test>.GetAllAsync`، يطبّق الفلاتر الثلاثة (Contains غير حساس لحالة الأحرف على `Name`، مساواة/Contains على `Group`، مساواة على `Id`)، يرتّب النتيجة بـ `Group` ثم `ArrangeNo` (مطابق لترتيب الأعمدة في النظام المرجعي)، ويعيد `_mapper.Map<List<TestDto>>(...)`.
3.8 تحديث `GetTestWithReferencesQueryHandler` الموجود ليعبّئ الحقول الجديدة في `TestWithReferencesDto` عند التجميع اليدوي (الكائن الخارجي يُبنى يدوياً حالياً بلا AutoMapper، فيجب إضافة الأسطر المقابلة).
3.9 التأكد من عدم الحاجة لأي تعديل في `MasrLab.Application/DependencyInjection.cs` — كل ما سبق يُلتقط تلقائياً عبر `RegisterServicesFromAssembly` و`AddValidatorsFromAssembly` و`AddAutoMapper`.

### Migration قاعدة البيانات
لا يلزم migration في هذه الخطوة.

### نقاط التحقق
- [ ] `TestDto` و`TestWithReferencesDto` يحتويان جميع الحقول الجديدة بنفس أسماء Properties الـ Entity.
- [ ] `AddTestCommand` و`UpdateTestCommand` يمرّران جميع الحقول الجديدة، والـ Handlers تعبّئها فعلياً على الـ Entity، والـ Validators تتضمن القواعد المذكورة في 3.4.
- [ ] `DeleteTestCommand` + Handler + Validator موجودون وينفّذون حذفاً ناعماً (السجل يبقى في القاعدة مع `IsDeleted = true` ويختفي من الاستعلامات بفعل الـ Query Filter).
- [ ] `GetTestsListQuery` يعيد قائمة مرتبة بـ `Group` ثم `ArrangeNo`، والفلاتر الثلاثة تعمل كلٌّ على حدة ومجتمعة.
- [ ] `TestsMasterDataMappingProfile` يتضمن `CreateMap<Test, TestDto>()`.
- [ ] لم يُعدَّل `DependencyInjection.cs` في طبقة Application، ومشروع `MasrLab.Application` يُبنى دون أخطاء.

---

## الخطوة الرئيسية 4 — طبقة Presentation (ViewModel)

### الوصف
تنفيذ منطق `TestsMasterDataViewModel` الفارغ حالياً في `src/MasrLab.Presentation/ViewModels/SystemSettings/TestsMasterDataViewModel.cs` (وهو المسجَّل في DI والمستخدم فعلياً من `MainViewModel.ShowTestsMasterData`)، باتباع نمط CommunityToolkit.Mvvm المطبَّق فعلياً في المشروع (`ObservableObject` + `[ObservableProperty]` + `[RelayCommand]` + حقن `ISender` الخاص بـ MediatR). ملاحظة: توجد نسخة ثانية فارغة في `ViewModels/TestsMasterData/TestsMasterDataViewModel.cs` مسجَّلة أيضاً في DI ولا يستخدمها شيء — تُترك كما هي خارج النطاق، والعمل يكون على نسخة `SystemSettings` فقط.

### الخطوات الفرعية
4.1 تعديل `TestsMasterDataViewModel` (نسخة `SystemSettings`) ليستقبل `ISender` (MediatR) عبر الـ Constructor — نفس نمط الحقن المطبَّق في المشروع حيث الـ ViewModels تُسجَّل `AddTransient` في `AddPresentation` ويُحقن الـ `IServiceProvider`/الخدمات عبر الـ Constructor.
4.2 إضافة خصائص حالة النافذة بـ `[ObservableProperty]`:
- `ObservableCollection<TestDto> Tests` — مصدر الـ Grid.
- `TestDto? SelectedTest` — السطر المحدد في الـ Grid؛ عند تغيّره تُنسخ قيمه إلى حقول النموذج السفلي.
- `int TotalTestsCount` — عدّاد "Number Of Tests".
- `string? SearchByName`, `string? SearchByGroup`, `int? SearchById` — حقول البحث الثلاثة العلوية.
- حقول النموذج السفلي كاملة كخصائص منفصلة قابلة للربط: `TestName`, `TestCode`, `ReportName`, `EillName`, `HistoryName`, `ArabicName`, `GroupName`, `Branch`, `LogGroup`, `Collection`, `SeeReport`, `PrintWithOther`, `AddWithGroup`, `IsMainTest`, `TestTimeDays`, `ArrangeNo`, `ReferenceType` (النوع `ReferenceType` من Domain)، `PatientPrice`, `LabToLabPrice`, `BarcodeName`, `Tube1`, `Tube2`, `Tube3`, `SentOutsideLab`, `OutsourcedLabName`, `OutsourcedCostPrice`, `PatientQuestion`.
- `ObservableCollection<string> GroupNames` — قيم `Group` المميزة من قائمة التحاليل المحمَّلة (تغذية ComboBox "Group Name").
- `ObservableCollection<string> ExternalLabNames` — أسماء المعامل الخارجية من كيان `ExternalLab` الموجود فعلياً (تغذية ComboBox "Lab Name" في قسم Sent outside Lab)، تُحمَّل عبر استعلام بسيط على `IRepository<ExternalLab>` أو Query جديد ضمن النمط نفسه.
- `IReadOnlyList<ReferenceType> ReferenceTypes` — قيم Enum `ReferenceType` الأربع لتغذية ComboBox "Reference type" (منها "By sex and age").
- `bool IsEditing` — للتمييز بين وضع الإضافة ووضع التعديل (يوجّه استدعاء `AddTestCommand` أم `UpdateTestCommand`).
4.3 إضافة الـ Commands التالية بـ `[RelayCommand]`:
- `LoadTestsCommand` — يُستدعى عند فتح النافذة: يرسل `GetTestsListQuery(null, null, null)`، يملأ `Tests` و`TotalTestsCount` و`GroupNames`.
- `SearchCommand` — يرسل `GetTestsListQuery(SearchByName, SearchByGroup, SearchById)` ويحدّث `Tests` و`TotalTestsCount`.
- `AddNewTestCommand` (زر "اضافة تحليل") — يفرّغ جميع حقول النموذج، يضبط `IsEditing = false`، وينقل التركيز لحقل `Test Name`.
- `SaveTestCommand` (زر "حفظ") — يبني `AddTestCommand` من حقول النموذج (مع تحويل `TestTimeDays` إلى `TurnaroundTime` نصياً للتوافق مع الـ Command الحالي، أو تمرير القيمة الرقمية في الحقل الجديد `TestTimeDays`)، يرسله عبر `ISender`، ثم يعيد تحميل القائمة عبر `LoadTestsCommand`.
- `UpdateTestCommand` (زر "تعديل") — مُفعَّل فقط عند وجود `SelectedTest`؛ يبني `UpdateTestCommand` بـ `Id` التحليل المحدد وجميع الحقول، يرسله، ثم يعيد التحميل. يُربط بـ `CanExecute` مبني على `SelectedTest is not null`.
- `DeleteTestCommand` (زر "حذف") — مُفعَّل فقط عند وجود `SelectedTest`؛ يرسل `DeleteTestCommand(SelectedTest.Id)` ثم يعيد التحميل ويفرّغ النموذج. يُربط بـ `CanExecute` مبني على `SelectedTest is not null`.
- `TransferCommand` (زر "ترحيل") — ينفّذ وظيفة ترحيل بيانات التحاليل كما في النظام المرجعي (إعادة ترقيم `ArrangeNo` تسلسلياً داخل كل مجموعة وحفظها دفعة واحدة عبر `UpdateTestCommand` لكل سجل متأثر — لأن الـ Handler الحالي يدعم التحديث الفردي فقط ولا يوجد نمط Batch موجود فعلياً في الكود).
- `OpenReferenceValuesCommand` (زر "القيم المرجعية") — Placeholder فقط: لا ينفّذ أي منطق؛ يُذكر تعليقياً أنه سيفتح نافذة "تعديل القيم المرجعية" في مهمة لاحقة مستقلة (خارج نطاق هذه المهمة).
- `CloseCommand` (زر "القائمة الرئيسية") — يغلق النافذة؛ عندها يعيد `MainViewModel.OpenWindow` الموجود فعلياً إظهار النافذة الرئيسية تلقائياً عبر `mainWindow.Show()` في كتلة `finally` — لا حاجة لأي منطق إضافي لإظهار الرئيسية.
4.4 عند تغيّر `SelectedTest` (عبر `OnSelectedTestChanged` الجزئي المولَّد من CommunityToolkit) تُنسخ جميع خصائص الـ DTO إلى حقول النموذج، ويُضبط `IsEditing = true`.
4.5 التحقق من أن التسجيل في DI قائم فعلياً: `services.AddTransient<MasrLab.Presentation.ViewModels.SystemSettings.TestsMasterDataViewModel>()` موجود في `AddPresentation` — لا تعديل مطلوب في `DependencyInjection.cs` الخاص بـ Presentation.

### Migration قاعدة البيانات
لا يلزم migration في هذه الخطوة.

### نقاط التحقق
- [ ] `TestsMasterDataViewModel` (نسخة `SystemSettings`) يحتوي جميع الخصائص والـ Commands المذكورة، ويُحقن `ISender` عبر الـ Constructor.
- [ ] زر "حفظ" في وضع الإضافة يرسل `AddTestCommand`، وزر "تعديل" في وضع التحديد يرسل `UpdateTestCommand` بالـ `Id` الصحيح.
- [ ] زر "حذف" معطَّل بلا تحديد، ومفعَّل بعد تحديد سطر، وبعد الحذف يختفي السطر من الـ Grid (Soft Delete).
- [ ] عدّاد `TotalTestsCount` يعكس عدد السجلات المعروضة بعد كل تحميل أو بحث.
- [ ] زر "القيم المرجعية" موجود كأمر Placeholder فقط بلا أي منطق داخلي.
- [ ] زر "القائمة الرئيسية" يغلق النافذة، والنافذة الرئيسية تظهر تلقائياً (السلوك مضمون من `OpenWindow` الموجود فعلياً).

---

## الخطوة الرئيسية 5 — طبقة Presentation (View / XAML)

### الوصف
استبدال محتوى `TestsMasterDataWindow.xaml` الحالي (Placeholder بنص "لم يتم تنفيذ هذه النافذة حتى الآن") بالتخطيط الكامل لنافذة "بيانات التحاليل" مطابقاً للنظام المرجعي، مع الالتزام بإعدادات النافذة الموجودة فعلياً: `FlowDirection="RightToLeft"` و`Language="ar-EG"` و`WindowStartupLocation="CenterScreen"`، وبالربط الكامل مع `TestsMasterDataViewModel` عبر Data Binding. الـ `MainWindow.xaml` و`MainViewModel` لا يحتاجان أي تعديل: الزر "بيانات التحاليل" موجود فعلياً ومربوط بـ `ShowTestsMasterDataCommand` الذي يفتح `TestsMasterDataWindow` ويخفي النافذة الرئيسية بالكامل أثناء العرض.

### الخطوات الفرعية
5.1 تعديل `src/MasrLab.Presentation/Views/SystemSettings/TestsMasterDataWindow.xaml` ببناء التخطيط العام بـ `Grid` من أربعة صفوف: شريط البحث (Auto)، جدول التحاليل (*)، قسم التفاصيل السفلي (Auto)، شريط الأزرار السفلي (Auto)، مع الحفاظ على `Title="MasrLab - بيانات التحاليل"` وتوسيع الأبعاد (مقترح `Width="1200" Height="800"`) لاستيعاب التخطيط الكامل.
5.2 بناء شريط البحث العلوي: ثلاثة `TextBox` مربوطة بـ `SearchByName` / `SearchByGroup` / `SearchById` مع عناوين "Search by test name" و"By group name" و"By test ID"، وزر بحث مربوط بـ `SearchCommand`، و`TextBlock` للعدّاد مربوط بـ `TotalTestsCount` بصيغة "Number Of Tests = {…}".
5.3 بناء `DataGrid` التحاليل مربوطاً بـ `Tests` (`ItemsSource="{Binding Tests}"`، `SelectedItem="{Binding SelectedTest}"`) بالأعمدة التسعة المطابقة للنظام المرجعي: `Id` (ID)، `ArrangeNo` (Arrang)، `Group` (Group Name)، `Name` (Test Name)، `Price` (Pat. P)، `LabToLabPrice` (Lab P)، `OutsourcedLabName` (Out Lab Name)، `OutsourcedCostPrice` (Out P)، `Barcode` (Barcode) — جميعها `DataGridTextColumn` للقراءة، مع سكرول أفقي ورأس أعمدة ثابت.
5.4 بناء قسم "Test Information": شبكة حقول تحتوي `TextBox` لكلٍّ من `TestName` (مع عنوان "Test Name")، `TestCode` ("Test Code")، `ReportName` ("Report Name")، `EillName` ("Eill Name")، `HistoryName` ("History Name")، `ArabicName` ("Arabic Name")، و`ComboBox` لكلٍّ من `GroupName` (مصدره `GroupNames`)، `Branch`، `LogGroup`، `Collection`، مربوطة بـ `SelectedItem`/`Text` مع الخصائص المقابلة في الـ ViewModel.
5.5 إضافة خانات الاختيار الأربع `CheckBox`: "See Report" ← `SeeReport`، "Print with other" ← `PrintWithOther`، "Add with group" ← `AddWithGroup`، "Main test" ← `IsMainTest`.
5.6 إضافة حقول: `TextBox` رقمي لـ "Test time (Day)" ← `TestTimeDays`، `TextBox` رقمي لـ "Arrange No." ← `ArrangeNo`، `ComboBox` لـ "Reference type" ← `ReferenceType` بعناصره `ReferenceTypes` (تظهر قيمة "By sex and age" كما في النظام المرجعي)، `TextBox` رقمي لـ "Patient price" ← `PatientPrice`، `TextBox` رقمي لـ "Lab to lab price" ← `LabToLabPrice`.
5.7 بناء قسم "Barcode": `TextBox` لـ "Name" ← `BarcodeName`، وثلاثة `ComboBox` لـ "Tube 1" / "Tube 2" / "Tube 3" ← `Tube1` / `Tube2` / `Tube3`.
5.8 بناء قسم "Sent outside Lab": `CheckBox` ← `SentOutsideLab`، و`ComboBox` لـ "Lab Name" ← `OutsourcedLabName` بمصدر `ExternalLabNames`، و`TextBox` رقمي لـ "Cost price" ← `OutsourcedCostPrice`، مع تعطيل الحقلين الأخيرين عندما يكون الـ CheckBox غير مفعَّل (ربط `IsEnabled` بـ `SentOutsideLab`).
5.9 إضافة `TextBox` متعدد الأسطر (`AcceptsReturn="True"`) لـ "Patient Question" ← `PatientQuestion`.
5.10 بناء شريط الأزرار السفلي بالأزرار السبعة بنفس ترتيب النظام المرجعي (من اليمين لليسار بفعل `FlowDirection="RightToLeft"`): "القائمة الرئيسية" ← `CloseCommand`، "القيم المرجعية" ← `OpenReferenceValuesCommand` (Placeholder — سيفتح نافذة منفصلة في مهمة لاحقة)، "حذف" ← `DeleteCommand`، "ترحيل" ← `TransferCommand`، "حفظ" ← `SaveTestCommand`، "تعديل" ← `UpdateTestCommand`، "اضافة تحليل" ← `AddNewTestCommand`.
5.11 ترك `TestsMasterDataWindow.xaml.cs` كما هو (Constructor + `InitializeComponent()` فقط — نفس نمط باقي النوافذ في المشروع مثل `TestGroupsWindow.xaml.cs`) دون أي منطق في الـ Code-Behind.
5.12 التحقق النهائي من سلسلة الفتح الكاملة دون أي تعديل: زر "اعدادات النظام" (`SelectSystemCommand`) → الأزرار التسعة في `WrapPanel` → زر "بيانات التحاليل" (`ShowTestsMasterDataCommand`) → `OpenWindow<TestsMasterDataWindow, TestsMasterDataViewModel>()` → إخفاء `MainWindow` بالكامل (`mainWindow.Hide()`) → عرض النافذة كـ `ShowDialog()` → عند الإغلاق تُعاد الرئيسية تلقائياً (`mainWindow.Show()`).

### Migration قاعدة البيانات
لا يلزم migration في هذه الخطوة.

### نقاط التحقق
- [ ] النافذة تفتح من زر "بيانات التحاليل" وتختفي النافذة الرئيسية بالكامل أثناء عرضها، وتعود الرئيسية عند الإغلاق — دون أي تعديل في `MainWindow.xaml` أو `MainViewModel.cs`.
- [ ] الـ `DataGrid` يعرض الأعمدة التسعة بالترتيب المطابق للنظام المرجعي، والبحث الثلاثي (بالاسم/بالمجموعة/بالرقم) يعمل ويحدّث العدّاد.
- [ ] تحديد سطر في الـ Grid يملأ جميع حقول "Test Information" وقسمي "Barcode" و"Sent outside Lab" و"Patient Question" بالقيم الصحيحة.
- [ ] زر "اضافة تحليل" يفرّغ النموذج، وزر "حفظ" يضيف تحليلاً جديداً يظهر فوراً في الـ Grid، وزر "تعديل" يحدّث التحليل المحدد، وزر "حذف" يزيله من العرض.
- [ ] حقلا "Lab Name" و"Cost price" معطَّلان ما لم يكن "Sent outside Lab" مفعَّلاً.
- [ ] زر "القيم المرجعية" موجود بصرياً ومعرَّف كأمر Placeholder فقط دون أي سلوك داخلي.
- [ ] `TestsMasterDataWindow.xaml.cs` خالٍ من أي منطق (InitializeComponent فقط)، ومشروع `MasrLab.Presentation` (WPF) يُبنى دون أخطاء XAML.
