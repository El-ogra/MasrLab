# MasrLab — خارطة طريق إكمال المشروع عبر كل الطبقات

**الكوميت المُدقَّق (Hash إلزامي):** `51f4d0f7ce8645613a1faec0ca5ce50f18f98d59`
**الفرع:** `niamod` — **رسالة الكوميت:** «تنفيذ التدفق الثالث»
**تاريخ التدقيق:** 2026-08-09 (UTC)
**المصدر الوحيد للحقيقة:** الكود عند هذا الكوميت. أي ملف داخل `Docs/` (بما في ذلك أي وثيقة خارطة طريق سابقة أو ملفات `Docs/DecisionRecords/` و`Docs/Archive/`) عومل كفرضية موثَّقة لا كحقيقة، وتم التحقق من كل ادعاء مباشرة في الكود.
**قيود بيئة التدقيق:** لم يكن `dotnet` متاحًا في بيئة التدقيق (`which dotnet` → لا شيء). لذلك **كل أحكام هذه الوثيقة أحكام ساكنة على الكود، غير مُثبتة تنفيذيًا** بـ `dotnet build` أو `dotnet test`. كل بند وسم بذلك عند اللزوم.

---

## 1) الملخص التنفيذي

عند هذا الكوميت المشروع يتألف من أربعة مشاريع مصدر:
`MasrLab.Domain` (Class Library, `net8.0`), `MasrLab.Application` (`net8.0`), `MasrLab.Infrastructure` (`net8.0`), `MasrLab.Presentation` (WPF, `net8.0-windows`) — بدليل `grep -h TargetFramework src/*/*.csproj`. وثلاثة مشاريع اختبار: `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`.

الحكم الموجز على الحالة الفعلية عند الكوميت:

- **طبقة Application:** قوية بنيويًا وشاملة عرضًا (72 معالجًا: 43 معالج أمر + 29 معالج استعلام؛ 43 مدققًا يغطي كل الأوامر). أُنجزت التدفقات الثلاثة المالكة المزعومة (زيارة → اختبار → إيصال) والتدفق الثالث لإدخال النتائج، لكن **تبقى ثغرات فعلية**: تغطية اختبارات المعالجات ضحلة (≈ 6 معالجات مُختبَرة من 72، إضافة لأربعة مدققات)، لا يوجد اختبار تكامل يحلّ الحاوية `AddApplication + AddInfrastructure`، عقد الأخطاء غير موحّد (5 معالجات ترمي `Exception` مجرّدة و12+ معالج يرمي `InvalidOperationException` بينما يعرِّف الدومين `EntityNotFoundException` غير مستعملة)، ملفا Placeholder ما زالا في `Common/Models` و`Common/Validations`، وسياسة فشل حفظ التدقيق **ما زالت تبتلع الاستثناء بصمت** بعد التسجيل التحذيري فقط.
- **طبقة Infrastructure:** الاستمرارية (Persistence) ناضجة (37 `DbSet<>`، 30 EntityTypeConfiguration، اعتراضات Audit/Soft‑Delete، 5 migrations متسلسلة والـSnapshot متزامن معها)، ومستودعات كل الواجهات مسجَّلة. لكن **4 خدمات من أصل 6 خدمات Infrastructure غير مُنفَّذة إطلاقًا**: `AuthenticationService`, `BackupService`, `BarcodeService`, `PrintService` كلها تحتوي `throw new NotImplementedException()` صريحة، و`CurrentUserService.UserId => null` بلا مصدر هوية فعلي، مما يجعل تشغيل التطبيق الإنتاجي متعذرًا.
- **طبقة Presentation:** **هيكل فارغ تقريبًا**. 30 ViewModel معظمها 7 أسطر فقط (`ObservableObject` بلا أي محتوى)، و30 View بـ`<Grid/>` فارغ، لا استهلاك واحد لـ `IMediator`/`ISender`/`MediatR` في كامل `src/MasrLab.Presentation` (بحث صفري)، ولا معالجة `ValidationException`، ولا استهلاك واحد لـ `ICurrentUserService`، ولا `INotificationHandler` لأي حدث مجال في كامل `src/`.
- **ترتيب العمل المُحقَّق:** الترتيب المقترح (إكمال Application ثم Infrastructure بادئًا بـAuthentication ثم Presentation) صحيح **جزئيًا**، مع تصحيح جوهري: هوية المستخدم في التدفقات الحالية **لا تأتي من `ICurrentUserService` عمليًا** بل عبر معاملات أمر صريحة (`ReceivedByUserId`, `EnteredByUserId`, `UserId`). لذلك يمكن اختبار Application كاملًا دون نظام دخول. **الحاجب الفعلي لبناء واجهة أمامية مفيدة هو غياب `AuthenticationService` مقترنًا بـ`CurrentUserService` قادر على حمل الهوية بعد الدخول** — لا مجرد `ICurrentUserService`.

---

## 2) الحالة الفعلية لطبقة Application (بالأدلة)

### 2.1 البنية العرضية (تُعدّ كاملة)

| البند | الحالة | الدليل |
|---|---|---|
| عدد المعالجات | ✅ 72 | `find src/MasrLab.Application/Features -name '*Handler.cs' \| wc -l` → 72 |
| توزيع المعالجات | ✅ 43 أمر + 29 استعلام | `-name '*CommandHandler.cs' \| wc -l` → 43؛ `-name '*QueryHandler.cs' \| wc -l` → 29 |
| المدققات | ✅ 43 مدققًا يغطي كل الأوامر الـ43 | `find … -name '*Validator.cs' \| wc -l` → 43؛ ولا يوجد أمر بلا مدقق ملاصق (فحص جميع مجلدات الأوامر أرجع صفرًا للنواقص). |
| DI مركزي في Application | ✅ | `src/MasrLab.Application/DependencyInjection.cs:13-37` يسجّل MediatR/AutoMapper/Validators/سلوكين + 10 خدمات دومين. |
| Behaviors مسجَّلة | ✅ Validation و Audit | `DependencyInjection.cs:19-20`. |
| خدمات المجال في Application/Services | ✅ 10 خدمات (Accounting, CultureSensitivity, MedicalHistory, Outsourcing, PriceListResolver, Pricing, ReceiptCalculation, ReferralCommission, ResultValidation, SampleTracking) | `ls src/MasrLab.Application/Services/`. |
| DTOs | ✅ 49 ملف DTO | `ls src/MasrLab.Application/Common/DTOs/` (49 عنصرًا). |
| Profiles | ✅ 9 ملفات Profile | `ls src/MasrLab.Application/Common/Mappings/Profiles/`. |
| لا استيراد لـ Infrastructure من Application | ✅ | مرجع `.csproj` نظيف: `src/MasrLab.Application/MasrLab.Application.csproj` لا يحتوي إشارة لمشروع Infrastructure. |

### 2.2 التدفقات المالكة الثلاثة المزعومة (F‑1 → F‑3)

- **F‑1 إنشاء زيارة (`CreatePatientVisitCommandHandler`):** موجود ويستهلك `ICurrentUserService` — `src/MasrLab.Application/Features/PatientVisits/Commands/CreatePatientVisit/CreatePatientVisitCommandHandler.cs:19,26,43,48`. يرمي `InvalidOperationException("Current user is not authenticated.")` عند غياب `UserId` (سطر 44).
- **F‑2 إضافة اختبار للزيارة + إصدار إيصال:** `AddTestToVisitCommandHandler` يستهلك `IPriceListResolverService` و`IPricingService` (بحث في `Features/**` أرجع الملفين: `AddTestToVisit/AddTestToVisitCommandHandler.cs` و`IssueReceipt/IssueReceiptCommandHandler.cs`). `IssueReceiptCommandHandler.cs:42,49,55` يستخدم `BusinessRuleViolationException` و`InvalidOperationException`، و`:98` يمرر `request.ReceivedByUserId` مباشرة (لا `ICurrentUserService`).
- **F‑3 إدخال نتيجة اختبار:** `EnterTestResultCommandHandler.cs:16,25,51,73` يستهلك `ISampleTrackingService`، ويشترط `OverrideReason` عند عدم تحصيل العينة (السطر 51). المعامل `EnteredByUserId` يأتي من الأمر لا من `ICurrentUserService` (السطر 68).

### 2.3 حالة سلوك `AuditBehavior` (سياسة فشل الحفظ)

**النتيجة:** سياسة ابتلاع فشل حفظ سجل التدقيق **ما زالت قائمة** (تسجيل تحذيري فقط ثم متابعة). دليل مباشر:

```
src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs:72   catch (Exception ex)
src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs:75   _logger.LogWarning(ex, "Failed to persist audit entry for request {RequestName}.", requestName);
```

التعليق داخل الميثود صريح (`AuditBehavior.cs:74`): «Audit persistence failure must not abort the request». هذا قرار معماري معلَّق لم يُحسم بعد: هل يبقى كما هو، أم يُصعَّد إلى استثناء عند فشل تدقيق حسّاس، أم يُنقل إلى قناة مراقبة خارجية.

### 2.4 عقد الأخطاء عبر الطبقة (غير موحّد)

المعالجات ترمي أنواع استثناءات مختلفة بلا سياسة واحدة:

- **5 معالجات ترمي `Exception` مجرَّدة** (سيئة لعقد UI):
  - `Features/ResultsEntry/Commands/CreateBlankReport/CreateBlankReportCommandHandler.cs:20`
  - `Features/ResultsEntry/Commands/CreateCombinedReport/CreateCombinedReportCommandHandler.cs:20`
  - `Features/SampleCollection/Commands/MarkSampleCollected/MarkSampleCollectedCommandHandler.cs:21`
  - `Features/TestGroups/Commands/ManageTestGroups/ManageTestGroupsCommandHandler.cs:28`
  - `Features/UsersAndPermissions/Commands/UpdateUser/UpdateUserCommandHandler.cs:21`

- **12+ رمي `InvalidOperationException`** لغياب كيان (يجب أن يكون `EntityNotFoundException` من الدومين): معالجات `PriceList`, `PatientVisit`, `Patient`, `Culture`, `AttendanceLog`, `Test`, `Account`, `VisitTest` (`grep -rh "throw new" src/MasrLab.Application/Features --include='*Handler.cs' | sort | uniq -c`).

- **الاستثناء المُصمَّم للدومين `EntityNotFoundException` غير مستعمل مطلقًا داخل Application** (`grep -rn EntityNotFoundException src/MasrLab.Application` → صفر). وكذلك `InsufficientPermissionException` معرَّف في `src/MasrLab.Domain/Exceptions/InsufficientPermissionException.cs:3` لكن بحث استعماله في Application/Features يعود صفرًا.

### 2.5 اختبار تكامل حاوية DI

**لا يوجد.** بحث `ServiceProvider|BuildServiceProvider|AddApplication|AddInfrastructure` في `tests/` أعاد **صفر نتائج**. هذا فراغ حرج: لا شيء يضمن أن `AddApplication()+AddInfrastructure(config)` تُحلّ كل التبعيات بنجاح.

### 2.6 ملفات Placeholder المتبقية

اثنان لم يُنظَّفا:

- `src/MasrLab.Application/Common/Models/_Placeholder.cs` — تعليق داخلي يحيل إلى «خارطة قديمة» ولا يوجد `Result<T>` مشترك ولا تعاقد أخطاء موحّد.
- `src/MasrLab.Application/Common/Validations/_Placeholder.cs` — تعليق يشير إلى «قواعد التحقق المشتركة في المرحلة 9» لم تُضَف.

كذلك يوجد ملف اختبار بأسماء مضلِّلة خارج طبقة Application يلوّث نظافة الاختبارات ككل (يُعالج في مرحلة Infrastructure):
- `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs` — يحوي فعليًا `GenericRepositoryTests`, `UnitOfWorkTests`, `PatientRepositoryTests` (اسم الملف مضلِّل جدًا).
- `tests/MasrLab.Domain.Tests/PlaceholderTests.cs` — يحوي `BaseEntityTests`, `EnumTests`, `EntityTests` (اسم مضلِّل).

### 2.7 تغطية اختبارات المعالجات/الخدمات/المدققين (عدّ دقيق)

- إجمالي اختبارات `MasrLab.Application.Tests`: **82 اختبار `[Fact]/[Theory]`** (`grep -rc '\[Fact\]\|\[Theory\]' tests/MasrLab.Application.Tests --include='*.cs'`).
- **اختبارات المعالجات (Handlers)**: 6 معالجات مُختبَرة من أصل 72 = **≈ 8.3%**:
  - `AddTestToVisitCommandHandlerTests.cs`
  - `CreatePatientVisitCommandHandlerTests.cs`
  - `CreatePriceListCommandHandlerTests.cs`
  - `EnterTestResultCommandHandlerTests.cs`
  - `GetPatientHistoryQueryHandlerTests.cs`
  - `IssueReceiptCommandHandlerTests.cs`
  - `SetDefaultPriceListCommandHandlerTests.cs` (7 ملفات، لكن ملف واحد يغطي أمر واحد)
- **اختبارات المدققين**: 4 مدققين مُختبَرين من 43 = **≈ 9.3%**:
  - `AddTestCommandValidatorTests.cs`, `CreateUserCommandValidatorTests.cs`, `RegisterPatientCommandValidatorTests.cs`, `UpdatePriceListItemsCommandValidatorTests.cs`.
- **اختبارات الخدمات الداخلية**: 3 خدمات مُختبَرة من 10 = **30%**:
  - `AccountingServiceTests.cs`, `PriceListResolverServiceTests.cs`, `ReferralCommissionServiceTests.cs`.
- **اختبارات Behaviors**: كاملة للسلوكين — `ValidationBehaviorTests.cs`, `AuditBehaviorTests.cs` (3 حالات: نجاح، فشل مع إعادة رمي، فشل حفظ التدقيق دون إحباط — أسطر 61,80,99).
- **اختبارات AutoMapper**: `MappingConfigurationTests.cs` (`AssertConfigurationIsValid`) + `MappingProfileTests.cs` بـ8 فئات profile.

### 2.8 حقن ميت داخل Application/Services

`PricingService.cs:11-14` يحقن `IPriceListResolverService` **دون أن يستخدمه** في `CalculateSubtotal` (يقرأ فقط `visit.VisitTests.Sum(vt => vt.Price)`) ولا في `CalculateTotal`. حقن ميت يجب حذفه أو استعماله (توثيق سياسة تخصص كل خدمة).

### 2.9 ملخّص «ما تبقى بالضبط لإكمال Application»

| # | البند | خطورته | الدليل |
|---|---|---|---|
| A1 | توحيد عقد الأخطاء: استبدال 5 مواضع `throw new Exception(...)` و12+ موضع `InvalidOperationException("... not found")` بـ `EntityNotFoundException` (وسياسة موحّدة). | عالية | §2.4 |
| A2 | حسم سياسة فشل `AuditBehavior`: قرار موثَّق (يبقى تحذيرًا؟ يُصعَّد لبعض الطلبات؟ Outbox خارجي؟) + توثيق في DecisionRecord جديد. | عالية | §2.3 |
| A3 | حذف الحقن الميت في `PricingService` أو استخدامه فعليًا. | متوسطة | §2.8 |
| A4 | حذف `Common/Models/_Placeholder.cs` و`Common/Validations/_Placeholder.cs` بعد إضافة `Result<T>` (اختياري) وقواعد تحقق مشتركة (اختياري). | منخفضة | §2.6 |
| A5 | رفع تغطية اختبارات المعالجات من ≈ 8% إلى مستوى تشغيلي (كل معالج ينتمي لمسار مالك حرج). | عالية | §2.7 |
| A6 | رفع تغطية اختبارات المدققين من ≈ 9% إلى تغطية شاملة (43/43) على الأقل للأوامر ذات القواعد غير التافهة. | متوسطة | §2.7 |
| A7 | إضافة **اختبار تكامل حاوية DI**: يبني `AddApplication()+AddInfrastructure(config)` ويحلّ كل `IRequestHandler<,>` و`IValidator<>` بنجاح. | عالية | §2.5 |
| A8 | تسمية ملفات اختبار مضلِّلة (خارج Application لكنها تلوث مصفوفة الاختبارات ككل): تُعالج ضمن مرحلة Infrastructure. | منخفضة | §2.6 |

---

## 3) الحالة الفعلية لطبقة Infrastructure (بالأدلة)

### 3.1 الاستمرارية (Persistence) — ناضجة

- `DbContext` سليم مع `ApplyConfigurationsFromAssembly` (`src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs:69-72`).
- **37 `DbSet<>` مسجَّل** يغطي 36 كيانًا في `src/MasrLab.Domain/Entities/**` بالإضافة إلى `PatientHistoryView`.
- **30 EntityTypeConfiguration** موزَّعة على `Administrative/Core/Culture/Financial/Settings`.
- **Interceptors مسجَّلة**: `AuditableEntityInterceptor`, `SoftDeleteInterceptor` (`DependencyInjection.cs:15-16`).
- **UnitOfWork** يوجد ويترجم أخطاء SQL 2601/2627 لكيان `Patient` إلى `DuplicateLabIdException` (اعتُمد سابقًا في تحصين LabId).
- **Migrations**: خمس هجرات متسلسلة (`Migrations/`):
  1. `20260803173749_InitialCreate`
  2. `20260804140414_AddPatientHistoryView`
  3. `20260804211139_AddRequestAuditLog`
  4. `20260805033329_AddUniquePatientLabIdIndex`
  5. `20260808223709_AddOverrideReasonToTestResult`
- **الـSnapshot متزامن**: `Migrations/MasrLabDbContextModelSnapshot.cs` يحوي `OverrideReason` (`grep -c OverrideReason … Snapshot.cs` → 1). لا `migrations معلّقة` من طرف الكود ضدّ الـSnapshot عند هذا الكوميت. **ملاحظة**: هل تتماشى الميجريشن مع الشيمّا المتوقَّعة في `Docs/Future-Migrations.md`؟ الشيمّا المتوقَّعة تذكر جداول متوقّعة لكن التحقق منها ميدانيًا يتطلب `dotnet ef migrations script` — **غير مُثبت تنفيذيًا** في بيئة التدقيق.

### 3.2 تسجيل الحاوية

- كل الخدمات الست مسجَّلة (`DependencyInjection.cs:39-45`): `IAuthenticationService`, `IDateTimeService`, `ICurrentUserService`, `IPrintService`, `IBackupService`, `IBarcodeService`.
- **20 مستودع** مسجَّل صراحةً + `IRepository<>` عام (`DependencyInjection.cs:59-77`) — يغطي كل واجهات المستودعات المعرَّفة في `src/MasrLab.Domain/Interfaces/`.
- التسجيل مكتمل بنيويًا. **الفراغ الحقيقي في التنفيذ لا في التسجيل.**

### 3.3 خدمات Infrastructure — الحقيقة الفعلية

**4 خدمات من 6 غير منفَّذة إطلاقًا** (`grep -rn NotImplementedException src/MasrLab.Infrastructure`):

| الخدمة | الحالة | الدليل |
|---|---|---|
| `AuthenticationService` | ❌ غير منفَّذ | `Services/AuthenticationService.cs:10` (`LoginAsync`), `:15` (`LogoutAsync`) — كلاهما `throw new NotImplementedException()`. |
| `BackupService` | ❌ غير منفَّذ | `Services/BackupService.cs:9,:14` — `BackupAsync`, `RestoreAsync`. |
| `BarcodeService` | ❌ غير منفَّذ | `Services/BarcodeService.cs:9` — `GenerateBarcode`. |
| `PrintService` | ❌ غير منفَّذ | `Services/PrintService.cs:9,:14` — `PrintAsync`, `RenderAsync`. |
| `DateTimeService` | ✅ منفَّذ | `Services/DateTimeService.cs` — `Now`, `UtcNow`. |
| `CurrentUserService` | 🟡 ستَب متعمَّد | `Services/CurrentUserService.cs:7` → `public int? UserId => null;` — لا مصدر هوية فعلي. |

### 3.4 اختبارات Infrastructure

- إجمالي: **10 اختبارات** فقط عبر 8 ملفات (`grep -rc '\[Fact\]\|\[Theory\]' tests/MasrLab.Infrastructure.Tests`).
- تشمل: `AccountingRepositoryDateRangeIntegrationTests`, `AsyncCancellationPolicyTests`, `LabIdConcurrencyIntegrationTests`, `LabIdUniqueIndexIntegrationTests`, `OutsourcedSampleRepositoryDateRangeIntegrationTests`, `VisitRepositoryDateRangeIntegrationTests`, `PlaceholderTests` (يخفي 5 مستودعات مختبَرة على InMemory).
- اختبارات LocalDB **تُتخطَّى تلقائيًا** عندما LocalDB غير متاح (`LocalDbTestInfrastructure.cs:16` `LocalDbAvailability.UnavailableReason` — يستخدم Skip).
- **لا يوجد** اختبار حاوية Infrastructure كامل يتحقق من `AddInfrastructure(config)`.

### 3.5 ملخّص «ما تبقى لطبقة Infrastructure»

| # | البند | الاعتماد |
|---|---|---|
| I1 | تنفيذ `AuthenticationService.LoginAsync/LogoutAsync` فعليًا: قراءة `User` من DB، تحقق كلمة مرور مُجزَّأة (BCrypt/Argon2)، تحميل `Permission[]` عبر `IPermissionRepository`، إرجاع `AuthResult(UserId, Username, Permissions)` المعرَّف في `src/MasrLab.Application/Common/Models/AuthResult.cs`. | لا يعتمد على Presentation. |
| I2 | إعادة تصميم `CurrentUserService` ليصبح متغيرًا حسب الجلسة/الخيط — الحل الشائع في WPF: `AmbientContext` أو `HolderService` بسمة `Set(int userId)` تُستدعى من Presentation بعد نجاح تسجيل الدخول. حاليًا الصيغة `UserId => null` تجعل `AuditBehavior` يسجل كل الطلبات بـ `UserId = null`، وتجعل `CreatePatientVisitCommandHandler:44` يرمي دائمًا `InvalidOperationException("Current user is not authenticated.")`. | مُلزم قبل استهلاك Presentation لأمر إنشاء زيارة. |
| I3 | تنفيذ `PrintService.PrintAsync/RenderAsync` (اقتراح: QuestPDF أو RDLC لتوليد PDF، ومحرك طباعة Windows عبر `PrintDialog`/`XpsDocumentWriter`). | تعتمد عليه شاشات الإيصال/التقرير في Presentation. |
| I4 | تنفيذ `BarcodeService.GenerateBarcode` (اقتراح: ZXing.Net). | يعتمد عليه إصدار الإيصال + غلاف العينة. |
| I5 | تنفيذ `BackupService.BackupAsync/RestoreAsync` (SQL Server `BACKUP DATABASE`/استعادة عبر `SqlConnection`). | مستقل، يمكن تأخيره إلى ما بعد الحد الأدنى القابل للاستخدام. |
| I6 | إعادة تسمية `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs` إلى ملفات موصوفة (`GenericRepositoryTests.cs`, `UnitOfWorkTests.cs`, `PatientRepositoryTests.cs`). كذلك `tests/MasrLab.Domain.Tests/PlaceholderTests.cs` (تحتوي فعليًا `BaseEntityTests`, `EnumTests`, `EntityTests`). | مستقل. |
| I7 | إضافة **اختبار تكامل حاوية**: `services.AddApplication().AddInfrastructure(config).BuildServiceProvider()` يحلّ كل `IRequestHandler<,>` المُكتشف بالانعكاس. | مستقل. |
| I8 | تحقق فعلي من الميجريشن مقابل `Docs/Future-Migrations.md` بتشغيل `dotnet ef migrations script` عند توفر بيئة `dotnet`. | مستقل. **غير مُثبت في هذا التدقيق.** |

---

## 4) الحالة الفعلية لطبقة Presentation (بالأدلة)

### 4.1 التمهيد والحاوية

- `App.xaml.cs` يبني الحاوية بشكل صحيح: `AddApplication()`, `AddInfrastructure(configuration)`, `AddPresentation()`, `EnsureCreatedAsync`, ثم `DefaultAdminSeeder.SeedAsync` و`DefaultSettingsSeeder.SeedAsync` مع `CancellationToken.None` — دليل: `src/MasrLab.Presentation/App.xaml.cs:29-40`.
- `MainWindow.xaml` مجرد إطار WPF بـ RTL و`Language="ar-EG"` وشبكة فارغة (`MainWindow.xaml:1-13`).
- `Presentation/DependencyInjection.cs:31-64` يسجل 30 ViewModel + `NavigationStore` + `MainWindow` + `MainViewModel` كـ Singletons/Transients.
- `appsettings.json` موجود بقيمة `DefaultConnection` **فارغة** — لن يعمل التطبيق حتى تُعبَّأ.

### 4.2 المحتوى الفعلي للـViewModels والـViews

- **كل الـViewModels تقريبًا فارغة تمامًا**: أكبر 10 ملفات ViewModel لا يتجاوز حجم كل منها **7 أسطر** فقط (`for f in $(find src/MasrLab.Presentation/ViewModels -name '*.cs'); do wc -l < "$f"; done | sort -rn | head`). المحتوى النموذجي:

  ```
  using CommunityToolkit.Mvvm.ComponentModel;
  namespace MasrLab.Presentation.ViewModels;
  public partial class LoginViewModel : ObservableObject { }
  ```
  دليل مباشر: `src/MasrLab.Presentation/ViewModels/LoginViewModel.cs` و`ViewModels/MainViewModel.cs` — كلاهما `ObservableObject` بلا خصائص/أوامر.

- **كل الـViews تقريبًا فارغة**: أكبر 8 ملفات XAML لا يتجاوز أحدها **6 أسطر** (`Grid` فارغ). دليل: `src/MasrLab.Presentation/Views/LoginView.xaml` كله 6 أسطر (`<Grid/>`)، و`Views/MainView.xaml` مثله.

- **`NavigationStore` و`NavigationService` كلاهما فارغ**: `Navigation/NavigationStore.cs` (5 أسطر) `public class NavigationStore { }`، و`Navigation/NavigationService.cs` (5 أسطر) — لا آلية تنقل فعلية.

- **Controls أساسية فارغة**: `PatientInfoCard.xaml`, `TestSelector.xaml`, `SearchFilterBar.xaml`, `PrintPreviewControl.xaml`, `IconButton.xaml` كلها `<Grid/>` بلا محتوى.

- **Converters**: 5 ملفات كلها ترمي `NotImplementedException` في `Convert`/`ConvertBack` (`AccountTypeConverter.cs:10,:15`, `GenderConverter.cs:10,:15`, `HighLowStatusConverter.cs:10,:15`, `VisitStatusConverter.cs:10,:15`, `BooleanToVisibilityConverter.cs`).

- **Printing**: `BarcodeGenerator.cs`, `EnvelopePrinter.cs`, و10 تقارير في `Printing/Reports/*` + `Templates/ReportBaseTemplate.cs` — كل منها 8 أسطر يحوي فقط `public class X { public X() { } }`. **لا تنفيذ فعلي لأي تقرير**.

### 4.3 التكامل مع Application/Infrastructure

- **صفر استهلاك لـ MediatR في Presentation**: `grep -rln "IMediator\|ISender\|MediatR" src/MasrLab.Presentation --include='*.cs'` أرجع **صفرًا**. لا ViewModel يرسل أمرًا أو استعلامًا واحدًا.
- **صفر استهلاك لـ `ICurrentUserService`**: بحث في Presentation صفري (`ICurrentUserService` مستهلَك فقط في `AuditBehavior.cs`, `CreatePatientVisitCommandHandler.cs`, و`Infrastructure/DependencyInjection.cs` و`CurrentUserService.cs`).
- **لا `INotificationHandler` في كامل `src/`**: 21 حدث دومين معرَّف في `src/MasrLab.Domain/Events/DomainEvents.cs` بلا مستهلك.
- **لا معالجة `ValidationException`** في أي مكان في Presentation.

### 4.4 ملخّص «حالة Presentation الفعلية»

طبقة Presentation عند هذا الكوميت هي **هيكل ملفات مسجَّل في DI فقط**: كل الشاشات موجودة اسميًا، الحاوية مبنيّة، لكن **لا شيء مبنيّ داخل أي شاشة** — لا UI فعلي، لا Binding، لا Command، لا استدعاء واحد للطبقات السفلية. يتعذَّر تشغيل أي وظيفة نهائية عليها اليوم.

---

## 5) تقييم ترتيب الطبقات المقترح والتصحيح

**الترتيب المقترح للتحقق:** Application → Infrastructure (بدءًا بالمصادقة) → Presentation (بدءًا بشاشة الدخول ثم شاشات التدفقات الثلاثة).

### 5.1 التحقق من ادعاء «تعذّر تنفيذ Application دون نظام دخول فعلي»

**النتيجة: الادعاء صحيح جزئيًا فقط.** الحقيقة الأدق كما رصدها الكود:

- من بين 43 معالج أمر و29 معالج استعلام، **`ICurrentUserService` مستهلَك فعليًا في معالج واحد فقط**: `CreatePatientVisitCommandHandler.cs:19` — كل بقية المعالجات التي تحتاج هوية المستخدم تستقبلها **كمعامل صريح في الأمر**:
  - `IssueReceiptCommand.cs:9` — `int ReceivedByUserId` كحقل من الأمر.
  - `EnterTestResultCommandHandler.cs:68` — يستخدم `request.EnteredByUserId`.
  - `RecordCashTransactionCommandHandler.cs:38,:42` — يستخدم `request.UserId`.
- لذلك: **Application قابل للاختبار الكامل اليوم بدون نظام دخول فعلي** — يكفي تمرير `int` مباشرة في الأوامر واختبار `ICurrentUserService` بمُزيف. الحاجب هنا ليس Application، بل Presentation، التي بلا هوية جلسة لا يمكنها ملء هذه الحقول تلقائيًا وبأمان.
- **الحاجب الفعلي لتحويل هذه الأوامر إلى مسار إنتاجي عبر الواجهة الأمامية**: زوج (`IAuthenticationService` منفَّذ + `CurrentUserService` قادر على حمل الهوية عبر الجلسة، لا `UserId => null` الحالي).

### 5.2 هل توجد تبعية تقنية تفرض ترتيبًا مختلفًا؟

- **Infrastructure/Persistence جاهزة بالفعل** (37 DbSet + 5 migrations + مستودعات + Interceptors). لا داعي لتأجيل ذلك خلف Application.
- **`AuthenticationService` وحده هو ما يحجب تشغيل UI ذي معنى** — لا حاجة لتأخير باقي خدمات Infrastructure (`PrintService`, `BarcodeService`, `BackupService`) قبل بدء Presentation.
- **Presentation بلا Login غير قابلة للاختبار الوظيفي** لأن كل شاشة تحتاج هوية جارية.
- **`PrintService` و`BarcodeService` يمكن الاعتماد عليهما بمُوك مؤقت في Presentation** حتى تُنفَّذ.

### 5.3 الترتيب المُصحَّح المعتمد في هذه الوثيقة

بناءً على الأدلة أعلاه، الترتيب المُصحَّح والأدق هو:

1. **إغلاق ثغرات Application النقطية** (S1..S3 في الفصل 6). لا حاجة لبناء تدفق جديد؛ الثغرات دقّية.
2. **إكمال Infrastructure على مرحلتين**:
   - **I‑A (حاجبة):** `AuthenticationService` + إعادة تصميم `CurrentUserService` — لأن هذين وحدهما ما يفتحان Presentation.
   - **I‑B (غير حاجبة):** `BarcodeService` → `PrintService` → `BackupService` — يمكن أن تتوازى جزئيًا مع بناء Presentation.
3. **Presentation على أربع موجات**:
   - **P‑1:** شاشة الدخول + إطار التنقّل الأدنى + `MainViewModel` نشط + معالجة `ValidationException` عالميًا.
   - **P‑2:** شاشات التدفق المالك الأول (تسجيل مريض → إنشاء زيارة → إضافة اختبارات مسعَّرة).
   - **P‑3:** شاشات التدفق الثاني (إصدار إيصال + طباعته — تعتمد على I‑B).
   - **P‑4:** شاشات التدفق الثالث (تحصيل عينات → إدخال نتائج → تسليم النتائج/التقارير — تعتمد على I‑B).

هذا يختلف عن الترتيب المطروح في نقطتين:
- **إسقاط ادعاء «Application يتعذّر تنفيذها بدون نظام دخول»** — التنفيذ تمّ فعليًا؛ ما تبقى هو تشذيب.
- **تقسيم Infrastructure إلى موجتين** (حاجبة/غير حاجبة) بدل «إكمالها كتلة واحدة قبل بدء Presentation» — يتيح توازيًا فعليًا.

---

## 6) خارطة الطريق الموحّدة المرتّبة زمنيًا

كل مرحلة قابلة للتنفيذ التدريجي (بند مغلق قبل الانتقال). كل مرحلة تنتهي بمعايير قبول واضحة قابلة للتحقق من الكود مباشرة.

### المسار S — إغلاق طبقة Application

- **S1 — توحيد عقد الأخطاء** *(يعالج A1)*
  - استبدال جميع مواضع `throw new Exception(...)` (5 مواضع في §2.4) بـ `EntityNotFoundException(nameof(Entity), id)`.
  - استبدال 12+ موضع `InvalidOperationException("... not found")` بنفس الاستثناء المجاليّ.
  - إضافة سياسة تحت `Common/` لعقد أخطاء موحّد + DecisionRecord جديد.
  - **قبول:** `grep -rn "throw new Exception(" src/MasrLab.Application` = صفر؛ عدد استخدامات `EntityNotFoundException` داخل `Features/**` ≥ 15.

- **S2 — حسم سياسة `AuditBehavior`** *(A2)*
  - قرار: (أ) الإبقاء على السلوك مع تسجيل `Warning` (كما هو)، أو (ب) رفع فشل الحفظ إلى `Error` مع تفعيل حَسّاسية مُعامِلة (فشل تدقيق طلب مالي = إحباط)، أو (ج) قناة Outbox مؤجَّلة.
  - إن اختير (ب): تعديل `AuditBehavior.cs:72-76` وإضافة اختبار.
  - **قبول:** DecisionRecord `DD-12-Audit-Persistence-Failure-Policy.md` + اختبار جديد في `AuditBehaviorTests.cs`.

- **S3 — تشذيب داخلي** *(A3, A4)*
  - حذف الحقن الميت من `PricingService.cs:11-14` (أو ربطه بالحاسبة الحقيقية).
  - حذف `Common/Models/_Placeholder.cs` و`Common/Validations/_Placeholder.cs`.
  - **قبول:** `find src/MasrLab.Application -name '*Placeholder*'` = صفر.

- **S4 — رفع تغطية الاختبارات — الجولة الأولى (المدققات)** *(A6)*
  - إضافة اختبارات مدقق لكل معالج أمر ذو قواعد غير تافهة (بدء بأوامر التدفقات المالكة الثلاثة).
  - هدف مرحلي: 20/43 مدقق بتغطية سلوك.
  - **قبول:** `find tests/MasrLab.Application.Tests -name '*ValidatorTests.cs' \| wc -l` ≥ 20.

- **S5 — رفع تغطية الاختبارات — الجولة الثانية (المعالجات الحرجة)** *(A5)*
  - كل معالج داخل التدفقات الثلاثة (F‑1, F‑2, F‑3) يحصل على اختبار سلوك (نجاح + فشل + حواف).
  - **قبول:** كل معالج مذكور في §2.2 له ملف اختبار مقابل، وإجمالي اختبارات المعالجات ≥ 20.

- **S6 — اختبار تكامل حاوية DI** *(A7)*
  - إنشاء `tests/MasrLab.Application.Tests/DIContainerResolutionTests.cs`:
    ```csharp
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(Mock.Of<IConfiguration>());
    services.AddApplication();
    // (اختياري: إضافة بدائل مُزيَّفة لكل خدمات Infrastructure).
    using var sp = services.BuildServiceProvider();
    foreach (var handlerType in typeof(CreatePatientVisitCommand).Assembly
        .GetTypes().Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))))
    { sp.GetRequiredService(handlerType); }
    ```
  - **قبول:** الاختبار يفشل إذا نقصت أي تبعية.

- **S7 — إثبات تنفيذي على بيئة `dotnet 8 SDK`** *(يعالج قيد التدقيق)*
  - `dotnet restore && dotnet build -c Release` بلا تحذيرات + `dotnet test` أخضر بالكامل.
  - **قبول:** لقطة CI أو سجل تنفيذ يُودَع في DecisionRecord.

### المسار I — إكمال Infrastructure

- **I‑A1 — تنفيذ `AuthenticationService.LoginAsync`** *(I1)*
  - جلب `User` عبر `IRepository<User>` باستخدام `Username`؛ تحقق كلمة مرور مُجزَّأة (BCrypt.Net أو `Microsoft.AspNetCore.Cryptography.KeyDerivation`)؛ جلب `Permission[]` عبر `IPermissionRepository`.
  - إرجاع `new AuthResult(user.Id, user.Username, perms.Select(p => $"{p.ScreenType}:{p.PermissionOperation}").ToArray())`.
  - **قبول:** `grep -c NotImplementedException src/MasrLab.Infrastructure/Services/AuthenticationService.cs` = 0؛ اختبارات وحدة على `LoginAsync` (نجاح، كلمة خطأ، مستخدم غير موجود، غير نشط).

- **I‑A2 — تنفيذ `AuthenticationService.LogoutAsync`** *(I1)*
  - يعتمد على معالج `RecordLogoutCommand` القائم إن كان مناسبًا، وإلا يكتب سجل خروج في `IAttendanceLogRepository`.
  - **قبول:** لا `NotImplementedException` متبقٍّ في الملف.

- **I‑A3 — إعادة تصميم `CurrentUserService`** *(I2)*
  - نقلها إلى نمط `AmbientContext`: حقل قابل للتعيين مرة واحدة عبر `Set(int userId)`، مسجَّل `Scoped` وربطه بجلسة التطبيق (WPF: `App.CurrentSessionScope`).
  - **قبول:** `CurrentUserService.cs` لا يعيد `null` بعد نجاح Login؛ اختبار سلوك: عندما تُدعى `Set(42)` يقرأ `AuditBehavior` رقم 42.

- **I‑A4 — اختبار سيناريو Login كامل**: من الأمر إلى الحاوية إلى `CurrentUserService`.
  - **قبول:** اختبار يُثبت أن `CreatePatientVisitCommandHandler.cs:44` لا يرمي بعد تسجيل الدخول.

*(يكفي إنجاز I‑A1..I‑A4 لإطلاق Presentation P‑1..P‑2.)*

- **I‑B1 — تنفيذ `BarcodeService`** *(I4)*
  - اقتراح: ZXing.Net (Code128 افتراضيًا) — تحويل النتيجة إلى `byte[]` PNG.
  - **قبول:** `NotImplementedException` = 0 في الملف؛ اختبار وحدة يولّد صورة ويقرؤها.

- **I‑B2 — تنفيذ `PrintService`** *(I3)*
  - `RenderAsync(reportName, payload, ct)`: تشعّب حسب `reportName` إلى مولّد QuestPDF/RDLC؛ إرجاع `byte[]` PDF.
  - `PrintAsync`: إرسال الـPDF إلى الطابعة الافتراضية أو الطابعة المسمّاة عبر Windows Printing.
  - **قبول:** اختبارات وحدة لـ`RenderAsync` على تقرير واحد على الأقل (Receipt).

- **I‑B3 — تنفيذ `BackupService`** *(I5)*
  - `BackupAsync(filePath)`: تنفيذ `BACKUP DATABASE ... TO DISK = @path`.
  - `RestoreAsync(filePath)`: تنفيذ `RESTORE DATABASE` + التعامل مع اتصال حَصْري.
  - **قبول:** اختبارات تكامل تُتخطى إذا LocalDB غير متاح (نفس نمط `LocalDbAvailability`).

- **I‑B4 — تسمية ملفات الاختبار المضلِّلة** *(I6)*
  - إعادة تسمية `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs` → ملفات موصوفة (بحسب الفئات: `GenericRepositoryTests.cs`, `UnitOfWorkTests.cs`, `PatientRepositoryTests.cs`).
  - إعادة تسمية `tests/MasrLab.Domain.Tests/PlaceholderTests.cs` → `BaseEntityTests.cs`, `EnumTests.cs`, `EntityCreationTests.cs`.
  - **قبول:** `find tests -name '*Placeholder*'` = صفر.

- **I‑B5 — اختبار تكامل حاوية Infrastructure الكاملة** *(I7)*
  - `AddInfrastructure(configuration)` + كل الحقن يحلّ.
  - **قبول:** اختبار جديد في `tests/MasrLab.Infrastructure.Tests`.

- **I‑B6 — تحقق ميجريشن مقابل `Docs/Future-Migrations.md`** *(I8)*
  - `dotnet ef migrations script` وتوثيق أي انحراف.
  - **قبول:** DecisionRecord يوثِّق الانحرافات (إن وُجدت) وقرار المعالجة.

### المسار P — بناء Presentation على أربع موجات

- **P‑1 — الأساس + الدخول** *(معتمد على I‑A كاملًا)*
  - `LoginView.xaml` + `LoginViewModel` مع `RelayCommand LoginAsync` يستدعي `IMediator.Send(new LoginQueryOrCommand(...))` أو مباشرة `IAuthenticationService`.
  - عند النجاح: `CurrentUserService.Set(userId)` + `NavigationStore.CurrentViewModel = shellHost`.
  - `NavigationStore` و`NavigationService` يُنفَّذان (استبدال 5 أسطر فارغة بمنطق فعلي).
  - معالج `ValidationException` عالمي في `App.xaml.cs` (`DispatcherUnhandledException`) يعرض الأخطاء للمستخدم.
  - `Converters` الخمسة تُنفَّذ (استبدال `NotImplementedException`).
  - **قبول:** بعد Login بنجاح تُعرض نافذة رئيسية غير فارغة تحوي قائمة تنقّل واحدة على الأقل.

- **P‑2 — التدفق المالك الأول: تسجيل مريض → زيارة → اختبارات مسعَّرة**
  - `RegisterPatientView` + `RegisterPatientViewModel` يستدعي `RegisterPatientCommand`.
  - `SearchPatientsView` + `VisitHistoryView` — يستهلكان استعلاماتهما القائمة.
  - `PatientInfoCard` control يُنفَّذ (اليوم `<Grid/>`).
  - `TestSelector` control يُنفَّذ ويستدعي `AddTestToVisitCommand`.
  - **قبول:** يمكن يدويًا استكمال دورة: تسجيل مريض → إنشاء زيارة → إضافة اختبار من واجهة حقيقية.

- **P‑3 — التدفق الثاني: إصدار الإيصال وطباعته** *(يعتمد على I‑B1, I‑B2)*
  - `PriceListsView`, `AccountTypeDrawerView`, `DoctorReferralDrawerView`, `PeriodDrawerView`.
  - `IssueReceipt` من ViewModel مناسب يستدعي `IssueReceiptCommand`.
  - `ReceiptReport` في `Printing/Reports/ReceiptReport.cs` يُنفَّذ (اليوم 8 أسطر فارغة).
  - `PrintPreviewControl` يُنفَّذ ويستخدم `IPrintService.RenderAsync`.
  - **قبول:** طباعة إيصال حقيقي (PDF مُصدَّر) من الواجهة.

- **P‑4 — التدفق الثالث: تحصيل العينات → إدخال النتائج → تسليم النتائج** *(يعتمد على I‑B2 لتقارير النتائج)*
  - `SampleCollectionView` + `MarkSampleCollectedCommand`.
  - `EnterResultsView` + `EnterTestResultCommand` (يمرر `EnteredByUserId` من `ICurrentUserService`).
  - `BlankReportView`, `CombinedReportView`, `DeliverResultsView`.
  - `IndividualResultReport`, `CombinedReport`, `BlankReport` في `Printing/Reports` تُنفَّذ.
  - **قبول:** استكمال يدوي لدورة كاملة من مريض جديد إلى تسليم تقرير مطبوع.

- **P‑5 — التغطية الجانبية**
  - شاشات إدارة (`UsersPermissionsView`, `SystemSettingsView`, `TestsMasterDataView`, `TestGroupsView`, `PriceListsView`, `DoctorsReferralsView`, `FixedCommentsView`).
  - شاشات إحصاء وتتبع (`StatisticsView`, `PatientHistoryView`, `CasesFollowUpView`, `AttendanceAuditView`, `OutsourcedSamplesView`, `WorkSheetsView`, `AddCultureView`, `AddAntibioticView`, `CultureResultView`).
  - كل شاشة تُربط بمعالجها الموجود في Application (الأوامر/الاستعلامات جاهزة).
  - **قبول:** لا شاشة في `Views/**` تظل `<Grid/>` فارغة.

- **P‑6 — النسخ الاحتياطي واستعادة** *(يعتمد على I‑B3)*
  - شاشة داخل `SystemSettingsView` تستدعي `ManageBackupCommand`.
  - **قبول:** يمكن أخذ نسخة احتياطية واستعادتها من الواجهة.

- **P‑7 — أحداث المجال (اختياري)**
  - إذا اعتُمد تفعيل الأحداث: إضافة `INotificationHandler<PatientVisitCreated>`… إلخ.
  - **قبول:** بحث `INotificationHandler` في `src/` ≥ 1.

---

## 7) تقييم الجاهزية النهائية لكل طبقة عند هذا الكوميت

| الطبقة | تصنيف الجاهزية | تفسير مختصر |
|---|---|---|
| **Domain** | ✅ ناضجة (ساكنًا) | 167 اختبارًا (بعضها بأسماء ملفات مضلِّلة تعالجها I‑B4). كيانات، Value Objects، Domain Services، Exceptions، Events كلها موجودة ومختبَرة. لم تُطلب معالجة إضافية في هذه الوثيقة. |
| **Application** | 🟡 صالحة بنيويًا، ناقصة في: عقد الأخطاء (S1)، سياسة التدقيق (S2)، تشذيب Placeholder والحقن الميت (S3)، تغطية اختبار المعالجات/المدققين (S4, S5)، اختبار تكامل الحاوية (S6)، إثبات تنفيذي (S7). | 72 معالجًا + 43 مدققًا + 10 خدمات مسجَّلة. لا مسار حرج مسدود، فقط ثغرات جودة وقيود عقد. |
| **Infrastructure/Persistence** | ✅ جاهزة ساكنًا | 37 DbSet + 30 Configuration + 5 migrations متسلسلة + Snapshot متزامن + Interceptors + مستودعات. يحتاج تحقق ميجريشن تنفيذي (I‑B6). |
| **Infrastructure/Services** | ❌ **حاجبة**: 4 خدمات NotImplemented + `CurrentUserService.UserId => null`. | الحاجب الوحيد لتشغيل UI ذي معنى هو I‑A. |
| **Presentation** | ❌ **هيكل فقط**: 30 ViewModel و30 View فارغة، `NavigationStore`/`NavigationService`/Controls/Printing كلها ستَبات، صفر MediatR في الطبقة. | يبدأ العمل الحقيقي فيها بعد إغلاق I‑A. |

**التقدير الإجمالي:** المشروع كامل الحدود المعمارية ومكتمل بمعظم منطق الأعمال في Application، لكنه على مستوى المستخدم النهائي **غير قابل للتشغيل الإنتاجي** لسببين محددَين قابلين للتصحيح:
1. غياب تنفيذ `AuthenticationService` + هوية جلسة حقيقية في `CurrentUserService`.
2. Presentation فارغة.

الترتيب المُصحَّح في §5.3 يحوّل هذين السببين إلى مسار عمل واضح بأربع موجات UI فوق موجتَي Infrastructure، بعد إغلاق سبع نقاط تشذيب في Application.

---

## ملحق: قائمة الأدلة المرجعية (منتقاة)

| الادعاء | الدليل الحرفي |
|---|---|
| العدد الحقيقي للمعالجات | `find src/MasrLab.Application/Features -name '*Handler.cs' \| wc -l` → 72 |
| العدد الحقيقي للمدققات | `find src/MasrLab.Application/Features -name '*Validator.cs' \| wc -l` → 43 |
| ابتلاع فشل التدقيق | `AuditBehavior.cs:72-76` |
| `CurrentUserService.UserId => null` | `src/MasrLab.Infrastructure/Services/CurrentUserService.cs:7` |
| 4 خدمات Infrastructure NotImplemented | `AuthenticationService.cs:10,15`; `BackupService.cs:9,14`; `BarcodeService.cs:9`; `PrintService.cs:9,14` |
| صفر MediatR في Presentation | `grep -rln "IMediator\|ISender\|MediatR" src/MasrLab.Presentation --include='*.cs'` → صفر |
| ViewModels فارغة (7 أسطر) | `wc -l src/MasrLab.Presentation/ViewModels/LoginViewModel.cs` → 7 |
| Views فارغة (`<Grid/>`) | `src/MasrLab.Presentation/Views/LoginView.xaml` كله 6 أسطر |
| Converters ترمي NotImplemented | `src/MasrLab.Presentation/Resources/Converters/AccountTypeConverter.cs:10,15` وأخواتها |
| Placeholder Application قائم | `src/MasrLab.Application/Common/Models/_Placeholder.cs`, `src/MasrLab.Application/Common/Validations/_Placeholder.cs` |
| ملفات اختبار مضلِّلة | `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs`, `tests/MasrLab.Domain.Tests/PlaceholderTests.cs` |
| 5 هجرات + Snapshot متزامن مع OverrideReason | `ls src/MasrLab.Infrastructure/Persistence/Migrations/` + `grep -c OverrideReason … Snapshot.cs` → 1 |
| Application يعتمد على ICurrentUserService في معالج واحد فقط | `grep -rln ICurrentUserService src/MasrLab.Application/Features` → `CreatePatientVisitCommandHandler.cs` |
| IssueReceipt يستقبل UserId عبر الأمر | `src/MasrLab.Application/Features/PatientVisits/Commands/IssueReceipt/IssueReceiptCommand.cs:9` (`int ReceivedByUserId`) |
| EntityNotFoundException غير مستعمل في Application | `grep -rn EntityNotFoundException src/MasrLab.Application` → صفر |
| `throw new Exception` في 5 معالجات | §2.4 |
| اختبار تكامل حاوية DI غير موجود | `grep -rln "AddApplication\|AddInfrastructure\|BuildServiceProvider" tests --include='*.cs'` → صفر |
| قيد بيئي: `dotnet` غير متاح | `which dotnet` → لا شيء |

