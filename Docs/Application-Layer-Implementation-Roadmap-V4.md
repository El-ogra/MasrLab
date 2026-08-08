# MasrLab — خارطة تنفيذ طبقة Application V4

**المشروع:** MasrLab Laboratory Information System  
**المكدّس:** .NET 8 · WPF · MVVM · Clean Architecture · MediatR · AutoMapper · FluentValidation  
**نقطة التدقيق الوحيدة:** `e5554c5f7327468a83da3f49fe6f3e6e796f8d92` — الرسالة: `تنفيذ المرحلة D الدفعة C` — الفرع: `niamod`  
**حالة الوثيقة:** مرجع مستقل لخطة العمل المتبقي في طبقة Application.

---

# الملخص التنفيذي

تم تثبيت شجرة العمل صراحةً على الهاش المحدد أعلاه قبل الفحص. طبقة `src/MasrLab.Application` ناضجة بنيوياً: تضم 68 معالجاً، 39 أمراً، 29 استعلاماً، و39 مدقق FluentValidation؛ لا يوجد `NotImplementedException` داخل الطبقة، وجميع الأوامر لها مدقق مقابل في مجلد الاستخدام نفسه. التسجيل المركزي يشمل MediatR وAutoMapper والمدققين وسلوكي Validation وAudit، كما يسجل الخدمات العشر ذات العقود الموجودة في Domain. الدليل المباشر: `src/MasrLab.Application/DependencyInjection.cs:17-34` ووجود `AddCaseFollowUpCommandValidator` في `Features/CasesFollowUp/Commands/AddCaseFollowUp/AddCaseFollowUpCommandValidator.cs:5-11`.

أغلقت الشفرة فعلياً عدداً كبيراً من مخاطر التكامل: لا توجد أي نتيجة لـ `GetAwaiter().GetResult()` في `src/`، كما لا توجد استدعاءات `GetAllAsync` في Application ولا `SaveChangesAsync()` بلا رمز إلغاء. تمت إعادة بناء مولد Lab ID ليطلب أكبر لاحقة من المستودع مع تمرير `CancellationToken` بدلاً من تحميل سجل المرضى كاملاً؛ الدليل `Common/Helpers/LabIdGenerator.cs:15-20`.

لكن الجاهزية الإنتاجية **ليست مكتملة**. ست خدمات Domain فقط لها مستهلكات فعلية داخل المعالجات: المحاسبة، الحساسية الزراعية، التاريخ الطبي، الإحالة الخارجية، والتحقق من النتائج؛ بينما `IPriceListResolverService` و`IPricingService` و`IReceiptCalculationService` و`IReferralCommissionService` و`ISampleTrackingService` مسجلة ولكن لا تظهر في أي ملف تحت `Features/`. ما زال `PriceListResolverService` يعيد `0` عند غياب بند السعر (`Services/PriceListResolverService.cs:29-31`)؛ وهذا غير كافٍ لمسار مالي ما لم يكن الصفر قاعدة مجال صريحة.

AutoMapper أصبح مستخدماً في عدد من معالجات الاستعلامات، وتوجد حماية تهيئة فعلية (`tests/MasrLab.Application.Tests/MappingConfigurationTests.cs:9-17`)، لكن الاستراتيجية مختلطة: معالجات أخرى ما زالت تنشئ DTOs يدوياً، ومنها سجل التاريخ في `Features/PatientHistory/Queries/GetPatientHistory/GetPatientHistoryQueryHandler.cs:21-39`. تغطي اختبارات Application تعريفياً 14 `[Fact]/[Theory]` موزعة على ملفين فقط؛ لا توجد اختبارات معالجات، خدمات Domain، AuditBehavior، LabIdGenerator أو تكامل حاوية DI. لم يكن تنفيذ `dotnet build` أو `dotnet test` ممكناً في بيئة التدقيق لأن أمر `dotnet` غير مثبت؛ لذا فإن أي حكم بناء/تشغيل هنا حكم ساكن على الشفرة وليس إثباتاً تنفيذياً.

---

# نطاق التدقيق ومنهجيته

## تثبيت نقطة المصدر

تم التحقق من أن `HEAD` يساوي تماماً `e5554c5f7327468a83da3f49fe6f3e6e796f8d92` وأن رسالة الكوميت مطابقة. لم يُبنَ أي استنتاج على أحدث حالة عائمة للفرع أو على شجرة عمل أخرى.

## الفحوص الميكانيكية المنفذة

- تعداد الملفات: 68 `*Handler.cs`، 39 `*Command.cs`، 29 `*Query.cs`، 39 `*Validator.cs`، 49 DTO، 14 Profile، و31 `CreateMap<>`.
- بحث صفري عن `GetAwaiter().GetResult()` داخل `src/`، و`NotImplementedException` داخل Application، و`MasrLab.Infrastructure` داخل Application، و`GetAllAsync` داخل Application، و`SaveChangesAsync()` بلا وسيطة.
- مراجعة `DependencyInjection.cs`، مسارات الخدمات، السلوكيات، المدقق المضاف، جميع اختبارات Application، عقود Domain، واجهات المستودعات، وتسجيلات Infrastructure.
- بحث مستقل عن استهلاك كل واجهة خدمة Domain داخل `src/MasrLab.Application/Features`، وعن `IMapper` و`AssertConfigurationIsValid()` و`INotificationHandler` و`ISender`/`IMediator` و`ValidationException` في Presentation.
- محاولة تشغيل البناء والاختبارات. النتيجة: فشل تشغيلي سابق للشفرة بسبب `dotnet: command not found`، ولا يجوز تحويل ذلك إلى نتيجة نجاح أو فشل للبناء.

## قيد الأدلة

كل رقم سطر أدناه يخص الهاش المدقق عليه. وعبارة «غياب» تعني نتيجة بحث محددة ضمن المسار المذكور، لا ادعاءاً عن مستودعات أو بيئات خارجها.

---

# تقييم الحالة الحالية بالأدلة

| المحور | الحالة | الدليل المباشر |
|---|---|---|
| حدود الطبقات | ✅ منفذ | مشروع Application يراجع Domain فقط: `MasrLab.Application.csproj:8-10`؛ بحث `MasrLab.Infrastructure` في Application صفري. |
| CQRS والعقود | ✅ منفذ | 68 معالجاً و68 طلباً (39 أمراً + 29 استعلاماً) في `Features/`؛ لا `IRequest<object>` في المسح. |
| التحقق | ✅ منفذ بنيوياً | 39 مدققاً لـ39 أمراً، والسلوك يستدعي جميع المدققين ويطرح `ValidationException`: `Common/Behaviors/ValidationBehavior.cs:16-34`. |
| DI للخدمات والسلوكيات | ✅ منفذ ساكناً | Validation ثم Audit: `DependencyInjection.cs:20-21`؛ الخدمات العشر scoped: `:25-34`. |
| Audit | 🟡 جزئي | المنطق ينشئ `RequestAuditLog` ويحفظه: `AuditBehavior.cs:28-70`، لكن لا اختبار أو إثبات تكاملي، وفشل حفظ التدقيق يُبتلع في `:68-70`. |
| الخدمات Domain | 🟡 جزئي | العقود والتنفيذ والتسجيل مكتملة، لكن 5 من 10 واجهات بلا مستهلك Features. |
| AutoMapper | 🟡 جزئي | مسجل: `DependencyInjection.cs:18`؛ اختبار صحة التهيئة: `MappingConfigurationTests.cs:9-17`؛ استعمال موجود في معالجات استعلامات، لكن mapping اليدوي ما زال موجوداً. |
| async والإلغاء | ✅ منفذ في نطاق البحث | لا blocking shim؛ كل `SaveChangesAsync` المرصود يمرر CT، ومنها `EnterTestResultCommandHandler.cs:43-44`. |
| أداء الاستعلامات | 🟡 جزئي | أزيل `GetAllAsync` من Application، ومولد Lab ID يستعلم max-suffix؛ يلزم اختبار/مراجعة استعلامات Infrastructure والتزامن الفعلي. |
| اختبارات Application | ❌ غير كافية | ملفان فقط و14 سمة اختبار معرفة؛ لا مرجع لاختبار Handler/Service/Audit/LabId في مجلد الاختبارات. |
| الأحداث والحدود Presentation | ❌ غير منفذ | لا `INotificationHandler` في Application، ولا `ISender`/`IMediator` أو معالجة `ValidationException` في Presentation. |
| خدمات Infrastructure المسندة للعقود | ❌ غير منفذة | سبعة `NotImplementedException` في Authentication/Backup/Barcode/Print. |

---

# العمل المنفذ والموثق

## البنية الأساسية والاعتمادات

- ✅ **اتجاه الاعتماد صحيح.** المرجع الوحيد للمشاريع في `src/MasrLab.Application/MasrLab.Application.csproj:8-10` هو `MasrLab.Domain.csproj`. لا توجد نتيجة لاستيراد Infrastructure من أي ملف C# داخل Application.
- ✅ **بوابة التحذيرات موجودة في الشجرة.** `Directory.Build.props:3-5` يضبط `LangVersion=latest` و`Nullable=enable` و`TreatWarningsAsErrors=true`. لم يُتحقق من نجاح البناء ديناميكياً لغياب SDK.
- ✅ **سلوك التحقق مسجل ويستخدم رمز الإلغاء.** `ValidationBehavior.cs:20-31` يجمع عمليات `ValidateAsync(..., cancellationToken)` ويطرح `ValidationException` عند الإخفاق.

## التصحيحات المغلقة

- ✅ **C1 / TD-1 — إزالة sync-over-async.** البحث `grep -rn "GetAwaiter().GetResult()" src/` أعاد صفر نتائج. الواجهات الحية للخدمات المتأثرة تعرض أسطحاً async فقط، مثل `IResultValidationService.cs:7-8`، والتنفيذ يعتمد `await` وCT في `ResultValidationService.cs:25-55`.
- ✅ **C3 / TD-3 — تسجيل خدمات Domain العشر.** `DependencyInjection.cs:25-34` يحتوي عشرة أسطر `AddScoped<I*Service,*Service>` تغطي كل الواجهات تحت `src/MasrLab.Domain/Services`.
- ✅ **C4 — تسجيل AuditBehavior بعد ValidationBehavior.** الترتيب الحرفي موجود في `DependencyInjection.cs:20-21`.
- ✅ **C6 / TD-10 — مدقق AddCaseFollowUp.** الملف موجود، ويربط `TestId` بشرط `GreaterThan(0)` و`Notes` بشرط `NotEmpty`: `AddCaseFollowUpCommandValidator.cs:5-11`.
- ✅ **C7 / TD-5 — إصلاح مسار التحقق من الجنس/العمر.** `ValidateResultAsync` و`IsResultInRangeAsync` يستقبلان `gender` و`ageYears` ويمررانهما إلى `FindMatchingReference`: `ResultValidationService.cs:25-31` و`:49-55`؛ المطابقة تقيد الجنس والمدى العمري في `:71-84`.
- ✅ **C8 — تمرير CancellationToken في النطاق المرصود.** لا توجد نتيجة لـ`GetAllAsync()` أو `SaveChangesAsync()` بلا وسيطات؛ مثال معالج النتائج `EnterTestResultCommandHandler.cs:43-44`.
- ✅ **C9 — تحسين LabIdGenerator من التحميل الكامل.** يستخدم `GetMaxLabIdSuffixAsync(prefix, cancellationToken)` ثم يزيدها: `LabIdGenerator.cs:15-20`.
- ✅ **C10 (جزء صلاحية التهيئة) / TD-7 (جزئي).** يوجد اختبار `AssertConfigurationIsValid()` في `MappingConfigurationTests.cs:9-17`، و`AddAutoMapper` في `DependencyInjection.cs:18`.
- ✅ **C11 (التعليق القديم) / TD-13.** خدمة التاريخ الطبي تشير الآن إلى `PatientHistoryView` وتستخدم `IPatientHistoryRepository`: `Services/MedicalHistoryService.cs:17-33`.

## تكاملات خدمة Domain الموجودة فعلاً

- ✅ **IResultValidationService وIMedicalHistoryService:** معالج إدخال النتيجة يحقن الخدمتين ويستدعي التحقق ثم التاريخ مع CT: `EnterTestResultCommandHandler.cs:11-25` و`:28-46`.
- ✅ **IAccountingService:** معالج المعاملة النقدية يحقن الخدمة ويستدعي `RecalculateNetProfitAsync`: `RecordCashTransactionCommandHandler.cs:11-25` و`:47-50`.
- ✅ **ICultureSensitivityService:** معالج إضافة المضاد يستدعي `RecordSensitivityAsync` مع CT: `AddAntibioticToCultureCommandHandler.cs:8-21`.
- ✅ **IOutsourcingService:** معالجا `MarkTestAsOutsourced` و`SettleOutsourcedAccount` يستدعيان عمليات الخدمة: `MarkTestAsOutsourcedCommandHandler.cs:8-23` و`SettleOutsourcedAccountCommandHandler.cs:16-25`.
- ✅ **IMedicalHistoryService للاستعلام:** `GetPatientHistoryQueryHandler.cs:10-19` يستدعي `BuildHistoryAsync` مع CT.

---

# العمل المنفذ جزئياً والفجوات المثبتة

## الخدمات المسجلة غير الموصولة بالمعالجات

🟡 **C3/مرحلة تكامل الخدمات / TD-3 المتبقي.** التسجيل مكتمل، لكن البحث المستقل لكل واجهة داخل `src/MasrLab.Application/Features` أعاد غياباً كاملاً للواجهات التالية:

1. `IPriceListResolverService`
2. `IPricingService`
3. `IReceiptCalculationService`
4. `IReferralCommissionService`
5. `ISampleTrackingService`

هذا ليس نقص DI؛ بل نقص استعمال وظيفي. يجب إما ربط كل خدمة بسيناريو CQRS يملكها، أو إزالة/تقييد العقد إن لم يعد له use case فعلي.

## التسعير

🟡 **C2 / TD-2.** تم حصر توقيع resolver في `ResolvePriceAsync(int testId, int priceListId, CancellationToken ct)`، ويرفض `priceListId <= 0` باستثناء مجال: `PriceListResolverService.cs:24-29`. هذا يلغي مسار الالتباس الخاص بمعرّف إحالة اختياري. لكنه ما زال يرجع `item?.Price ?? 0` عند عدم وجود بند للاختبار (`:29-31`). لا توجد في الشفرة قاعدة مجال أو اختبار يثبت أن سعر الصفر صحيح؛ لذلك لا يصح اعتبار خطر السعر الصفري مغلقاً بالكامل.

## التدقيق التشغيلي

🟡 **C5 / TD-4.** AuditBehavior يقرأ المستخدم والوقت (`AuditBehavior.cs:30-32`)، ويحفظ اسم الطلب، النتيجة، والخطأ (`:50-66`) ثم يعيد طرح استثناء الطلب (`:42-46`). إلا أن `PersistAuditEntryAsync` يبتلع كل استثناء حفظ في `:68-70`، ولا يوجد اختبار يثبت صف تدقيق للنجاح أو للفشل، ولا اختبار للحاوية المركبة. النتيجة: التصميم قابل للعمل لكنه غير مثبت سلوكياً وقد يخفي فقدان سجل التدقيق.

## استراتيجية AutoMapper

🟡 **C10 / TD-7.** يوجد IMapper في معالجات الاستعلام، مثلاً `SearchPatientsQueryHandler.cs:12-14` و`GetAttendanceLogsQueryHandler.cs:11-13`، ويوجد اختبار تهيئة. لكن mapping اليدوي باقٍ في `GetPatientHistoryQueryHandler.cs:21-39`، بينما توجد 14 profile و31 map فقط مقابل 49 DTO. لا يمكن من الشفرة الحالية إثبات أن كل DTO عائد من معالج له خريطة أو أن الاستراتيجية الواحدة قد اختيرت.

## تحسين الاستعلامات ومعرف المختبر

🟡 **TD-6 / C9 المتبقي.** لا توجد استدعاءات `GetAllAsync` في Application، وأصبح توليد Lab ID مبنياً على `GetMaxLabIdSuffixAsync`. لكن عملية «قراءة أعلى قيمة ثم +1» لا تظهر معها في Application آلية unique constraint/retry أو معاملة serializable؛ لذلك يبقى خطر تصادم التزامن غير محكوم ولا توجد له اختبارات موازاة.

## الوثائق والـplaceholders

🟡 **C11 / C12 / C13 / C14 وTD-11/12/14/15/16.** ما زال الملفان `Common/Models/_Placeholder.cs` و`Common/Validations/_Placeholder.cs` موجودين، ويحتويان تعليقاً يحيل إلى أسماء خطط قديمة (`_Placeholder.cs:1-6` في كل منهما). لا توجد shared validation rules ولا `Result<T>`/عقد أخطاء. ملف الاختبارات ما زال اسمه `PlaceholderTests.cs` رغم محتواه. ولا يوجد `INotificationHandler` في Application أو معالجة `ValidationException`/MediatR في Presentation.

---

# مصفوفة بنود التدقيق A–G

> هذه المصفوفة تعيد تقييم بنود العمل ذات المعرفات A–G على الشفرة الحية. لا تفترض أي نسبة تقدم مسبقة.

| البند | الحالة | الدليل والحكم |
|---|---|---|
| A — الأساس والهيكل | ✅ منفذ | حدود مرجعية صحيحة في `MasrLab.Application.csproj:8-10`؛ بوابة تحذيرات في `Directory.Build.props:3-5`؛ CQRS منظم تحت `Features/`. |
| B — العقود والـCQRS والتحقق | ✅ منفذ بنيوياً | 39 validators لـ39 commands؛ `ValidationBehavior.cs:16-34` مسجل في `DependencyInjection.cs:20`. التحقق التشغيلي يتطلب تشغيل الاختبارات. |
| C — DTOs وmapping | 🟡 جزئي | 49 DTO و14 profile و31 map؛ IMapper واختبار config موجودان؛ mapping اليدوي واستراتيجية مختلطة باقيان. |
| D — خدمات Domain وتكامل المعالجات | 🟡 جزئي | العشر خدمات مسجلة؛ 5 لها مستهلكات؛ 5 بلا مستهلكات. الأدلة التفصيلية في قسم الخدمات أعلاه. |
| E — الأداء والإلغاء | 🟡 جزئي | لا `GetAllAsync` أو `SaveChangesAsync()` بلا CT؛ LabId improved. لا دليل على معالجة سباق توليد Lab ID أو اختبارات أداء/تكامل. |
| F — الاختبارات والتكامل | ❌ غير منفذ بالقدر المطلوب | 14 اختبارات معلنة فقط؛ لا اختبارات handler/service/audit/LabId/DI/integration. التنفيذ الفعلي للاختبارات لم يكن متاحاً لغياب dotnet. |
| G — حدود العرض والتوثيق والأحداث | ❌ غير منفذ | لا MediatR أو ValidationException في Presentation؛ لا notification handlers؛ placeholders وقرار/عقد الخطأ غير مغلقين. |

---

# مصفوفة التصحيحات C1–C14

| التصحيح | الحالة | الدليل |
|---|---|---|
| C1 إزالة blocking async | ✅ | بحث `GetAwaiter().GetResult()` في `src/` صفري؛ أسطح الخدمة async مثل `ResultValidationService.cs:25-55`. |
| C2 منع السعر الصفري الصامت | 🟡 | يتحقق من `priceListId` في `PriceListResolverService.cs:24-29`، لكنه يعيد صفر عند غياب بند السعر في `:31`. |
| C3 تسجيل الخدمات العشر | ✅ | `DependencyInjection.cs:25-34`. |
| C4 تسجيل AuditBehavior بعد Validation | ✅ | `DependencyInjection.cs:20-21`. |
| C5 تدقيق persisted حقيقي | 🟡 | `AuditBehavior.cs:56-66` ينشئ ويحفظ log؛ لا اختبار، والاستثناءات تُبتلع في `:68-70`. |
| C6 مدقق AddCaseFollowUp | ✅ | `AddCaseFollowUpCommandValidator.cs:5-11`. |
| C7 نطاق الجنس/العمر | ✅ | `ResultValidationService.cs:25-31`, `:49-55`, `:71-84`. |
| C8 نشر CancellationToken | ✅ في النطاق الساكن | البحث المستهدف بلا `GetAllAsync()` ولا `SaveChangesAsync()`؛ أمثلة الحفظ كلها تمرر CT. |
| C9 LabId بدون تحميل الجدول | 🟡 | `LabIdGenerator.cs:17-20` يستخدم max suffix؛ لا معالجة مؤكدة للتزامن/فريدية. |
| C10 حسم mapping | 🟡 | config test موجود: `MappingConfigurationTests.cs:9-17`؛ استراتيجية مختلطة ظاهرة في `GetPatientHistoryQueryHandler.cs:21-39`. |
| C11 تنظيف الآثار الراكدة | 🟡 | تعليق MedicalHistory سليم؛ لكن placeholders واسم `PlaceholderTests.cs` باقيان. |
| C12 قواعد تحقق مشتركة | ❌ | `Common/Validations/_Placeholder.cs:1-8` فقط؛ لا ملفات قواعد مشتركة. |
| C13 عقد Result/error boundary | ❌ | `Common/Models/_Placeholder.cs:1-8` فقط؛ Presentation بلا `ValidationException`. |
| C14 إغلاق قرار الأحداث/التنفيذ | ❌ | لا `INotificationHandler` في Application؛ لا دليل على إغلاق قرار أحداث تشغيلي. |

---

# الديون التقنية TD-1 إلى TD-16

| المعرف | الحالة | قرار التدقيق |
|---|---|---|
| TD-1 blocking sync-over-async | ✅ مغلق | بحث المصدر صفري. |
| TD-2 سعر صفري عند تعذر الحل | 🟡 مفتوح | `PriceListResolverService.cs:31`. |
| TD-3 خدمات Domain غير مسجلة | ✅ مغلق | 10 registrations في `DependencyInjection.cs:25-34`. |
| TD-4 Audit stub/unregistered | 🟡 | التسجيل والمنطق موجودان، لكن الاختبارات مفقودة وفشل التخزين مخفي. |
| TD-5 sync range يتجاهل الجنس/العمر | ✅ مغلق | السطح الحي async ويستخدم المدخلين. |
| TD-6 full-table scans / LabId | 🟡 | لا GetAllAsync في Application؛ سباق LabId غير مثبت المعالجة. |
| TD-7 AutoMapper غير مستخدم/غير محمي | 🟡 | مستخدم ومختبر config، لكن ليس موحداً ولا مشمولاً كاملاً. |
| TD-8 CT مسقط | ✅ في نطاق المسح | لا save بلا CT ولا استدعاءات async مرصودة بلا CT في Application. |
| TD-9 أربع خدمات Infrastructure stub | ❌ مفتوح | 7 throws: Authentication `:10,:15`؛ Backup `:9,:14`؛ Barcode `:9`؛ Print `:9,:14`. |
| TD-10 مدقق AddCaseFollowUp | ✅ مغلق | الملف والقواعد موجودة. |
| TD-11 أحداث Domain بلا handlers | ❌ مفتوح | بحث `INotificationHandler` في Application صفري؛ توجد ملفات أحداث Domain. |
| TD-12 shared validation/placeholders | ❌ مفتوح | placeholder files موجودة ولا قواعد مشتركة. |
| TD-13 تعليق MedicalHistory قديم | ✅ مغلق | `MedicalHistoryService.cs:17-33` يصف المستودع الحي بلا ادعاء تعطل. |
| TD-14 اختبارات Application ضعيفة واسم مضلل | ❌ مفتوح | 14 اختبارات معلنة في ملفين، و`PlaceholderTests.cs` باقٍ. |
| TD-15 قرار الأحداث غير مغلق | ❌ مفتوح | لا handler ولا دليل قرار تشغيلي يغطي كل event. |
| TD-16 عقد خطأ على حد Presentation | ❌ مفتوح | `ValidationBehavior.cs:30-31` يرمي exception؛ بحث Presentation عن `ValidationException`/MediatR صفري. |

---

# النتائج المعمارية

## نقاط القوة

1. **فصل طبقي قابل للتحقق:** Application تعتمد على Domain ولا تستورد Infrastructure؛ وهو أساس قابل للاستمرار لعمارة نظيفة.
2. **التسجيل صار متسقاً مع العقود:** الخدمات العشر والسلوكان مسجلون في نقطة تركيب واحدة (`DependencyInjection.cs:17-34`).
3. **انضباط async تحسن جذرياً:** توقيعات الخدمات الحية تمرر CT، وعمليات الحفظ المرصودة تمرره أيضاً.
4. **Business rules بدأت تنتقل إلى services:** مثال result validation/history في معالج واحد (`EnterTestResultCommandHandler.cs:30-46`) وتكامل الحساسية/outsourcing/mحاسبة.
5. **طبقة mapping لها فحص صحة:** `AssertConfigurationIsValid()` يمنع تكويناً غير صالح من المرور صامتاً عند تشغيل الاختبارات.

## المخاطر المعمارية الباقية

1. **خدمات غير مستهلكة = ازدواجية أو شفرة ميتة.** تسجيل الخدمة لا يثبت أن قواعد التسعير/العمولات/العينات تطبق في flow المستخدم.
2. **السعر 0 مخرج مالي غير آمن افتراضياً.** يجب أن يُمثل «لا سعر» نتيجة صريحة أو exception domain، لا قيمة مالية ممكنة الالتباس.
3. **Audit لا يجب أن يخفي تعذر حفظه دون سياسة معلنة.** إخفاء الخطأ قد يكون مناسباً لتوافر الطلب، لكنه لا يحقق متطلب قابلية التتبع ما لم توجد مراقبة/بديل.
4. **سقف اختبار منخفض.** أي تعديل في DI أو الخدمات أو mapping قد ينجح في compile ويخفق عند runtime بلا كشف مبكر.
5. **Presentation لا يثبت استخدام Application.** وجود composition root (`Presentation/App.xaml.cs:27-38`) لا يعني أن ViewModels ترسل requests عبر MediatR أو تعالج failures.

---

# تبعيات عبر الطبقات (Cross-Layer Dependencies)

| مجال Application المتأثر | العمل المطلوب في الطبقة الأخرى | سبب التبعية ونوعها |
|---|---|---|
| تدفق تسجيل الدخول/الخروج | **Infrastructure:** تنفيذ `AuthenticationService.LoginAsync` و`LogoutAsync` بدلاً من throws في `Infrastructure/Services/AuthenticationService.cs:8-15`. **Presentation:** ربط شاشة الدخول بالعقد وواجهة الأخطاء. | شرط سابق لإثبات تدفق مصادقة كامل، رغم أن معالجات الحضور نفسها قد تبني سجلات محلية. |
| النسخ الاحتياطي | **Infrastructure:** تنفيذ `BackupService.BackupAsync/RestoreAsync` (`BackupService.cs:7-15`). **Presentation:** اختيار المسار وإظهار نتيجة/خطأ العملية. | `ManageBackup` في Application لا يحقق عملية نسخ حقيقية إذا كان التنفيذ السفلي يرمي استثناء. |
| الباركود والتقارير/الطباعة | **Infrastructure:** تنفيذ `BarcodeService.GenerateBarcode` و`PrintService.PrintAsync/RenderAsync` (`BarcodeService.cs:7-14`, `PrintService.cs:7-14`). **Presentation:** استهلاك المخرجات وعرض/طباعة التقارير. | شرط لاحق لتدفقات وثائق ونتائج قابلة للمستخدم؛ لا يكفي العقد المسجل. |
| AuditBehavior | **Infrastructure:** استمرار توفير `IRequestAuditLogRepository` و`IUnitOfWork`، وكلاهما مسجلان في `Infrastructure/DependencyInjection.cs:36-39,58-59`. | شرط تركيب مسبق لنجاح تدقيق runtime؛ يجب اختباره بحاوية تجمع `AddApplication` و`AddInfrastructure`. |
| عقد أخطاء التحقق | **Presentation:** اعتماد ISender/IMediator في ViewModels أو adapter واضح، ومعالجة `ValidationException` عند الحد. | `ValidationBehavior` يرمي الاستثناء في `ValidationBehavior.cs:30-31`؛ لا توجد معالجة حالية في Presentation، لذلك تبعية لاحقة قبل تجربة مستخدم صحيحة. |
| تحسين استعلامات LabId والبيانات | **Infrastructure:** تأكيد unique index أو transaction/retry لرقم المختبر، وتنفيذ استعلام `GetMaxLabIdSuffixAsync` بكفاءة وذرية. | Application يقرأ ثم يزيد (`LabIdGenerator.cs:18-20`)؛ مقاومة السباق لا تُحسم في هذه الطبقة وحدها. |
| التكامل واختبارات E2E | **Infrastructure:** DbContext/provider وrepositories حقيقية للاختبار. **Presentation:** اختبارات تدفقات VM عندما تعتمد MediatR. | شرط لاحق للتحقق من pipeline والواجهة، وليس عملاً يمكن لطبقة Application إثباته وحدها. |

---

# خارطة الطريق المتبقية المعاد بناؤها

## المرحلة 1 — إغلاق سلامة قواعد الأعمال غير الموصولة

**الهدف:** جعل كل خدمة Domain مسجلة ذات أثر وظيفي متعمد، وإزالة النتائج المالية الغامضة.

**المهام:**

1. حدد لكل من `IPriceListResolverService` و`IPricingService` و`IReceiptCalculationService` و`IReferralCommissionService` و`ISampleTrackingService` معالجاً أو سبب إزالة موثقاً.
2. اربط التسعير والخصم/المتبقي والعمولة والتحصيل بالمعالجات المناسبة، مع CT حيث ينطبق.
3. غيّر `ResolvePriceAsync` بحيث يميز «بند غير موجود» صراحةً: exception domain أو نوع نتيجة معلوم، ولا يرجع `0` إلا إذا كان سعر صفر قاعدة مكتوبة ومختبرة.
4. أضف اختبارات وحدات لكل سلوك خدمة، خصوصاً missing price، نطاقات المرجع، العمولة، والعينة غير المحصلة.

**معايير القبول:** بحث كل واجهة من الخدمات العشر في Features يعيد مستهلكاً مبرراً؛ لا مسار مالي يحول غياب السعر إلى صفر بلا سياسة؛ tests تغطي الحالات الحرجة.

**التبعيات:** لا توجد لتوصيل الخدمات داخل Application، لكن تدفقات UI الكاملة تعتمد على Presentation.

## المرحلة 2 — تثبيت سياسة mapping والتدقيق

**الهدف:** منع ازدواجية mapping وفقد Audit دون رؤية.

**المهام:**

1. اختر قاعدة صريحة: AutoMapper لكل التحويلات البسيطة أو manual mapping فقط للحالات الحسابية/المركبة.
2. أكمل الخرائط أو احذف profiles غير المستعملة؛ راجع كل DTO عائد من handler.
3. أبقِ اختبار `AssertConfigurationIsValid()` وأضف اختبارات mapping للحالات ذات الحقول الحساسة/nulls.
4. أضف اختبارات AuditBehavior: نجاح، فشل handler، وfailure في audit persistence وفق سياسة متفق عليها.
5. قرر هل فشل audit مسموح أن لا يوقف الطلب، وإن كان كذلك سجله عبر logging/telemetry أو مسار موثوق بديل.

**معايير القبول:** استراتيجية واحدة مرئية؛ تغطية mapping موثقة؛ audit outcome مثبت باختبارات.

## المرحلة 3 — صلابة البيانات والإلغاء والتزامن

**الهدف:** الحفاظ على التحسن الحالي مع سد سباق Lab ID والتحقق منه عملياً.

**المهام:**

1. أضف ضماناً للتفرد في Infrastructure (unique index أو معاملة/retry) لمعرف Lab ID.
2. اختبر توليداً متوازياً يكشف التصادم ويثبت الاستراتيجية المختارة.
3. أنشئ قائمة مراجعة آلية تمنع عودة `GetAllAsync` غير المقيد و`SaveChangesAsync` بلا CT داخل Application.
4. اختبر الاستعلامات ذات النطاق الزمني والfilters على provider قريب من الإنتاج.

**معايير القبول:** اختبار موازاة أخضر؛ لا استدعاءات محظورة؛ CT يصل إلى repository/UoW في حالات الاختبار.

**التبعيات:** Infrastructure شرط سابق لآلية التفرد والاختبار الحقيقي للـprovider.

## المرحلة 4 — حزمة الاختبارات والتكامل

**الهدف:** نقل الثقة من مراجعة ساكنة إلى إثبات قابل للتكرار.

**المهام:**

1. تثبيت .NET SDK 8 في بيئة CI/التطوير وتشغيل `dotnet build` و`dotnet test` مع warnings-as-errors.
2. اختبارات handler happy/failure paths للأوامر والاستعلامات ذات الأولوية.
3. اختبارات service وvalidator وLabId وAudit وDI resolution.
4. اختبارات integration لحاوية `AddApplication + AddInfrastructure` وDbContext: تسجيل مريض، إدخال نتيجة، حساسية، outsource، ومعاملة نقدية.
5. اختبار أن validation failure لا يكتب قاعدة البيانات وأن audit policy تعمل كما قرر الفريق.

**معايير القبول:** البناء والاختبارات قابلة للتشغيل في CI؛ لا تعتمد اختبارات Application على افتراضات غير محاكة؛ التكامل يثبت الحاوية كاملة.

## المرحلة 5 — إغلاق حدود Presentation والأحداث والتوثيق

**الهدف:** تشغيل Application من WPF بعقد خطأ متعمد وإزالة آثار scaffolding.

**المهام:**

1. اعتماد مسار ISender/IMediator من ViewModels أو adapter مكافئ واضح.
2. معالجة `ValidationException` والاستثناءات domain عند حدود UI، برسائل وتجربة مستخدم محددتين.
3. جرد أحداث Domain وتحديد handler أو defer صريح لكل حدث؛ أضف `INotificationHandler` للحالات المعتمدة.
4. تنفيذ Authentication/Backup/Barcode/Print في Infrastructure ثم اختبارها من طبقة العرض.
5. إضافة قواعد تحقق مشتركة وقرار error contract، ثم حذف `_Placeholder.cs` وإعادة تسمية `PlaceholderTests.cs`.

**معايير القبول:** لا placeholders؛ لا حدث بلا قرار؛ ViewModels تستدعي Application وتتعامل مع failures؛ الخدمات الأربع لا ترمي `NotImplementedException`.

---

# تقييم الجاهزية النهائي

| البعد | التقييم | الحكم |
|---|---|---|
| البنية والعقود | ✅ جاهز | الحدود وCQRS وDI الأساسية سليمة ساكناً. |
| صحة async/Cancellation | ✅ جاهز في النطاق الساكن | أزيل blocking shim وتظهر CT في العمليات المرصودة. |
| قواعد الأعمال | 🟡 غير مكتملة | خدمات كثيرة موصولة، لكن خمس خدمات بلا استعمال وتسعير missing item غير آمن. |
| mapping | 🟡 غير مكتمل | config validity واستخدام موجودان؛ لا سياسة موحدة أو تغطية مكتملة. |
| audit | 🟡 غير مثبت | تنفيذ وتسجيل موجودان؛ لا اختبارات وسياسة failure تخفي الأعطال. |
| قابلية التوسع/التزامن | 🟡 غير مكتمل | تحسن query shape، لكن LabId concurrency غير مثبت. |
| الاختبار والبناء | 🔴 غير جاهز | التغطية محدودة ولا اختبارات تكامل؛ لم يمكن تشغيل SDK في بيئة التدقيق. |
| التكامل عبر الطبقات | 🔴 غير جاهز | Presentation لا تستخدم MediatR ولا تعالج validation؛ 4 خدمات Infrastructure ما زالت stubs. |
| الأحداث والوثائق النهائية | 🔴 غير جاهز | لا notification handlers ولا عقد خطأ مغلق وplaceholders باقية. |

## الحكم

طبقة Application **صالحة كأساس متماسك ومتحسن وظيفياً، لكنها ليست جاهزة بعد كطبقة إنتاج مكتملة**. الأولوية ليست إضافة scaffolding جديد؛ بل إتمام ربط القواعد غير المستهلكة، جعل missing-price آمناً، إثبات Audit وLabId تحت الاختبار، ثم إغلاق حدود Infrastructure/Presentation. لا ينبغي إعلان الجاهزية النهائية قبل اجتياز المرحلة 4 على بيئة مزودة بـ .NET SDK، وتنفيذ التبعيات الحرجة في المرحلة 5.
