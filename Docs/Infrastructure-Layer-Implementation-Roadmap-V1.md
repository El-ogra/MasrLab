# خارطة طريق تنفيذ طبقة Infrastructure — مشروع MasrLab

> **الكوميت المُدقَّق عليه:** `2a49bf54ac5fbffc18f8f4b9c15676acbbd7eb93`
> **الفرع:** `niamod` — **رسالة الكوميت:** «البداية في Infrastructure»
> **تاريخ التدقيق:** 2026-08-10 — **المنهجية:** تدقيق مستقل مباشر على الكود المصدري عند الكوميت أعلاه فقط.

---

## أولاً: الملخص التنفيذي

عند الكوميت `2a49bf5` دخلت طبقة Infrastructure مرحلة التنفيذ الفعلي، وحُسمت معظم نقاط التأسيس الحرجة:

- **آلية الترحيلات سليمة:** نقطة بدء التطبيق تستدعي `MigrateAsync()` (`src/MasrLab.Presentation/App.xaml.cs:38`)، أي أن تطبيق الترحيلات يتم عبر المسار الإنتاجي الصحيح ويُنشئ سجل `__EFMigrationsHistory`.
- **المصادقة منفَّذة فعليًا:** `AuthenticationService` يتحقق من كلمات المرور عبر BCrypt (`src/MasrLab.Infrastructure/Services/AuthenticationService.cs:24`)، ولا يوجد أي حساب افتراضي بكلمة مرور ثابتة؛ استُبدل بشاشة إعداد أول تشغيل (`FirstRunSetupWindow`).
- **الحزم حُسمت وثُبِّتت:** BCrypt.Net-Next 4.2.0 وZXing.Net 0.16.11 وQuestPDF 2026.7.2 (`src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj:15,28,29`).
- **ثلاث خدمات ما زالت غير منفَّذة:** `PrintService` و`BackupService` و`BarcodeService` ترمي `NotImplementedException`.
- **الحاجب الوحيد المتبقي:** `DefaultConnection` فارغة في `appsettings.json`، والملف المحلي البديل غير مُتتبَّع في Git — لا يمكن تشغيل التطبيق أو اختبارات التكامل بدون إعداد بيئة محلية.

**الخلاصة:** يمكن البدء الآن فورًا في تنفيذ الخدمات الثلاث المتبقية (كود فقط، غير معتمد على قاعدة بيانات)، بالتوازي مع تجهيز بيئة قاعدة البيانات المحلية التي تحجب التشغيل والتحقق التكاملي.

---

## ثانيًا: الحقائق المؤكَّدة بالأدلة

### 1. الترحيلات (Migrations)

يوجد **6 ترحيلات** في `src/MasrLab.Infrastructure/Persistence/Migrations/`:

| # | الترحيل | الدليل |
|---|---------|--------|
| 1 | `20260803173749_InitialCreate` | `Migrations/20260803173749_InitialCreate.cs` |
| 2 | `20260804140414_AddPatientHistoryView` | `Migrations/20260804140414_AddPatientHistoryView.cs:13-68` (SQL خام لإنشاء الـ View) |
| 3 | `20260804211139_AddRequestAuditLog` | `Migrations/20260804211139_AddRequestAuditLog.cs` |
| 4 | `20260805033329_AddUniquePatientLabIdIndex` | `Migrations/20260805033329_AddUniquePatientLabIdIndex.cs` |
| 5 | `20260808223709_AddOverrideReasonToTestResult` | `Migrations/20260808223709_AddOverrideReasonToTestResult.cs` |
| 6 | `20260810021405_AddComparisonFlagToPatientHistoryView` | `Migrations/20260810021405_AddComparisonFlagToPatientHistoryView.cs` |

الترحيل السادس (الأحدث) ينفّذ أربعة تغييرات دفعة واحدة:
- إعادة تسمية عمود `Accounts.NetProfit` إلى `NetActivityAfterCommission` (`AddComparisonFlagToPatientHistoryView.cs:17-20`).
- إضافة عمود `PriceLists.IsDefault` مع فهرس فريد مُرشَّح `[IsDefault] = 1` (`AddComparisonFlagToPatientHistoryView.cs:22-34`).
- تحويل فهرس `IX_PatientVisits_LabId` إلى فريد (`AddComparisonFlagToPatientHistoryView.cs:13-15, 36-40`).
- إعادة إنشاء `PatientHistoryView` بعمود `ComparisonFlag` محسوبًا داخل SQL (`AddComparisonFlagToPatientHistoryView.cs:42-105`).

**اتساق اللقطة (ModelSnapshot):** اللقطة `MasrLabDbContextModelSnapshot.cs` (2334 سطرًا) متزامنة تمامًا مع الترحيل السادس — تحتوي على `OverrideReason` (سطر 991)، و`NetActivityAfterCommission` (سطر 1302)، و`IsDefault` مع الفهرس المُرشَّح (سطور 1681، 1700-1702)، و`ComparisonFlag` (سطر 1936). لا توجد فجوة بين اللقطة والترحيلات.

**حالة PatientHistoryView:** الفجوة التصميمية في الخاصية `ComparisonFlag` عولجت بالكامل: الكيان `PatientHistoryView.cs:20` يعرّفها، والترحيل السادس يضيفها فعليًا إلى الـ SQL View (`AddComparisonFlagToPatientHistoryView.cs:93-99`). ملاحظة جانبية: الملف `Persistence/Views/PatientHistoryView.sql` ما زال **Placeholder** بلا منطق (`PatientHistoryView.sql:9,43-45`) بينما التعريف الفعلي يعيش داخل الترحيلات — الملف إما أن يُحدَّث أو يُحذف لئلا يضلّل القارئ.

### 2. آلية تهيئة قاعدة البيانات

نقطة بدء التطبيق تستخدم `MigrateAsync()`:

```csharp
await context.Database.MigrateAsync();   // App.xaml.cs:38
```

ثم تستدعي الـ Seeders وتحدد مسار أول تشغيل (`App.xaml.cs:39-63`). **لا يوجد أي استدعاء لـ `EnsureCreatedAsync` في مسار التشغيل**؛ الاستخدام الوحيد لـ `EnsureCreated` هو في مساعد الاختبارات كمسار سريع موثَّق (`tests/MasrLab.Infrastructure.Tests/LocalDbTestInfrastructure.cs:79-84`)، وهذا مقبول لأنه مخصص لاختبارات الاستعلام لا للمسار الإنتاجي.

### 3. Connection String

- `src/MasrLab.Presentation/appsettings.json:3` — `"DefaultConnection": ""` (**فارغة**).
- `appsettings.Local.json` **غير مُتتبَّع في Git** إطلاقًا (مُستثنى في `.gitignore:9`)، لكن المشروع ينسخه إلى مجلد الإخراج إن وُجد (`src/MasrLab.Presentation/MasrLab.csproj:29-31`)، و`App.xaml.cs:24` يحمّله كملف اختياري فوق الملف الأساسي.
- `MasrLabDbContextFactory.cs:11` يستخدم سلسلة LocalDB ثابتة: `Server=(localdb)\mssqllocaldb;Database=MasrLabDb;Trusted_Connection=True;TrustServerCertificate=True;`.

**الاستنتاج:** بيئة التطوير تفترض LocalDB، وأي جهاز جديد يحتاج إنشاء `appsettings.Local.json` يدويًا، وإلا فشل الاتصال عند `UseSqlServer(configuration.GetConnectionString("DefaultConnection"))` (`src/MasrLab.Infrastructure/DependencyInjection.cs:27`).

### 4. حالة الخدمات الست

| الخدمة | الحالة | الميثودات غير المنفَّذة | الدليل |
|--------|--------|--------------------------|--------|
| `AuthenticationService` | ✅ منفَّذة | 0 من 2 | `Services/AuthenticationService.cs:19-39` — تحقق BCrypt (`:24`)، تعيين المستخدم الحالي (`:30`)، مسح الجلسة (`:37`) |
| `CurrentUserService` | ✅ منفَّذة بالكامل | 0 | `Services/CurrentUserService.cs:7-45` — تخزين آمن للخيوط بقفل، مع `SetCurrentUser`/`ClearCurrentUser` |
| `DateTimeService` | ✅ منفَّذة | 0 | `Services/DateTimeService.cs:7-8` |
| `PrintService` | ❌ غير منفَّذة | 2 من 2 | `Services/PrintService.cs:9,14` — `NotImplementedException` |
| `BackupService` | ❌ غير منفَّذة | 2 من 2 | `Services/BackupService.cs:9,14` — `NotImplementedException` |
| `BarcodeService` | ❌ غير منفَّذة | 1 من 1 | `Services/BarcodeService.cs:9` — `NotImplementedException` |

**الحصيلة: 3 منفَّذة، 3 غير منفَّذة (5 ميثودات ترمي `NotImplementedException`).**

### 5. تسجيلات Dependency Injection

`src/MasrLab.Infrastructure/DependencyInjection.cs` مكتمل:
- الـ Interceptors الاثنان (`:19-20`)، و`MasrLabDbContext` عبر factory يجمع كل `SaveChangesInterceptor` المسجلة (`:23-33`).
- `IUnitOfWork` (`:36-40`)، والخدمات الست (`:43-48`) — مع ملاحظة أن `ICurrentUserService` مسجلة **Singleton** (`:45`) بينما بقية الخدمات Scoped، وهو اختيار مقصود لحفظ جلسة المستخدم على مستوى التطبيق.
- المستودعات: `IRepository<>` العام (`:51`) + 19 مستودعًا متخصصًا (`:52-70`).

لا توجد فجوة تسجيل: كل واجهة في Application/Domain لها مقابل مسجَّل.

### 6. Persistence

- **`MasrLabDbContext`:** 37 DbSet (`MasrLabDbContext.cs:21-67`)، ربط الـ View عبر `HasNoKey().ToView("PatientHistoryView")` (`:74`)، وتوليد Query Filter تلقائي `IsDeleted = false` لكل كيان يطبّق `ISoftDeletable` (`:76-88`)، والتكوينات عبر `ApplyConfigurationsFromAssembly` (`:72`) — 37 ملف Configuration في `Persistence/Configurations/`.
- **الـ Interceptors:** `AuditableEntityInterceptor` يملأ حقول التدقيق باستخدام `ICurrentUserService.UserId ?? 0` (`Interceptors/AuditableEntityInterceptor.cs:40-53`)، و`SoftDeleteInterceptor` يحوّل الحذف إلى حذف منطقي (`Interceptors/SoftDeleteInterceptor.cs:30-37`). كلاهما يغطي المسارين المتزامن وغير المتزامن.
- **`UnitOfWork`:** يترجم أخطاء SQL Server 2601/2627 إلى `DuplicateLabIdException` و`DuplicateVisitLabIdException` (`Persistence/UnitOfWork.cs:24-31`). حالة الحافة الخاصة بالأنواع المملوكة (owned types) عولجت صراحة عبر `!e.Metadata.IsOwned()` (`UnitOfWork.cs:42, 56`) — أي أن الترجمة لم تعد تفشل عند وجود `Patient.Age` ككيان مملوك.

### 7. Seeders

- **`DefaultAdminSeeder` تغيّر جوهريًا:** `SeedAsync` أصبح No-Op يُرجع `Task.CompletedTask` (`Seeding/DefaultAdminSeeder.cs:12-15`) — **لا يوجد أي إنشاء لمستخدم افتراضي ولا كلمة مرور ثابتة**. أضيفت `IsFirstRunAsync` التي تتحقق من خلو جدول Users (`DefaultAdminSeeder.cs:7-10`).
- **الإنشاء الفعلي لأول مسؤول** ينتقل إلى شاشة الإعداد: `FirstRunSetupViewModel` يجزّئ كلمة المرور بـ BCrypt قبل الحفظ (`src/MasrLab.Presentation/ViewModels/FirstRunSetupViewModel.cs:57`) ويُنشئ المستخدم بـ `IsAdmin = true` (`:59-65`). المسار في `App.xaml.cs:42-53`: أول تشغيل → `FirstRunSetupWindow`، وإلا → `LoginWindow`، ولا يُفتح `MainWindow` إلا بعد نجاح أحدهما (`App.xaml.cs:65-67`).
- **`DefaultSettingsSeeder`** لم يتغير وظيفيًا: ينشئ 4 إعدادات (LabName, Currency, LabLogoPath, DefaultPrinter) فقط عند خلو الجدول (`Seeding/DefaultSettingsSeeder.cs:8-23`).

### 8. الاختبارات في Infrastructure.Tests

ثمانية ملفات في `tests/MasrLab.Infrastructure.Tests/`:
- 5 اختبارات تكاملية مشروطة بتوافر LocalDB عبر سمة `LocalDbFact` المخصصة (`LocalDbTestInfrastructure.cs:45-52`) التي تضبط سبب `Skip` وقت الاكتشاف إن غابت LocalDB: `LabIdUniqueIndexIntegrationTests`، `LabIdConcurrencyIntegrationTests`، `VisitRepositoryDateRangeIntegrationTests`، `AccountingRepositoryDateRangeIntegrationTests`، `OutsourcedSampleRepositoryDateRangeIntegrationTests`.
- `PlaceholderTests` (InMemory) و`AsyncCancellationPolicyTests` (تحليل كود) و`LocalDbTestInfrastructure` (مساعد بلا اختبارات).

**تغطية:** المستودعات العامة، تفرّد LabId، التزامن، استعلامات نطاقات التواريخ.
**لا تغطية إطلاقًا لـ:** الخدمات الست (بما فيها `AuthenticationService` المنفَّذ حديثًا)، الـ Seeders، الـ Interceptors، `PatientHistoryView`، وسلوك `Migrate` على قاعدة جديدة. كما لا يوجد اختبار لمسار أول تشغيل (`IsFirstRunAsync` → `FirstRunSetupViewModel`).

### 9. المخاطر الأمنية الحالية

| # | المخاطرة | الدليل | الخطورة |
|---|----------|--------|---------|
| 1 | `DefaultConnection` فارغة والملف المحلي غير مُتتبَّع — سلوك الإعداد متروك لكل جهاز بلا مرجعية | `appsettings.json:3`, `.gitignore:9` | متوسطة |
| 2 | `TrustServerCertificate=True` في سلاسل الاتصال الثابتة (factory والاختبارات) يلغي التحقق من شهادة TLS | `MasrLabDbContextFactory.cs:11`, `LocalDbTestInfrastructure.cs:26,60` | منخفضة (مقبولة محليًا، غير مقبولة للإنتاج) |
| 3 | لا يوجد Lockout ولا عدّاد محاولات فاشلة — `LoginAsync` يسمح بمحاولات غير محدودة | بحث شامل في `src/` عن lockout/FailedAttempt أعاد صفر نتائج؛ `AuthenticationService.cs:19-33` | متوسطة |
| 4 | فشل المصادقة يُرمَّز بـ `AuthResult(UserId=0, ...)` كنظير لـ null — هشّ وقابل للالتباس مع مستخدم حقيقي Id=0 نظريًا | `AuthenticationService.cs:26`, `LoginViewModel.cs:44` | منخفضة |
| 5 | الاستثناءات تُبتلَع بصمت في ViewModels وتُستبدل برسالة عامة — يصعّب تشخيص أعطال الإعداد الأول | `FirstRunSetupViewModel.cs:91-94`, `LoginViewModel.cs:56-59` | منخفضة |

**ما لم يعد قائمًا:** لا توجد كلمة مرور نصية واضحة في أي مكان، ولا بيانات اعتماد افتراضية معروفة، ولا `EnsureCreated` في مسار التشغيل — تحقق بحث شامل في `src/`.

### 10. فجوات مكتشفة حديثًا (لم تُوثَّق من قبل)

1. **خرق طبقي في Presentation:** `FirstRunSetupViewModel` يحقن `MasrLabDbContext` مباشرة ويكتب إلى قاعدة البيانات (`FirstRunSetupViewModel.cs:5,12,66,83`)، متجاوزًا طبقة Application (MediatR) رغم وجود `CreateUserCommand`. يجب إعادة توجيهه عبر أمر Application.
2. **`ManageBackupCommandHandler` لا يستدعي `IBackupService` إطلاقًا:** المعالج يكتفي بـ `SaveChangesAsync` فارغ (`ManageBackupCommandHandler.cs:15-19`) — ميزة النسخ الاحتياطي في Application مجرد قشرة بلا وظيفة حتى بعد تنفيذ `BackupService` ما لم يُربَط بها.
3. **ازدواجية مسار الطباعة:** حزمة QuestPDF مثبتة في Infrastructure، لكن خط أنابيب التقارير الفعلي يعيش في `src/MasrLab.Presentation/Printing/Reports/*` (11 تقريرًا) دون أي استهلاك لـ `IPrintService`؛ و`Printing/BarcodeGenerator.cs` فئة فارغة تمامًا رغم وجود `IBarcodeService`. موضع منطق الطباعة/الباركود النهائي يحتاج حسمًا معماريًا.
4. **ازدواجية ملفات الدخول:** توجد `LoginView.xaml` و`LoginWindow.xaml` معًا في `Views/`؛ المستخدم فعليًا هو `LoginWindow` (`App.xaml.cs:56`)، ومصير `LoginView` غير واضح.
5. **تسجيل الدخول لا يُسجَّل حضوريًا:** نجاح `LoginAsync` لا يُتبَع بإرسال `RecordLoginCommand` رغم وجود المعالج (`RecordLoginCommandHandler.cs`) — سجل الحضور لن يمتلئ ما لم يُربَط في مسار الدخول.

---

## ثالثًا: القرارات المطلوبة

### القرارات التي حُسمت فعليًا في الكود (لا تحتاج قرارًا)

| السؤال | الحالة | الدليل |
|--------|--------|--------|
| مكتبة التجزئة | ✅ محسوم — BCrypt.Net-Next 4.2.0 | `MasrLab.Infrastructure.csproj:15`, `AuthenticationService.cs:24`, `FirstRunSetupViewModel.cs:57` |
| مكتبة الباركود | ✅ محسوم على مستوى الحزمة — ZXing.Net 0.16.11 | `MasrLab.Infrastructure.csproj:29` (التنفيذ لم يُكتب بعد) |
| مكتبة الطباعة/PDF | ✅ محسوم على مستوى الحزمة — QuestPDF 2026.7.2 | `MasrLab.Infrastructure.csproj:28` (التنفيذ لم يُكتب بعد) |
| حذف المستخدم الافتراضي Admin | ✅ محسوم بشكل أقوى — أُلغي الافتراضي كليًا واستُبدل بإعداد أول تشغيل | `DefaultAdminSeeder.cs:12-15`, `App.xaml.cs:42-53` |
| إلزامية شاشة الدخول | ✅ محسوم — `LoginWindow` إلزامية قبل `MainWindow` | `App.xaml.cs:56-63` |
| آلية تخزين المستخدم الحالي | ✅ محسوم — Singleton بقفل خيوط، يُعيَّن بعد Login | `DependencyInjection.cs:45`, `CurrentUserService.cs:7-45` |

### القرارات التي ما زالت مفتوحة فعليًا

**القرار 1 — قاعدة بيانات الإنتاج**
- **الخيارات:** (أ) LocalDB فقط، (ب) SQL Server Express، (ج) SQL Server كامل.
- **الوضع في الكود:** كل السلاسل الثابتة تشير إلى LocalDB (`MasrLabDbContextFactory.cs:11`)، و`appsettings.json` فارغة بلا أي توجه.
- **التوصية الفنية:** اعتماد (ب) SQL Server Express كهدف إنتاج افتراضي لمختبر واحد (يدعم الخدمات الخلفية والنسخ الاحتياطي المجدول على عكس LocalDB)، مع إبقاء LocalDB لبيئة التطوير والاختبارات. القرار يؤثر مباشرة في تصميم `BackupService` (صلاحيات BACKUP DATABASE تختلف بين الإصدارين).

**القرار 2 — أحادية/تعددية المعمل (Tenancy)**
- **الخيارات:** (أ) معمل واحد (single-tenant)، (ب) تعدد معامل.
- **الوضع في الكود:** لا أي أثر لعمود Tenant أو عزل بيانات في أي كيان أو تكوين — البنية الحالية single-tenant ضمنيًا.
- **التوصية الفنية:** توثيق (أ) صراحة كقرار تصميمي؛ التحول لاحقًا إلى (ب) سيكون ترحيلاً مؤلمًا على كل جدول، فالأفضل حسمه الآن حتى لو بالرفض الموثَّق.

**القرار 3 (جديد) — الموضع المعماري النهائي لمنطق الطباعة والباركود**
- **الخيارات:** (أ) تنفيذ كامل داخل `PrintService`/`BarcodeService` في Infrastructure واستهلاك تقارير Presentation الحالية عبرها، (ب) إبقاء التوليد في Presentation وإعادة تعريف الخدمتين كمنسّقين (Orchestrators) فقط.
- **التوصية الفنية:** (أ) — يحافظ على العقد (`DD-10`) ويجعل المنطق قابلاً للاختبار خارج WPF.

---

## رابعًا: الفجوات التي تمنع البدء الفعلي

| الفجوة | حاجبة؟ | الأثر |
|--------|--------|-------|
| `DefaultConnection` فارغة وغياب `appsettings.Local.json` من المستودع | **حاجبة للتشغيل والاختبارات التكاملية فقط** | لا يمنع كتابة كود الخدمات الثلاث واختباراتها الوحدوية (InMemory/Moq)، لكن يمنع تشغيل التطبيق و`dotnet test` التكاملي |
| عدم وجود بيئة LocalDB/SQL Server في بيئة التدقيق الحالية | **قيد بيئي معلن** | هذا التدقيق قائم على تحليل الكود فقط؛ لا يمكن التحقق من نجاح `MigrateAsync` أو وجود `__EFMigrationsHistory` على قاعدة حية |
| الخدمات الثلاث غير المنفَّذة | **غير حاجبة لبعضها** | يمكن تنفيذها بالتوازي؛ لا يعتمد أي منها على الآخر |
| قرار قاعدة بيانات الإنتاج | **حاجب لـ `BackupService` فقط** | شكل أمر النسخ الاحتياطي وصلاحياته يتبعان نوع الخادم المستهدف |

**لا توجد فجوة تمنع بدء كتابة الكود الآن.**

---

## خامسًا: خارطة الطريق التنفيذية

> الترميز: 🔒 حاجب (يجب إتمامه قبل أي خطوة لاحقة في مساره) — ⚡ غير حاجب (قابل للتوازي).

### المرحلة 0 — تجهيز البيئة (حاجبة للتحقق، غير حاجبة لكتابة الكود)

| # | الخطوة | النوع | التبعية | معيار القبول |
|---|--------|-------|---------|--------------|
| 0.1 | إنشاء `appsettings.Local.json` بسلسلة LocalDB على جهاز التطوير | 🔒 للتشغيل | — | التطبيق يتجاوز `MigrateAsync` (`App.xaml.cs:38`) دون استثناء اتصال |
| 0.2 | إضافة `appsettings.Local.example.json` مُتتبَّع في Git يوثّق الصيغة المطلوبة | ⚡ | — | ملف مرجعي موجود بجوار `appsettings.json` |
| 0.3 | تشغيل التطبيق على قاعدة جديدة والتحقق من إنشاء `__EFMigrationsHistory` وتطبيق الترحيلات الست | 🔒 للتحقق التكاملي | 0.1 | جدول التاريخ يحوي 6 صفوف، و`PatientHistoryView` يحوي `ComparisonFlag` |
| 0.4 | اجتياز مسار أول تشغيل كاملاً: إنشاء مسؤول عبر `FirstRunSetupWindow` ثم دخول عبر `LoginWindow` | 🔒 للتحقق التكاملي | 0.3 | كلمة المرور مخزنة مجزّأة (تبدأ بـ `$2`)، و`MainWindow` لا يفتح قبل نجاح الدخول |

### المرحلة 1 — BarcodeService (⚡ مستقلة)

| # | الخطوة | التبعية | معيار القبول |
|---|--------|---------|--------------|
| 1.1 | تنفيذ `GenerateBarcode` عبر ZXing.Net بإخراج PNG `byte[]` | — | لا يرمي `NotImplementedException`؛ يُرجع صورة بالأبعاد المطلوبة |
| 1.2 | اختبارات وحدة: محتوى معروف → بايتات صالحة، وسلسلة فارغة → استثناء واضح | 1.1 | اختبارات تمر دون LocalDB |
| 1.3 | حسم موضع الاستهلاك: ربط `Printing/BarcodeGenerator.cs` الفارغة بالخدمة أو حذفها | 1.1 + القرار 3 | مسار باركود واحد فقط في الحل |

### المرحلة 2 — PrintService (⚡ مستقلة)

| # | الخطوة | التبعية | معيار القبول |
|---|--------|---------|--------------|
| 2.1 | حسم القرار 3 ثم تنفيذ `RenderAsync` عبر QuestPDF بموجب عقد DD-10 | القرار 3 | يُرجع PDF `byte[]` صالح لتقرير واحد مرجعي على الأقل |
| 2.2 | تنفيذ `PrintAsync` (إرسال للطابعة المحددة أو الافتراضية من إعداد `DefaultPrinter`) | 2.1 | طباعة تجريبية ناجحة على جهاز التطوير |
| 2.3 | اختبارات وحدة للـ Render (بنية PDF، لا بكسلات) | 2.1 | اختبارات تمر دون طابعة |

### المرحلة 3 — BackupService (⚡ لكنها تنتظر قرارًا)

| # | الخطوة | التبعية | معيار القبول |
|---|--------|---------|--------------|
| 3.1 | حسم القرار 1 (قاعدة بيانات الإنتاج) | — | قرار موثَّق في `Docs/DecisionRecords/` |
| 3.2 | تنفيذ `BackupAsync` عبر أمر `BACKUP DATABASE` أو `SqlPackage` حسب القرار | 3.1 | ملف `.bak` صالح يُنشأ على بيئة التطوير |
| 3.3 | تنفيذ `RestoreAsync` مع حارس أمان (تأكيد + إغلاق الاتصالات) | 3.2 | دورة Backup→Restore ناجحة على قاعدة اختبار |
| 3.4 | ربط `ManageBackupCommandHandler` بالخدمة فعليًا (إصلاح الفجوة الحديثة #2) | 3.2 | أمر MediatR يؤدي إلى نسخة فعلية |

### المرحلة 4 — إصلاحات هيكلية (⚡ قابلة للتوازي مع المراحل 1–3)

| # | الخطوة | التبعية | معيار القبول |
|---|--------|---------|--------------|
| 4.1 | نقل إنشاء المسؤول الأول من `FirstRunSetupViewModel` إلى أمر Application (إصلاح الخرق الطبقي) | — | ViewModel لا يراجع Infrastructure إطلاقًا |
| 4.2 | ربط نجاح الدخول بـ `RecordLoginCommand` | — | سجل AttendanceLog يُنشأ عند كل دخول ناجح |
| 4.3 | حسم مصير `LoginView.xaml` (حذف أو دمج) | — | نافذة دخول واحدة فقط |
| 4.4 | تحديث أو حذف `PatientHistoryView.sql` الـ Placeholder | — | لا تعريفان متضاربان للـ View |

### المرحلة 5 — تغطية الاختبارات (⚡ تتبع كل مرحلة)

| # | الخطوة | التبعية | معيار القبول |
|---|--------|---------|--------------|
| 5.1 | اختبارات `AuthenticationService`: نجاح/فشل/مستخدم محذوف | مرحلة 0 (للتكامل) أو InMemory | تغطية الحالات الثلاث |
| 5.2 | اختبارات الـ Interceptors (حقول التدقيق، الحذف المنطقي) | — | تمر على InMemory |
| 5.3 | اختبارات الـ Seeders ومسار `IsFirstRunAsync` | — | تمر على InMemory |
| 5.4 | اختبار تكاملي لتطبيق الترحيلات الست على قاعدة جديدة والاستعلام من `PatientHistoryView` | 0.1 | يعمل بـ `LocalDbFact` ويُتخطَّى بنظافة بلا LocalDB |

### المرحلة 6 — تقوية أمنية (⚡ يمكن تأجيلها لما بعد الإطلاق الداخلي)

| # | الخطوة | معيار القبول |
|---|--------|--------------|
| 6.1 | Lockout مؤقت بعد N محاولات فاشلة في `AuthenticationService` | المحاولة N+1 تُرفض زمنيًا مع تسجيل تدقيقي |
| 6.2 | استبدال نظير الفشل `AuthResult(UserId=0)` بتمييز صريح (nullable أو حالة) | لا التباس مع معرّف حقيقي |
| 6.3 | سياسة اتصال الإنتاج: إزالة `TrustServerCertificate=True` خارج بيئة التطوير | موثَّقة مع القرار 1 |

---

## سادسًا: تقييم الجاهزية النهائي

**هل يمكن البدء الآن؟ نعم — بشرطين:**

1. **للكود:** البدء فوري ومفتوح. المراحل 1 و2 و4 لا تتطلب قاعدة بيانات ولا قرارات إضافية.
2. **للتشغيل والتحقق:** يتطلب إتمام الخطوة 0.1 (ملف `appsettings.Local.json` على جهاز يحوي LocalDB) — وهو عمل دقائق، لكنه خارج قدرة بيئة التدقيق السحابية الحالية.

**الخطوة الأولى بالضبط:** الخطوة **0.1 + 0.3** (تجهيز سلسلة الاتصال ثم تشغيل التطبيق على قاعدة جديدة والتحقق من تطبيق الترحيلات الست وإنشاء `__EFMigrationsHistory`) — لأنها تحوّل كل حقائق هذا التدقيق من «مثبتة في الكود» إلى «مثبتة في التشغيل»، وتفتح الباب للاختبارات التكاملية المشروطة بـ `LocalDbFact`. يتوازى معها مباشرة الشروع في **المرحلة 1 (BarcodeService)** كأول تنفيذ خدمة لا يحتاج أي قرار ولا قاعدة بيانات.

---

## سابعًا: تأكيد نطاق التدقيق

- التدقيق أُجري حصريًا على الكود عند الكوميت `2a49bf54ac5fbffc18f8f4b9c15676acbbd7eb93` (الفرع `niamod`).
- بيئة التدقيق سحابية معزولة بلا SQL Server/LocalDB وبلا .NET SDK — كل الاستنتاجات قائمة على تحليل الكود المصدري؛ أي سلوك تشغيلي (نجاح الترحيلات فعليًا، وجود `__EFMigrationsHistory` على جهاز حقيقي) مُدرج كخطوة تحقق في المرحلة 0 ولم يُزعَم إثباته.
- لم يتم تعديل أو إنشاء أو حذف أو إعادة تسمية أي ملف في الحل البرمجي أثناء هذا التدقيق.
