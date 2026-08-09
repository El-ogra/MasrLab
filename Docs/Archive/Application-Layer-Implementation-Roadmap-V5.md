# MasrLab — تدقيق مستقل لطبقة Application وخارطة طريق التنفيذ

**المشروع:** MasrLab — نظام إدارة معامل التحاليل الطبية
**المكدّس:** .NET 8 · WPF · MVVM · Clean Architecture · MediatR · AutoMapper · FluentValidation · EF Core (SQL Server)
**نقطة التدقيق الوحيدة (Pinned Commit):** `0773d6dd3c7fd83e4d770032fd1106f34c775f14` — الرسالة: «تنفيذ المرحلة الثالثة من النسخة الرابعة» — الفرع: `niamod`
**حالة الوثيقة:** وثيقة مستقلة تمامًا (self-contained) مبنية على تدقيق مباشر للكود عند الكوميت المثبَّت أعلاه. كل معرّف مستخدم في هذه الوثيقة مُعرَّف داخلها، ولا تفترض الوثيقة أي مرجع خارجي آخر.

---

## 0. منهجية التدقيق وقيد الأدلة

- ثُبِّتت شجرة العمل صراحةً على الهاش `0773d6dd3c7fd83e4d770032fd1106f34c775f14` (`git checkout <hash>`، `HEAD is now at 0773d6d`، وشجرة العمل نظيفة: `git status --porcelain` أعاد صفر أسطر) قبل أي فحص. لم يُبنَ أي استنتاج على أحدث حالة عائمة للفرع.
- كل رقم سطر في هذه الوثيقة يخص ملفات الكوميت المُدقَّق عليه حصرًا. عبارة «غياب» تعني نتيجة بحث محددة (`grep` موجّه) ضمن المسار المذكور، لا ادعاء عن بيئات أو مستودعات أخرى.
- **قيد تشغيلي صريح:** لم يكن تنفيذ `dotnet build` أو `dotnet test` ممكنًا في بيئة هذا التدقيق لأن أمر `dotnet` غير مثبَّت (`which dotnet` → not found). لذلك كل حكم يتعلق بالبناء أو بنجاح الاختبارات في هذه الوثيقة هو **حكم ساكن (غير مُثبت تنفيذيًا) بعد**، وقد وُسم بذلك حيثما ورد.

### تعريف المصطلحات المستخدمة في هذه الوثيقة

| المعرّف | التعريف |
|---|---|
| **المرحلة 1** (تدقيق سابق) | إغلاق سلامة قواعد الأعمال غير الموصولة: ربط الخدمات الخاملة بمستهلكات CQRS، وإغلاق خطر «السعر الصفري الصامت». |
| **المرحلة 2** (تدقيق سابق) | تثبيت سياسة Mapping الموحدة وسياسة فشل التدقيق (Audit). |
| **المرحلة 3** (تدقيق سابق) | متانة التزامن والإلغاء: سدّ سباق توليد Lab ID، ومنع الاستعلامات الثقيلة والحفظ بلا رمز إلغاء. |
| **المرحلة 4** (تدقيق سابق) | حزمة الاختبارات والتكامل: اختبارات معالجات/خدمات/مدققين/تكامل وبيئة بناء قابلة للتكرار. |
| **المرحلة 5** (تدقيق سابق) | حدود Presentation والأحداث والتوثيق: استهلاك MediatR من الواجهة، عقد أخطاء موحّد، معالجات الأحداث، إزالة آثار scaffolding. |
| **F-1 / F-2 / F-3** | التدفقات المالكة الثلاثة المدروسة في هذه الوثيقة: إنشاء زيارة مريض (F-1)، إضافة اختبار لزيارة مع تسعير (F-2)، إصدار إيصال (F-3). |
| **حكم ساكن** | استنتاج مبني على قراءة الكود فقط دون تشغيل البناء أو الاختبارات. |

---

## 1. الملخص التنفيذي

طبقة `src/MasrLab.Application` عند هذا الكوميت تحتوي **68 معالجًا (39 أمرًا + 29 استعلامًا)، و39 مدقق FluentValidation، و49 DTO، و9 ملفات Profile لـ AutoMapper تحوي 12 خريطة `CreateMap<>`**، والتسجيل المركزي للحاوية مكتمل بنيويًا (`src/MasrLab.Application/DependencyInjection.cs:13-37`). لا يوجد `NotImplementedException` ولا `GetAwaiter().GetResult()` ولا استيراد لـ `MasrLab.Infrastructure` داخل الطبقة (بحث صفري في `src/MasrLab.Application`).

مقارنةً بنقطة التدقيق السابقة، أنجزت الكوميتات الثلاث الأخيرة (`cff25bc`, `cd3b286`, `0773d6d`) إغلاقات جوهرية حقيقية ومُثبتة بالكود:

1. **أُغلق خطر «السعر الصفري الصامت» نهائيًا:** غياب بند السعر أصبح استثناء مجال صريحًا (`src/MasrLab.Application/Services/PriceListResolverService.cs:32-34`) بدل إعادة `0`.
2. **أُغلق سباق توليد Lab ID بثلاث طبقات دفاع:** فهرس فريد على `Patient.LabId` (`src/MasrLab.Infrastructure/Persistence/Configurations/Core/PatientConfiguration.cs:51`)، وترجمة خطأ 2601/2627 إلى `DuplicateLabIdException` (`src/MasrLab.Infrastructure/Persistence/UnitOfWork.cs:24-26`)، وحلقة إعادة محاولة مع backoff في المعالج (`src/MasrLab.Application/Features/PatientManagement/Commands/RegisterPatient/RegisterPatientCommandHandler.cs:57-70`)، مع اختبارَي تكامل LocalDB (`tests/MasrLab.Infrastructure.Tests/LabIdConcurrencyIntegrationTests.cs`, `LabIdUniqueIndexIntegrationTests.cs`).
3. **ارتفعت تغطية اختبارات Application من 14 إلى 55 سمة `[Fact]/[Theory]` موزعة على 13 ملفًا** (عدّ آلي على الكوميت المُدقَّق)، واختبارات Infrastructure من ملف placeholder واحد إلى 8 ملفات تحوي 20 سمة اختبار، مع آلية تخطٍّ ذكية عند غياب LocalDB (`tests/MasrLab.Infrastructure.Tests/LocalDbTestInfrastructure.cs:44-52`).
4. **حُسمت سياسة Mapping:** 14 من 29 معالج استعلام تستخدم `IMapper`، و14 معالجًا يبني DTO يدويًا **بتعليقات توثيقية صريحة تشرح سبب الاستثناء** (مثل `src/MasrLab.Application/Features/PatientHistory/Queries/GetPatientHistory/GetPatientHistoryQueryHandler.cs:21-23`)، مع حذف 5 profiles غير مستعملة وإضافة 14 اختبارًا للخرائط.
5. **أُدخلت حراسات أعمال جديدة لم تكن موصوفة سابقًا:** حارس العينة غير المُجمَّعة قبل إدخال النتيجة مع مسار تجاوز موثَّق (`EnterTestResultCommandHandler.cs:46-56`)، وربط `ISampleTrackingService` بأول مستهلك فعلي (`:16,:25,:46-49`)، وربط `IReferralCommissionService` بخدمة المحاسبة (`src/MasrLab.Application/Services/AccountingService.cs:16,:21,:67`)، وإعادة تعريف صيغة صافي الربح رسميًا كـ `TotalIncome − TotalDiscount − CommissionsTotal` (`AccountingService.cs:32-35`).

لكن التدقيق يُثبت أيضًا أن **الفجوة المالكة الجوهرية ما زالت قائمة بالكامل**: لا يوجد أي أمر CQRS في `Features/**` ينشئ `PatientVisit`، ولا أي استدعاء إنتاجي لـ `PatientVisit.AddTest` خارج الكيان، ولا أي معالج ينشئ أو يقرأ أو يحدّث كيان `Receipt`. الأثر الوحيد لاستخدام هذه الكيانات هو في الاختبارات وفي معالجات قراءة (`GetPatientVisitHistory`, `GetPendingVisits`... إلخ). النظام عند هذا الكوميت **لا يملك مسارًا وظيفيًا يُدخل مريضًا إلى دورة زيارة/فحص/إيصال** رغم نضج كيانات Domain المقابلة. كما بقيت ثلاث خدمات Domain بلا مستهلك (`IPriceListResolverService`, `IPricingService`, `IReceiptCalculationService` — بحث صفري داخل `src/MasrLab.Application/Features/`)، وخدمة `PricingService` تحمل حقنًا ميتًا للـ resolver (`src/MasrLab.Application/Services/PricingService.cs:11-14` مقابل `:20-34`).

**الحكم المختصر:** طبقة Application صالحة ومتماسكة كبنية وكعقود، لكنها ليست مكتملة وظيفيًا ولا جاهزة إنتاجيًا؛ الأولوية القصوى هي بناء التدفقات المالكة الثلاثة (F-1 → F-2 → F-3) بعد حسم سؤال أعمال واحد يخص منطق التراجع بين قوائم الأسعار.

---

## 2. تقييم حالة المراحل الخمس مع الأدلة

> الجدول التالي يعيد تقييم كل مرحلة وبنودها التفصيلية على الكود الحي. الحالات: ✅ مكتمل / 🟡 جزئي / ❌ لم يتغير.

### المرحلة 1 — إغلاق سلامة قواعد الأعمال غير الموصولة — الحكم: 🟡 **جزئية (تقدّم جوهري مُثبت)**

| البند التفصيلي | الحالة | الدليل |
|---|---|---|
| تمييز «بند سعر غير موجود» صراحةً بدل إعادة 0 | ✅ مكتمل | `PriceListResolverService.cs:32-34` يرمي `BusinessRuleViolationException` عند غياب البند، و`:36` يعيد `item.Price` فقط لبند موجود؛ العقد موثق في `:20-24`. |
| رفض `priceListId <= 0` | ✅ مكتمل | `PriceListResolverService.cs:27-28`. |
| رفض الأسعار السالبة عند تعديل قائمة الأسعار | ✅ مكتمل (بند جديد لم يكن موصوفًا صراحة في المرحلة) | `UpdatePriceListItemsCommandValidator.cs:12-14` — `RuleForEach(...).Must(item => item.Price >= 0)`. |
| ربط `IReferralCommissionService` بمستهلك | ✅ مكتمل (غير مباشر ومبرَّر) | `AccountingService.cs:16,:21,:25,:67` — الخدمة تُستهلك داخل `RecalculateNetProfitAsync`، والمحاسبة نفسها مستهلكة في `Features/Accounting/Commands/RecordCashTransaction/RecordCashTransactionCommandHandler.cs:15,:51`. |
| ربط `ISampleTrackingService` بمستهلك | ✅ مكتمل (توسّع نطاق: جاء ضمن حارس العينة) | `EnterTestResultCommandHandler.cs:16,:25,:46-49`. |
| ربط `IPriceListResolverService` / `IPricingService` / `IReceiptCalculationService` بمستهلك Features | ❌ لم يتغير | بحث أسماء الواجهات الثلاث داخل `src/MasrLab.Application/Features/` أعاد **صفر نتائج**. التسجيل موجود (`DependencyInjection.cs:29-31`) لكن لا استهلاك وظيفي. |
| إزالة الحقن الميت في `PricingService` | ❌ لم يتغير | `PricingService.cs:11-14` يحقن `IPriceListResolverService` دون أي استخدام في `CalculateSubtotal/CalculateTotal` (`:20-34`). |

**فوارق مكتشَقة بالتدقيق (توسّع نطاق أثناء التنفيذ):**
- **حارس العينة غير المُجمَّعة** (جديد كليًا): رفض إدخال نتيجة لفحص عينته غير مُجمَّعة ما لم يُقدَّم سبب تجاوز غير فارغ (`EnterTestResultCommandHandler.cs:51-56`)، مع تخزين السبب دائمًا مع النتيجة في عمود جديد `TestResult.OverrideReason` (`src/MasrLab.Domain/Entities/Core/TestResult.cs:18-22`) وميجريشن جديدة `20260808223709_AddOverrideReasonToTestResult` (`src/MasrLab.Infrastructure/Persistence/Migrations/20260808223709_AddOverrideReasonToTestResult.cs:13-18`).
- **تغيير صيغة صافي الربح رسميًا:** استُبدلت المعادلة الموثقة الأوسع (`− TotalOutsourcedCost − CashWithdrawals + CashDeposits`) بـ `TotalIncome − TotalDiscount − CommissionsTotal` (`AccountingService.cs:32-35`، تعليق `src/MasrLab.Domain/Entities/Financial/Account.cs:12-14`، وتغيير توقيع العقد `src/MasrLab.Domain/Services/IAccountingService.cs:7`). **هذا قرار أعمال مؤثر لم يُعتمد خارجيًا حسب سجل القرارات الموجود في المستودع، وهو يحتاج اعتمادًا صريحًا** (انظر قسم الأسئلة المفتوحة، Q-2/Q-3).
- **منطق التراجع بين قوائم أسعار متعددة غير موجود:** `ResolvePriceAsync` محلّل لقائمة واحدة فقط (`PriceListResolverService.cs:25-37`)، والبحث عن أي آلية fallback أو ترتيب قوائم في `src/MasrLab.Application` و`src/MasrLab.Domain` أعاد صفر نتائج. أي حل مزدوج «قائمة النوع ← قائمة المعمل» غير مُنفَّذ وغير مطبَّق — انظر Q-1.

### المرحلة 2 — سياسة Mapping والتدقيق — الحكم: 🟡 **جزئية (Mapping محسومة بشروط، سياسة فشل Audit لم تُحسم)**

| البند | الحالة | الدليل |
|---|---|---|
| اختيار قاعدة صريحة للـ mapping | ✅ مكتمل توثيقيًا | القاعدة الظاهرة في الكود: «AutoMapper للتحويلات البسيطة + استثناء موثَّق بتعليق للتحويلات المركّبة». الأمثلة: `GetPatientHistoryQueryHandler.cs:21-23`، `GenerateTestLogQueryHandler.cs:41`، `GetPatientByIdQueryHandler.cs:12-14,:26`. حُذفت 5 profiles غير مستعملة (Accounting/DoctorReferral/Receipt/Settings/UsersPermissions) — بقيت 9 ملفات profile بـ 12 خريطة فقط مقابل 49 DTO. |
| حماية صحة التهيئة | ✅ مكتمل | `tests/MasrLab.Application.Tests/MappingConfigurationTests.cs:10-17` (`AssertConfigurationIsValid()`) + 13 اختبارًا تفصيليًا في `tests/MasrLab.Application.Tests/MappingProfileTests.cs`. |
| اختبارات AuditBehavior | ✅ مكتمل (وحدات) | `tests/MasrLab.Application.Tests/AuditBehaviorTests.cs` (3 اختبارات، Moq على `IRequestAuditLogRepository`/`IUnitOfWork`). |
| حسم سياسة «فشل حفظ التدقيق لا يُسقط الطلب» | 🟡 لم تُحسم | ما زال فشل الحفظ يُبتلع مع تحذير سجل فقط: `src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs:72-76`. أصبحت السياسة الآن **مختبَرة** لكنها غير موثقة كقرار أعمال ولا يقابلها مسار مراقبة/بديل. |
| audit يغطي نجاح/فشل الطلب ويعيد رمي الاستثناء | ✅ مكتمل ساكنًا | `AuditBehavior.cs:38-51`. |

### المرحلة 3 — متانة التزامن LabId والإلغاء — الحكم: ✅ **مكتملة ساكنًا (تجاوزت الحد الأدنى المطلوب)**

| البند | الحالة | الدليل |
|---|---|---|
| ضمان تفرّد Lab ID في Infrastructure | ✅ مكتمل | فهرس فريد: `PatientConfiguration.cs:51` (`HasIndex(e => e.LabId).IsUnique()`)، وميجريشن مخصصة `20260805033329_AddUniquePatientLabIdIndex`. ملاحظة دقيقة: `PatientVisit.LabId` مفهرَس لكنه **غير فريد** (`PatientVisitConfiguration.cs:23`) — مقبول لأن التفرّد المطلوب على المريض، لكنه قيد يجب توثيقه عند بناء F-1 (انظر §4.2). |
| آلية retry على التصادم | ✅ مكتمل | `UnitOfWork.cs:24-26` يرمي `DuplicateLabIdException` على خطأ SQL ‏2601/2627 المرتبط بكيان `Patient`، و`RegisterPatientCommandHandler.cs:57-70` يعيد التوليد حتى 5 محاولات مع backoff أسّي + jitter. |
| اختبار توليد متوازٍ | ✅ موجود (LocalDB-gated) | `tests/MasrLab.Infrastructure.Tests/LabIdConcurrencyIntegrationTests.cs:16-77` (12 تسجيلًا متزامنًا، يتحقق من التفرّد ومعدل نجاح ≥ 6/12)، و`LabIdUniqueIndexIntegrationTests.cs`. يُتخطَّى تلقائيًا عند غياب LocalDB (`LocalDbTestInfrastructure.cs:44-52`) — أي أن نجاحه ليس مُثبتًا إلا على بيئة Windows/LocalDB. |
| منع `GetAllAsync` غير المقيد و`SaveChangesAsync` بلا CT | ✅ مكتمل ساكنًا | بحث صفري في `src/MasrLab.Application`؛ مثال الحفظ: `EnterTestResultCommandHandler.cs:75-76`. |
| تمرير CT إلى نقاط seeding | ✅ مكتمل (توسّع نطاق) | `src/MasrLab.Presentation/App.xaml.cs:38-39` يمرّر `CancellationToken.None` صراحة إلى `DefaultAdminSeeder.SeedAsync` و`DefaultSettingsSeeder.SeedAsync` بعد إضافة معامل CT لكليهما. |

### المرحلة 4 — حزمة الاختبارات والتكامل — الحكم: 🟡 **جزئية (قفزة كمّية حقيقية، لكن بلا إثبات تشغيلي وبثغرات نوعية)**

| البند | الحالة | الدليل |
|---|---|---|
| اختبارات معالجات (happy/failure) | 🟡 بدأت | ملفان فقط يختبران معالجات: `EnterTestResultCommandHandlerTests.cs` (6) و`GetPatientHistoryQueryHandlerTests.cs` (1) من أصل 68 معالجًا. |
| اختبارات خدمات Domain | 🟡 بدأت | 3 من 10 خدمات مختبَرة: `AccountingServiceTests.cs` (7)، `ReferralCommissionServiceTests.cs` (5)، `PriceListResolverServiceTests.cs` (4). لا اختبارات لـ Pricing/ReceiptCalculation/SampleTracking/CultureSensitivity/Outsourcing/MedicalHistory/ResultValidation في `tests/MasrLab.Application.Tests/`. |
| اختبارات مدققين | 🟡 بدأت | 4 ملفات مدققين (`AddTest`, `CreateUser`, `RegisterPatient`, `UpdatePriceListItems`) من أصل 39 مدققًا. |
| اختبارات السلوكيات (Behaviors) | ✅ للسلوكين | `ValidationBehaviorTests.cs` (3)، `AuditBehaviorTests.cs` (3). |
| اختبارات تكامل الحاوية `AddApplication + AddInfrastructure` | ❌ غير موجودة | لا ملف DI-resolution/integration في `tests/`؛ اختبارات Infrastructure الجديدة هي اختبارات مستودعات/تزامن LocalDB لا اختبارات حاوية كاملة. |
| تسمية ملف الاختبارات المضللة | ✅ أُصلحت في Application | `tests/MasrLab.Application.Tests/PlaceholderTests.cs` **حُذف**؛ لكن `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs` ما زال موجودًا ويحوي 9 سمات اختبار — تبقى إعادة التسمية هناك. |
| إثبات بناء/تشغيل في بيئة التدقيق | ❌ غير مُثبت | `dotnet` غير مثبَّت في بيئة هذا التدقيق؛ كل أحكام البناء **غير مُثبتة تنفيذيًا**. يوجد في المستودع سجل قرارات يتضمن خرج تشغيل سابقًا يزعم 213/213 ناجحًا على جهاز Windows — يُعامل كقرينة خارجية لا كدليل تدقيق مستقل. |

**العدّ الآلي عند الكوميت المُدقَّق:** Application.Tests = 55 سمة `[Fact]/[Theory]` في 13 ملفًا؛ Infrastructure.Tests = 20 سمة في 8 ملفات (تشمل 9 في placeholder باقٍ)؛ Domain.Tests لم تتغير في الكوميتات الثلاث الأخيرة.

### المرحلة 5 — حدود Presentation والأحداث والتوثيق — الحكم: ❌ **لم يتغير (باستثناء لمسة CT)**

| البند | الحالة | الدليل |
|---|---|---|
| استهلاك MediatR من ViewModels | ❌ | بحث `MediatR|ISender|IMediator` في `src/MasrLab.Presentation` أعاد صفر نتائج؛ `src/MasrLab.Presentation/DependencyInjection.cs:29-64` يسجّل 26 ViewModel دون أي حقن لوسيط. |
| معالجة `ValidationException`/استثناءات المجال عند حد UI | ❌ | بحث `ValidationException` في `src/MasrLab.Presentation` صفري. |
| معالجات أحداث المجال (`INotificationHandler`) | ❌ | بحث `INotificationHandler` في كامل `src/` صفري، رغم وجود 21 حدث مجال معرَّفًا (`src/MasrLab.Domain/Events/DomainEvents.cs:8-46`)، وثلاثة منها تُطلَق من كيانات مركزية في التدفقات المالكة (`PatientVisitCreated`, `VisitTestAdded`, `ReceiptIssued`). |
| خدمات Infrastructure stubs | ❌ لم تتغير | 7 رميات `NotImplementedException`: `AuthenticationService.cs:10,:15`، `BackupService.cs:9,:14`، `BarcodeService.cs:9`، `PrintService.cs:9,:14`. |
| placeholders | ❌ لم تتغير | `src/MasrLab.Application/Common/Models/_Placeholder.cs` و`Common/Validations/_Placeholder.cs` باقيان بنص يحيل إلى خطة قديمة (`:1-6` في كل منهما). لا قواعد تحقق مشتركة ولا عقد `Result<T>`/أخطاء موحّد. |
| CT إلى seeders | ✅ (التغيير الوحيد المسجَّل لهذه المرحلة) | `App.xaml.cs:38-39`. |

### فوارق أخرى اكتشفها التدقيق ولم تُوصف سابقًا

1. **مُحوّلات WPF أربعة غير مُنفَّذة** (لطالما كانت stubs منذ الكوميت الأولي، لكن لم تُوثَّق كدَين في أي تقييم سابق): `HighLowStatusConverter.cs:10,:15`، `GenderConverter.cs:10,:15`، `AccountTypeConverter.cs:10,:15`، `VisitStatusConverter.cs:10,:15` — كلها `NotImplementedException` في Presentation، وستظهر فور ربط أي شاشة ببيانات حقيقية.
2. **ميلاد `GetVisitTestAsync` كعقد قراءة خفيف** بلا تتبّع (`IVisitRepository.cs:7` ↔ `VisitRepository.cs:16-21` مع `AsNoTracking`) — أضيف لدعم حارس العينة، وهو مناسب للقراءة لكنه غير صالح كمسار تحميل للكيان عند الحاجة لتعديله (مهم لتصميم F-2: يجب جلب `PatientVisit` المتتبَّع عبر `IRepository<PatientVisit>.GetByIdAsync` لا عبر هذا العقد).
3. **إصلاح دقّة كشف خطأ التكرار** في `UnitOfWork.cs:32-38`: صار يسمح بدخول الكيانات المملوكة (owned value objects مثل `Patient.Age`) ضمن entries الفاشلة؛ دون هذا الإصلاح لم يكن `DuplicateLabIdException` ليُطلَق أبدًا على حفظ مريض حقيقي، ولكانت حلقة الـ retry ميّتة عمليًا.
4. **قيود أعمال غير معتمدة رسميًا:** سجل القرارات الموجود في المستودع (`Docs/DecisionRecords/Decision_Records_For_Phase1_From_Version4.md`) يصرّح بوجود 6 أسئلة أعمال مفتوحة (Q1–Q6)؛ التدقيق يؤكد استمرارها كلها في الكود (انظر §5.4).

---

## 3. التحقيق في التدفقات المالكة الغائبة — النتيجة

### 3.1 الأسئلة الثلاثة — إجابات مُثبتة بالبحث

**س1: هل يوجد أي أمر في `Features/**` ينشئ `PatientVisit` فعليًا؟ — ❌ لا.**

- البحث عن `new PatientVisit` و`PatientVisit.Create` في كامل `src/` و`tests/`: كل النتائج الإنتاجية خارج `src/`؛ الاستخدامات الوحيدة لـ `PatientVisit.Create(...)` هي في الاختبارات: `tests/MasrLab.Domain.Tests/*` (20+ موضعًا)، `tests/MasrLab.Infrastructure.Tests/VisitRepositoryDateRangeIntegrationTests.cs:112-114` و`OutsourcedSampleRepositoryDateRangeIntegrationTests.cs:22`، و`tests/MasrLab.Application.Tests/MappingProfileTests.cs:288`.
- الكيان يعرض مصنعَين (`src/MasrLab.Domain/Entities/Core/PatientVisit.cs:25-39` بالمعرّفات، و`:42-46` بنسخة تأخذ كيان `Patient` وتورّث `DoctorId` منه — INV-03)، وكلا المصنعين **بلا مستهلك إنتاجي**.
- **فحص المسارات غير المباشرة:** `RegisterPatientCommandHandler.cs:31-71` ينشئ `Patient` فقط (`Patient.Register` في `:33`) ولا ينشئ زيارة ولا يستدعي أي تدفق يفعل؛ لا يوجد أي معالج حضور/استقبال آخر. `GetPatientVisitHistoryQueryHandler` يقرأ `GetByPatientIdAsync` فقط. الخلاصة: **لا يوجد مسار مباشر أو غير مباشر لإنشاء زيارة**.

**س2: هل يوجد أي استدعاء لـ `PatientVisit.AddTest` من خارج الكيان؟ — ❌ لا (إنتاجيًا).**

- كل نتائج `\.AddTest(` خارج `PatientVisit.cs:48` هي في `tests/MasrLab.Domain.Tests/` (15 موضعًا). لا يوجد أي أمر/معالج يضيف اختبارًا لزيارة قائمة مع تسعيره. ملاحظة مرافقة: يوجد أمر `TestsMasterData/AddTest` لكنه يضيف **تعريف اختبار** للكتالوج (`Features/TestsMasterData/Commands/AddTest/`) ولا علاقة له بـ `PatientVisit.AddTest`.

**س3: هل يوجد أي معالج يستخدم كيان `Receipt` فعليًا؟ — ❌ لا.**

- البحث عن `Receipt` في `src/MasrLab.Application`: الملفات الوحيدة ذات الصلة بالكيان هي `Services/ReceiptCalculationService.cs` (خدمة حسابية نقية لا تلمس الكيان) و`Common/DTOs/ReceiptDto.cs`؛ بقية النتائج هي **إعدادات الإيصال** (`UpdateReceiptSettings*`, `ReceiptSettingsDto`) — وهي إدارة إعدادات طباعة لا إصدار إيصالات.
- لا يوجد `IReceiptRepository` في `src/MasrLab.Domain/Interfaces/` (20 واجهة، لا واحدة للإيصال). الإيصال مكوَّن في EF (`MasrLabDbContext.cs:40` يعرّف `DbSet<Receipt>`، و`ReceiptConfiguration.cs` يضبط الجدول والفهارس `:27-29`)، لكن لا مسار تطبيقي يصل إليه.
- حالة الحياة المالية للكيان جاهزة تمامًا في Domain: `Receipt.AddVisitTest` (`Receipt.cs:44-53`)، `ApplyDiscount` (`:86-97`)، `Issue` (`:99-108`)، `AddPayment` (`:110-126`)، مع أحداث `ReceiptIssued`/`ReceiptPaymentAdded`/`DiscountApplied` (`DomainEvents.cs:26-30`) — **كلها بلا مستهلك تطبيقي**.

### 3.2 الخلاصة

الفجوة قائمة بالكامل ولم يتغير فيها شيء بين نقطة التدقيق السابقة وهذا الكوميت. الكوميتات الثلاث الأخيرة حسّنت الجودة حول الفجوة (تسعير آمن، حارس عينة، محاسبة) دون لمس التدفقات نفسها. النظام حاليًا يملك «ذيل» دورة العمل (إدخال نتائج لاختبارات زيارة موجودة، تسليم نتائج، جمع عينات لعينات موجودة) ولا يملك «رأسها» (إنشاء الزيارة وإضافة الاختبارات وإصدار الإيصال) — أي أن عدة معالجات موجودة (مثل `EnterTestResult`, `MarkSampleCollected`, `GetPendingSamples`) **غير قابلة للوصول وظيفيًا** لعدم وجود ما يسبقها.

### 3.3 خطة بناء التدفقات الثلاثة (وصفية، لا تنفيذية)

> الأساس: الكود الفعلي الموثَّق أعلاه. الأسماء المقترحة اقتراحات تصميمية قابلة للتعديل.

#### أ. F-1 — إنشاء زيارة مريض (`CreatePatientVisit`)

**القرار المعماري: أمر مستقل، لا جزء من `RegisterPatient`.** الدليل من الكود: `RegisterPatientCommandHandler.cs:31-71` يبني كيان `Patient` كاملًا (25 معاملًا في الأمر، `RegisterPatientCommand.cs:7-30`) ودورة حياته مستقلة؛ والزيارة كيان قائم بذاته بمصنع خاص `PatientVisit.Create(Patient, …)` (`PatientVisit.cs:42-46`) يتطلب كيان المريض المحفوظ. دمجهما يكسر مسؤولية كل أمر ويعقّد حلقة retry الخاصة بـ LabId (§2، المرحلة 3). **لكن يُوصى بأن يعيد الأمر معرّف الزيارة الجديدة** (خلافًا لنمط `Unit` السائد) لأن واجهة الاستقبال ستحتاج فورًا لإضافة اختبارات/إيصال للزيارة المولودة.

**الحقول المطلوبة (مشتقة من قيود `PatientVisit.cs:9-46` الفعلية):**

| الحقل | المصدر/القيد |
|---|---|
| `PatientId` | إلزامي، `GreaterThan(0)`، مع فحص وجود المريض (نمط المعالجات الشقيقة: رمي عند الغياب — انظر Q-5 لتوحيد نوع الاستثناء). |
| `RegisteredByUserId` | إلزامي `GreaterThan(0)` — يجب أن يُشتق من `ICurrentUserService` (`DependencyInjection` يوفّره عبر Infrastructure `DependencyInjection.cs:45`) لا من إدخال المستخدم. |
| `LabId` | يُولَّد داخليًا عبر `LabIdGenerator.GenerateAsync` (`LabIdGenerator.cs:15-21`)؛ لا يُقبل من الخارج. ملاحظة: فهرس `PatientVisit.LabId` غير فريد حاليًا (`PatientVisitConfiguration.cs:23`) — **يُطلب تقييم جعله فريدًا مع ميجريشن جديدة قبل إطلاق التدفق**، وإلا أُعيد إنتاج سباق التفرّد على مستوى الزيارة. |
| `DoctorId` (اختياري) | إن كان `null` ورث طبيب المريض (`PatientVisit.cs:42-46` — INV-03)؛ لهذا يجب استخدام نسخة المصنع الآخذة لكيان `Patient`. |
| `ReferralEntityId` (اختياري) | بلا قيود في الكيان. |
| `VisitDate`/`Status` | لا تُمرَّران: يضبطهما المصنع (`:30-31`: `UtcNow` + `Registered`). |

**المكوّنات المقترحة:**
- **Command:** `record CreatePatientVisitCommand(int PatientId, int? DoctorId, int? ReferralEntityId) : IRequest<int>` (يعيد `Visit.Id`).
- **Handler:** يحمّل المريض المتتبَّع عبر `IPatientRepository.GetByIdAsync`، يولّد LabId، يستدعي `PatientVisit.Create(patient, currentUserId, labId, doctorId, referralEntityId)`، يحفظ عبر `IRepository<PatientVisit>.AddAsync` ثم `IUnitOfWork.SaveChangesAsync(ct)`. حدث `PatientVisitCreated` سيُطلَق تلقائيًا من المصنع (`PatientVisit.cs:37`) — يبقى بلا معالج حتى تُحسم سياسة الأحداث (§5، المرحلة 5).
- **Validator:** القيود أعلاه؛ لا تحقق من حالة العينة أو الأسعار هنا.
- **DTO:** لا حاجة لـ DTO إدخال؛ عند الحاجة لعرض الزيارة يوجد أصلًا `VisitDto` (`Common/DTOs/VisitDto.cs`) و`VisitMappingProfile`.

#### ب. F-2 — إضافة اختبار لزيارة مع تسعير (`AddTestToVisit`)

**التكامل الإلزامي مع خدمة التسعير الحالية (وهذا ما يغلق عزلة `IPriceListResolverService` فعليًا):**

1. المعالج يحمّل `PatientVisit` **المتتبَّع** عبر `IRepository<PatientVisit>.GetByIdAsync` (لا عبر `GetVisitTestAsync` لأنه `AsNoTracking` — `VisitRepository.cs:16-21`).
2. التسعير عبر `IPriceListResolverService.ResolvePriceAsync(testId, priceListId, ct)` (`PriceListResolverService.cs:25-37`). **السلوك عند غياب السعر محسوم كودًا: استثناء `BusinessRuleViolationException`** — أي أن الأمر سيفشل بأمان. لا يوجد أي منطق تراجع بين قوائم أسعار متعددة في النظام (بحث صفري)؛ إن أرادت الأعمال مسار «قائمة نوع الحساب ← قائمة المعمل الافتراضية» فيجب حسم Q-1 **قبل** تنفيذ هذا التدفق، لأن توقيع الاستدعاء وتجربة المستخدم يختلفان جذريًا بين السياستين.
3. **`priceListId` من أين؟** الكود لا يحدد قائمة افتراضية: `Patient.AccountType` موجود (`RegisterPatientCommandHandler.cs:41`) وكيانات `PriceList`/`PriceListItem` موجودة (`Entities/Settings/`) لكن لا توجد خاصية «قائمة افتراضية» أو ربط `AccountType → PriceList` في أي مكان مُدقَّق. **هذه فجوة تصميم يجب حسمها ضمن هذا التدفق** (الخيارات: معامل صريح في الأمر من شاشة الاستقبال، أو إعداد نظام عبر `ISystemSettingRepository`، أو عمود على `PriceList` مثل `IsDefault` — وهو ما يستلزم ميجريشن).
4. الإضافة عبر `visit.AddTest(testId, price, isOutsourced)` (`PatientVisit.cs:48-55`) الذي يفرض: الزيارة غير مغلقة، ويطلق `VisitTestAdded`. `VisitTest.Price` نفسه يرفض السالب (`VisitTest.cs:13-20`) كحارس أخير.
5. **الربط بتتبع العينة:** لا يرتبط هذا الأمر بـ `ISampleTrackingService` (فهي قراءة حالة فقط، `SampleTrackingService.cs:22-37`)، لكن **يُوصى بإنشاء سجل `Sample` مقترن في نفس المعاملة** عبر `Sample.Create(patientVisitId, testId)` (`Sample.cs:18-26`) — لأن `GetPendingSamplesQueryHandler` (شاشة سحب العينات) و`MarkSampleCollectedCommandHandler` وحارس إدخال النتيجة كلها تفترض وجود سجلات عينات، ولا يوجد حاليًا أي منشئ لها. دون هذه الخطوة ستبقى شاشة العينات فارغة دائمًا وحارس العينة سيرفض كل إدخال نتيجة. (إن لم تكن كل الاختبارات تتطلب عينة، يُضبط ذلك براية على تعريف الاختبار — قرار تصميمي يُحسم مع صاحب الأعمال.)
6. الاختبارات المركّبة (Group): `TestGroup`/`TestGroupItem` وDTOs مقابلة موجودة؛ يمكن لاحقًا دعم إضافة مجموعة تتوسع إلى عدة `AddTest` في نفس المعاملة — خارج النطاق الأدنى.

**المكوّنات المقترحة:**
- **Command:** `AddTestToVisitCommand(int PatientVisitId, IReadOnlyList<int> TestIds, int PriceListId, bool MarkOutsourced...)` — قائمة اختبارات لتقليل رحلات الواجهة، أو اختبار واحد في النسخة الدنيا.
- **Handler:** تحميل الزيارة → لكل اختبار: حل السعر → `visit.AddTest` → `Sample.Create` → حفظ واحد في النهاية (`IUnitOfWork.SaveChangesAsync(ct)`).
- **Validator:** معرّفات `> 0`، قائمة غير فارغة، `PriceListId > 0` (مطابق لقيد الخدمة `:27-28`).
- **DTO:** لا DTO إدخال إضافي؛ مخرجات اختيارية `VisitTestIds` المولودة لتسهيل ربط الواجهة.

#### ج. F-3 — إصدار إيصال (`IssueReceipt`)

**العلاقة الفعلية في الكود (مدقَّقة، لا مفترضة):**

- `Receipt` يتطلب `PatientVisitId` (`Receipt.cs:13`) ويجمع `VisitTest`s و`ExtraServiceItem`s ويحسب الإجمالي ذاتيًا عبر `RecalculateTotal` (`:30-36`) ويُصدر عبر `Issue()` (`:99-108`) الذي يطلق `ReceiptIssued`. `VisitTest` يحمل `ReceiptId?` (`VisitTest.cs:24`) — أي أن الربط مصمَّم ثنائي الاتجاه لكنه غير مفعَّل بأي كود.
- **`RecordCashTransactionCommandHandler` موجود فعلًا ويعمل على مستوى الحسابات (drawers) لا على الإيصالات:** ينشئ `CashTransaction.Deposit/Withdraw` مرتبطًا بـ `Account` (`RecordCashTransactionCommandHandler.cs:37-47`) ثم يعيد حساب صافي الربح (`:51`). **لا توجد أي قراءة أو كتابة لـ `Receipt` فيه.** العلاقة المطلوبة إذن: إيصال → (دفعة) → قيد نقدي اختياري في حساب/صندوق المعمل، وهي علاقة **تكوين (orchestration) يجب بناؤها**، لا ربط موجود يُفعَّل. ونظرًا لأن توقيع `RecordCashTransactionCommand` الحالي لا يحمل مرجع إيصال، فالخيار الأنظف: معالج الإيصال ينسّق، ولا يُعدَّل أمر القيد النقدي في النسخة الأولى (أو يُضاف له `ReceiptId?` في نسخة لاحقة بعد حسم نموذج الربط المحاسبي).
- **دور الخدمات المعزولة حاليًا داخل هذا التدفق (وهو ما يغلق عزلتها فعليًا):**
  - `IReceiptCalculationService.CalculateRemaining/CalculateChangeDue` (`ReceiptCalculationService.cs:17-32`) لحساب المتبقي والفرق المسترد (`Receipt.ChangeDue` و`RefundToPatient` حقول موجودة `Receipt.cs:22-23` لكن لا من يضبطها).
  - `IPricingService.CalculateSubtotal/CalculateTotal` (`PricingService.cs:20-34`) كتحقق مقاطَع (cross-check) مع إجمالي الكيان قبل الإصدار — ملاحظة: يعمل على `PatientVisit` لا على `Receipt`، وهو سبب إضافي لتسوية الحقن الميت فيه (`:11-14`) أو استخدامه فعليًا هنا.
  - الخصم عبر `receipt.ApplyDiscount` (`Receipt.cs:86-97`) بقيوده المدمجة (لا خصم بعد السداد الكامل، ≤ الإجمالي).
- **الاعتماد الجديد الإلزامي في Domain:** واجهة `IReceiptRepository` (غير موجودة — §3.1) أو على الأقل استخدام `IRepository<Receipt>` العام مع استعلام «إيصال الزيارة المفتوح» لمنع ازدواج الإيصالات لنفس الزيارة (لا يوجد قيد فريد على `Receipt.PatientVisitId` — `ReceiptConfiguration.cs:27` فهرس عادي).
- **الدفع الجزئي:** `Receipt.AddPayment` (`:110-126`) يدعمه بحالات `PartiallyPaid/Paid`؛ يُفضَّل فصله لأمر لاحق `AddReceiptPayment` لتقليل حجم المعاملة الأولى.
- بعد الإصدار: استدعاء `visit.IssueReceipt()` (`PatientVisit.cs:77-82`) لمواءمة آلة حالة الزيارة — ملاحظة تصميمية: التسمية والانتقال الحاليان غريبان (يضبط `ResultsEntered`)، وتستحق مراجعة ضمن هذا التدفق.

**المكوّنات المقترحة:**
- **Command:** `IssueReceiptCommand(int PatientVisitId, decimal Discount, decimal PaidNow, int ReceivedByUserId, int? CashAccountId)` → يعيد `ReceiptId`.
- **Handler:** تحميل الزيارة باختباراتها (تحتاج استعلامًا متتبّعًا مع `VisitTests` — إما `IRepository<PatientVisit>` مع Include جديد في `IVisitRepository`، وهذا تعديل عقد Domain صغير) → منع إيصال ثانٍ مفتوح → بناء `Receipt` بربط كل `VisitTest` → `ApplyDiscount` إن وُجد → `Issue()` → ضبط `ChangeDue/RefundToPatient` عبر `IReceiptCalculationService` → إن كان `PaidNow > 0` نسّق `RecordCashTransaction` (أو كيان القيد مباشرة) → حفظ واحد.
- **Validator:** مبالغ `>= 0`، خصم ≤ الإجمالي (الكيان يفرضه أيضًا)، معرّفات `> 0`.
- **DTO:** إعادة استخدام `ReceiptDto` الموجود (`Common/DTOs/ReceiptDto.cs`) لنتيجة الإصدار/الطباعة لاحقًا.

---

## 4. تبعيات عبر الطبقات (Cross-Layer Dependencies)

| # | مجال العمل | التبعية على Infrastructure | التبعية على Presentation | النوع |
|---|---|---|---|---|
| D-1 | F-1 إنشاء زيارة | تقييم/إضافة فهرس فريد على `PatientVisit.LabId` + ميجريشن (`PatientVisitConfiguration.cs:23` حاليًا غير فريد)؛ لا يوجد عقد استعلام إضافي مطلوب | شاشة استقبال تستهلك الأمر عبر MediatR وتعالج `BusinessRuleViolationException`/`ValidationException` | شرط سابق (الفهرس) / لاحق (الواجهة) |
| D-2 | F-2 إضافة اختبار مسعَّر | حسم مصدر `PriceListId` الافتراضي (قد يستلزم عمودًا/إعدادًا + ميجريشن)؛ لا يوجد حاليًا أي ربط `AccountType → PriceList` في الكود | منتقي اختبارات (يوجد `Controls/TestSelector.xaml.cs` كنواة) وعرض خطأ «لا يوجد سعر» | شرط سابق |
| D-3 | F-3 إصدار إيصال | تعريف `IReceiptRepository` (Domain) وتنفيذه، أو توسيع `IVisitRepository` بجلب متتبّع مع `VisitTests`؛ تقييم قيد فريد على إيصال مفتوح لكل زيارة | شاشة إيصال + طباعة؛ الطباعة نفسها محجوبة بـ `PrintService.cs:9,:14` غير المُنفَّذة (stubs) | شرط سابق (العقد) / لاحق (الطباعة) |
| D-4 | التدقيق (Audit) | استمرار توفير `IRequestAuditLogRepository` و`IUnitOfWork` (مسجَّلان: `Infrastructure/DependencyInjection.cs:36-39,:59`)؛ سياسة فشل الحفظ المُبتلعة (`AuditBehavior.cs:72-76`) تحتاج مراقبة/بديلًا موثقًا | — | قائم، يحتاج قرارًا لا كودًا جديدًا |
| D-5 | المصادقة والحضور | تنفيذ `AuthenticationService.cs:10,:15` (يرمي حاليًا) — شرط لأي معنى فعلي لـ `RegisteredByUserId`/`EnteredByUserId` في التدفقات الثلاثة | شاشة دخول مربوطة | شرط سابق لتشغيل إنتاجي حقيقي (وليس لصحة الوحدة) |
| D-6 | الباركود للعينات | `BarcodeService.cs:9` غير مُنفَّذ؛ `Sample.Barcode` موجود (`Sample.cs:12`) | طباعة/عرض الباركود | لاحق لـ F-2 |
| D-7 | حدود الأخطاء في UI | — | اعتماد `ISender` في ViewModels + معالجة `ValidationException`/استثناءات المجال؛ حاليًا صفر استخدام (`Presentation/DependencyInjection.cs:29-64`) | شرط لاحق عام |
| D-8 | مُحوّلات WPF | — | تنفيذ 4 محوّلات ترمي `NotImplementedException` (`Converters/*.cs:10,:15`) قبل عرض حالات النتائج/الزيارات/الحسابات | شرط سابق لأي شاشة تعرض تلك الحالات |
| D-9 | تفريد/تزامن LabId | قائم (فهرس + ترجمة خطأ + retry)؛ اختباراته LocalDB-gated وتعمل على Windows فقط (`LocalDbTestInfrastructure.cs:14`) | — | قائم، يحتاج بيئة تشغيل CI مناسبة |
| D-10 | نسخ احتياطي | `BackupService.cs:9,:14` غير مُنفَّذ | اختيار المسار وعرض النتيجة | مستقل عن التدفقات الثلاثة |

---

## 5. خارطة الطريق الموحّدة والمرتّبة زمنيًا

> بُنيت من الصفر وفق الحالة الفعلية المُدقَّقة، بترقيم مستقل (S1–S6) يراعي التبعيات المتبادلة: لا يُبنى تدفق تسعير قبل اكتمال إصلاح خدمة التسعير، ولا إيصال قبل زيارة واختبارات.

### S1 — حسم أسئلة الأعمال الستة (قرارات، لا كود) — أسبوع قرار واحد

شرط سابق للتصميم النهائي للتدفقات. الأسئلة (كلها مُثبتة الوجود في الكود وسجل القرارات):

- **Q-1 (تسعير):** إبقاء سياسة «الفشل الآمن بالاستثناء» الحالية (`PriceListResolverService.cs:32-34`) أم تنفيذ تراجع متسلسل بين قوائم أسعار متعددة (غير موجود إطلاقًا)؟ وما مصدر القائمة الافتراضية لكل زيارة؟
- **Q-2 (عمولة):** أساس العمولة `Account.TotalIncome` كما نُفِّذ (`AccountingService.cs:59-60,:67`) أم الإيراد بعد الخصم؟
- **Q-3 (صيغة الصافي):** اعتماد `NetProfit = TotalIncome − TotalDiscount − CommissionsTotal` رسميًا (ما يُسقط `OutsourcedCost/CashWithdrawals/CashDeposits` من المعادلة الموثقة القديمة) أم استرجاعها؟ وهل يُقبل صافٍ سالب؟
- **Q-4 (حارس العينة):** هل يُفرض `Status = Abnormal` عند إدخال بتجاوز، أم يكفي `OverrideReason` غير الفارغ؟
- **Q-5 (عقد الخطأ):** توحيد «غير موجود» على استثناء واحد (حاليًا `InvalidOperationException` في `EnterTestResultCommandHandler.cs:41,:44` و`RecordCashTransactionCommandHandler.cs:35-36`).
- **Q-6 (الخدمات المعزولة):** سيُحسم فعليًا ببناء F-2/F-3 (Q-1 أولًا)؛ إن رُفض بناء التدفقات، تُقيَّد/تُزال العقود الثلاثة بدلًا من ذلك.

**معيار القبول:** سجل قرارات مُعتمد من صاحب الأعمال؛ كل قرار يشير لسطر كود سيتغير أو يثبت.

### S2 — التدفق المالك F-1: إنشاء زيارة مريض

المحتوى كما في §3.3-أ. يشمل: تقييم فهرس `PatientVisit.LabId` الفريد (D-1)، الأمر/المعالج/المدقق، واختبارات وحدة للمعالج (ناجح، مريض غائب، توريث الطبيب INV-03 عبر نسخة المصنع الآخذة لكيان المريض). **لا يعتمد على أي بند آخر سوى S1 (لتثبيت Q-5).**

### S3 — التدفق المالك F-2: إضافة اختبار لزيارة مع تسعير + إنشاء العينات

المحتوى كما في §3.3-ب. يشمل: تفعيل `IPriceListResolverService` بأول مستهلك Features (يغلق جزءًا من Q-6 فعليًا)، مصدر القائمة الافتراضية (D-2)، إنشاء `Sample` المقترن، واختبارات: سعر موجود، بند غائب ⇒ فشل آمن، زيارة مغلقة ⇒ `BusinessRuleViolationException` (`PatientVisit.cs:50-51`)، تكرار الاختبار، إنشاء العينة في نفس المعاملة. **يعتمد على S1 (Q-1) وS2.** بعده مباشرة تصبح `GetPendingSamples`/`MarkSampleCollected`/حارس `EnterTestResult` قابلة للوصول وظيفيًا لأول مرة.

### S4 — التدفق المالك F-3: إصدار إيصال + تسوية الخدمات المعزولة

المحتوى كما في §3.3-ج. يشمل: عقد `IReceiptRepository`/استعلام الجلب المتتبّع (D-3)، تفعيل `IReceiptCalculationService` و`IPricingService` (إغلاق Q-6 بالكامل وإزالة الحقن الميت في `PricingService.cs:11-14`)، منع إيصال مزدوج مفتوح، التنسيق مع القيد النقدي، ومراجعة آلة الحالة `PatientVisit.IssueReceipt()` (`:77-82`). اختبارات: إصدار سعيد، خصم يتجاوز الإجمالي، دفع يتجاوز الإجمالي (`Receipt.cs:118-119`)، إيصال ثانٍ، حساب `ChangeDue`. **يعتمد على S2 وS3.**

### S5 — تثبيت الجودة: اختبارات، سياسة Audit، توحيد Mapping

1. رفع تغطية المعالجات/الخدمات/المدققين (حاليًا 2/68 معالجًا و3/10 خدمات و4/39 مدققًا مختبَرة) مع أولوية للمسارات المالية والتدفقات الثلاثة الجديدة؛ إعادة تسمية `tests/MasrLab.Infrastructure.Tests/PlaceholderTests.cs`.
2. اختبار تكامل حاوية `AddApplication + AddInfrastructure` (غير موجود).
3. حسم سياسة فشل حفظ التدقيق (`AuditBehavior.cs:72-76`) وتوثيقها كقرار؛ إضافة مراقبة أو مسار بديل إن قُرِّر عدم الإسكات.
4. تشغيل `dotnet build`/`dotnet test` فعليًا على بيئة Windows/LocalDB (اختبارات S-التزامن مقيَّدة بذلك، `LocalDbTestInfrastructure.cs:14,:44-52`) وتحويل كل الأحكام الساكنة في هذه الوثيقة إلى مُثبتة تنفيذيًا.
5. تنظيف `_Placeholder.cs` الملفين (`Common/Models/_Placeholder.cs:1-6`, `Common/Validations/_Placeholder.cs:1-6`) وقرار عقد الأخطاء المشترك (Q-5).

**يعتمد على S2–S4 لتغطية التدفقات الجديدة؛ البندان 4–5 مستقلان ويمكن تقديمهما.**

### S6 — إغلاق حدود Presentation والأحداث والخدمات الأربع

1. اعتماد `ISender`/محوّل واضح في ViewModels ومعالجة `ValidationException`/استثناءات المجال عند الحد (D-7) — شرط لتشغيل التدفقات الثلاثة من الواجهة.
2. تنفيذ المحوّلات الأربعة stubs (D-8).
3. جرد أحداث المجال الـ21 (`DomainEvents.cs:8-46`) واتخاذ قرار «معالج أو تأجيل موثَّق» لكل منها؛ الأولوية لأحداث التدفقات الثلاثة (`PatientVisitCreated`, `VisitTestAdded`, `ReceiptIssued`, `ReceiptPaymentAdded`, `DiscountApplied`).
4. تنفيذ خدمات Infrastructure الأربع: Authentication (D-5، شرط إنتاجي)، Print/Barcode (شرط طباعة الإيصال وباركود العينات)، Backup.
5. شاشات الواجهة للتدفقات الثلاثة (استقبال/زيارة، إضافة اختبارات، إيصال).

**يعتمد على S2–S4؛ بنوده الداخلية قابلة للتوازي.**

### ترتيب التبعيات الصارم (ملخص)

`S1 (Q-1 تحديدًا) → S2 → S3 → S4 → (S5 ∥ S6)`؛ البندان S5.4 وS5.5 يمكن البدء بهما فورًا بالتوازي مع S1.

---

## 6. تقييم الجاهزية النهائي

| البعد | التقييم | الحكم |
|---|---|---|
| البنية والعقود | ✅ جاهز | حدود مرجعية صحيحة (`MasrLab.Application.csproj:9`)، 68 معالجًا/39 مدققًا متطابقين، DI مركزي مكتمل (`DependencyInjection.cs:13-37`). |
| سلامة التسعير | ✅ جاهز ساكنًا | لا مسار يحوّل غياب السعر إلى صفر (`PriceListResolverService.cs:32-36`)؛ لا أسعار سالبة عبر الأمر الوحيد المعدِّل للقوائم. |
| تزامن LabId | ✅ مُحكم ساكنًا | فهرس فريد + ترجمة خطأ + retry + اختبارا LocalDB؛ ينقصه التشغيل الفعلي على بيئة مناسبة فقط. |
| قواعد الأعمال/الخدمات | 🟡 غير مكتمل | 7 من 10 خدمات لها أثر وظيفي (5 مباشر + ReferralCommission عبر المحاسبة + SampleTracking عبر حارس العينة)؛ 3 معزولة بالكامل وحقن ميت في `PricingService.cs:11-14`. |
| **التدفقات المالكة** | 🔴 غائبة بالكامل | لا إنشاء زيارة، لا إضافة اختبار لزيارة، لا إصدار إيصال في أي مسار إنتاجي (§3) — الفجوة الحاكمة على الجاهزية الكلية. |
| Mapping | 🟡 محسومة بشروط | سياسة موثقة بالاستثناءات + حماية تهيئة + 14 اختبارًا؛ لكن 12 خريطة فقط لـ 49 DTO ويحتمل وجود DTOs عائدة بلا خريطة مثبتة. |
| التدقيق (Audit) | 🟡 مُثبت وحداتيًا، سياسته غير محسومة | 3 اختبارات سلوك + فشل الحفظ مُبتلع بلا بديل موثق (`AuditBehavior.cs:72-76`). |
| الاختبارات والبناء | 🟡 قفزة حقيقية لكن غير مُثبتة تنفيذيًا | 55 سمة اختبار في Application (من 14)، 20 في Infrastructure؛ لا تكامل حاوية؛ **لم يمكن تشغيل `dotnet` في بيئة هذا التدقيق — كل أحكام البناء/التشغيل هنا غير مُثبتة تنفيذيًا**. |
| حدود Presentation والأحداث | 🔴 غير جاهز | صفر MediatR وصفر معالجة أخطاء في الواجهة، 4 محوّلات stubs، 4 خدمات Infrastructure stubs، لا `INotificationHandler` لأي من أحداث المجال الـ21. |

### الحكم

طبقة Application عند الكوميت `0773d6dd3c7fd83e4d770032fd1106f34c775f14` **أكثر نضجًا وأمانًا مما كانت عليه عند أي نقطة تدقيق سابقة: التسعير آمن، توليد المعرفات محصَّن، والتغطية الاختبارية تضاعفت ~4 أضعاف — لكنها تظل غير مكتملة وظيفيًا بشكل حاسم** لأن رأس دورة العمل (زيارة → اختبارات مسعَّرة → إيصال) غير موجود في أي مسار إنتاجي، مما يجعل عدة معالجات ناضجة غير قابلة للوصول. خارطة الطريق S1→S6 أعلاه تعالج ذلك بالترتيب الوحيد المتسق مع التبعيات الفعلية المدقَّقة، ولا يصح إعلان الجاهزية الإنتاجية قبل: (1) اعتماد قرارات S1، (2) إكمال S2–S4، (3) تشغيل البناء والاختبارات فعليًا على بيئة Windows/.NET 8 SDK مع LocalDB لتحويل الأحكام الساكنة إلى مُثبتة.
