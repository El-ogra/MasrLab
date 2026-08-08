# سجل قرارات المرحلة 1 من خارطة V4 — MasrLab

> سجل موحّد لكل قرارات ونتائج العمل المنفَّذ في **المرحلة 1** من `Docs/Application-Layer-Implementation-Roadmap-V4.md` (إغلاق سلامة قواعد الأعمال غير الموصولة + الأمان المالي + حارس العينة).

- **الحالة:** منفَّذ وموثَّق (لم يُعتمد بعد من صاحب الأعمال — توجد أسئلة مفتوحة Q1–Q6 تتطلب قرارًا)
- **التاريخ:** 2026-08-09
- **المرحلة المرجعية:** `Docs/Application-Layer-Implementation-Roadmap-V4.md` → «المرحلة 1 — إغلاق سلامة قواعد الأعمال غير الموصولة» (سطر 223-237)
- **نقطة تثبيت الشجرة (HEAD):** `12ea8cb` — «وضع الوثيقة الجديدة»
- **نطاق السجل:** كل ما وُجد في شجرة العمل غير الملتزمة عند تثبيت هذا السجل (13 ملفًا معدَّلًا + 7 ملفات جديدة)، معزولًا عن أي التزام سابق
- **دليل الموثوقية:** `git diff` مقابل `HEAD` هو مصدر الحقيقة؛ أي انحراف بين الوصف المنصوص في المهمة والسلوك الفعلي على القرص مُعلَن في «ملاحظات الدقة» أدناه

---

## 0. نظرة عامة (Overview)

أُغلقت في هذه المرحلة فجوات محددة من خارطة V4:

1. **المهمة A — سلامة التسعير:** إزالة «السعر الصفري الصامت» (بند C2 / TD-2 في V4) بحيث يكون غياب بند السعر **استثناء مجال** صريحًا لا قيمة مالية، مع رفض أي سعر سالب عند تعديل قائمة الأسعار.
2. **المهمة B — خصم العمولات من صافي النشاط:** ربط `IReferralCommissionService` (التي كانت مسجلة بلا مستهلك) بخدمة المحاسبة، وجعل «صافي النشاط» يُحتسب صراحةً كـ `TotalIncome − TotalDiscount − CommissionsTotal`.
3. **المهمة C — حارس العينة وتسجيل التجاوز:** منع إدخال نتيجة لفحص عينته غير محلولة، مع مسار استثنائي موثَّق (سبب نصي) يُحفظ مع النتيجة نفسها، وربط `ISampleTrackingService` بمعالج الإدخال لأول مرة.

كما أُضيفت **23 اختبارًا** جديدًا لطبقة Application (من 14 إلى 37)، مع بقاء جميع الاختبارات القائمة خضراء (المجموع 213).

---

## 1. المهمة A — سلامة التسعير (إغلاق «الصفر الصامت»)

### السياق (Context)

- خارطة V4 سطر 16: «ما زال `PriceListResolverService` يعيد `0` عند غياب بند السعر … وهذا غير كافٍ لمسار مالي ما لم يكن الصفر قاعدة مجال صريحة» (بند C2/TD-2، سطر 148/169 في V4).
- قبل هذه المرحلة كان السطر الأخير من `ResolvePriceAsync` هو `return item?.Price ?? 0;` — أي أن «لا بند» و«بند بقيمة صفر» ينتجان نفس القيمة (0) دون تمييز.
- مدقق `UpdatePriceListItemsCommandValidator` لم يكن يحرس الأسعار السالبة إطلاقًا.

### القرار (Decision)

1. **غياب بند السعر = استثناء مجال صريح** — `ResolvePriceAsync(int testId, int priceListId, CancellationToken ct)`:
   - `src/MasrLab.Application/Services/PriceListResolverService.cs:27-28` — يرفض `priceListId <= 0` باستثناء `BusinessRuleViolationException`.
   - `:32-34` — إذا كان بند السعر `null` يرمي `BusinessRuleViolationException` بصيغة واضحة (`No price item found for test …`).
   - `:36` — يعيد `item.Price` فقط عند وجود بند فعلي (القيمة الصفرية مسموحة ومقصودة لبند موجود).
   - التوثيق في XML doc: `:20-24`.
2. **رفض الأسعار السالبة عند تعديل القائمة** — `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListItems/UpdatePriceListItemsCommandValidator.cs:12-14`:
   ```csharp
   RuleForEach(x => x.Items)
       .Must(item => item.Price >= 0)
       .WithMessage("Price cannot be negative.");
   ```
   الصفر مسموح (اختبار مجاني/هدية)، والسالبة مرفوضة.

### البدائل المطروحة (Alternatives Considered)

- **الإبقاء على `item?.Price ?? 0`:** مرفوض — يخلط «لا بند» بـ«بند صفر» ويترك مخرجًا ماليًا غامضًا (المعيار نفسه في V4).
- **إرجاع `decimal?` لتمييز الغياب:** مؤجل — تغيير نوع العائد لعقد Domain، ولم يكن مطلوبًا لإغلاق المخاطرة في هذه المرحلة؛ السؤال مفتوح في Q1.
- **قاعدة مجال «الصفر مسموح للبند الموجود»:** مقبول — الصفر قيمة مشروعة لبند فعلي، لكنه ليس بديلًا عن «لا بند».

### النتائج (Consequences)

- لا يوجد الآن أي مسار داخل الخدمة يحوّل غياب السعر إلى صفر؛ أي استدعاء لبند مفقود يفشل صراحةً.
- `UpdatePriceListItemsCommandValidator` يمنع الأسعار السالبة من الوصول للمخزَّن.
- 4 اختبارات جديدة: `PriceListResolverServiceTests.cs` — بند إيجابي (`:18-29`)، بند صفر (`:31-42`)، بند مفقود يرمي (`:44-54`)، priceListId غير صالح (`:56-63`).
- 3 اختبارات جديدة: `UpdatePriceListItemsCommandValidatorTests.cs` — سالب مرفوض (`:9-19`)، صفر مقبول (`:21-31`)، موجب مقبول (`:33-43`).

### توثيق التتبع (Audit traceability)

- التعديل يُنفَّذ عبر أمر يمر بخطّ التدقيق المركزي (`AuditBehavior`) فيُسجَّل `RequestName + UserId + ActionTimeUtc + Success/ErrorMessage` (راجع القسم 5).
- القيمة نفسها غير قابلة للضرب من سجل التدقيق (الخطّ لا يسلسل حمولة الطلب)؛ لكن السعر المُراجع لاحقًا يُستعاد من جدول البنود نفسه لأنه قيمة ثابتة في السجل.

### الروابط (Links)

- `src/MasrLab.Application/Services/PriceListResolverService.cs`
- `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListItems/UpdatePriceListItemsCommandValidator.cs`
- `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListItems/UpdatePriceListItemsCommand.cs` (العقد: `:5`, `PriceListItemData:7`)
- `tests/MasrLab.Application.Tests/PriceListResolverServiceTests.cs`
- `tests/MasrLab.Application.Tests/UpdatePriceListItemsCommandValidatorTests.cs`

---

## 2. المهمة B — خصم عمولات المحوِّلين من صافي النشاط

### السياق (Context)

- `IReferralCommissionService` كانت مسجلة في DI لكن بلا أي مستهلك (V4 سطر 95-103)، و`IAccountingService.CalculateNetProfit(Account)` كانت تحسب `TotalIncome − TotalDiscount` فقط بلا عمولات (الملف القديم قبل الـ diff).
- معادلة `Account.NetProfit` الموثقة سابقًا كانت أوسع (`TotalIncome − TotalDiscount − TotalOutsourcedCost − CashWithdrawals + CashDeposits`) لكن التعليق في `Account.cs` وصفها بأنها «Domain Service phase، not yet implemented» (حالة ما قبل الـ diff).
- الحسابات المالية تُنشأ عبر أوامر الصرف (`CreateDoctorDrawer`، `CreatePeriodDrawer`، `CreateAccountTypeDrawer`) وبعضها مقترن بطبيب (`DoctorId`).

### القرار (Decision)

1. **صيغة صافي النشاط الجديدة:** `NetProfit = TotalIncome − TotalDiscount − CommissionsTotal` حيث `CommissionsTotal = إجمالي عمولات الطبيب المُحيل` كبند مستقل وصريح:
   - `src/MasrLab.Domain/Services/IAccountingService.cs:7` — العقد: `decimal CalculateNetProfit(Account account, decimal commissionsTotal)`.
   - `src/MasrLab.Application/Services/AccountingService.cs:32-35` — الحساب النقي: `account.TotalIncome - account.TotalDiscount - commissionsTotal`.
2. **ربط خدمة العمولة بخدمة المحاسبة:**
   - `AccountingService.cs:16` حقن `IReferralCommissionService`، و`:18-26` البناء.
   - `:62-68` — `CalculateCommissionsAsync`: إن لم يكن للحساب `DoctorId` فلا عمولة (`:64-65`)، وإلا تُحتسب عبر `CalculateCommissionAsync(account.DoctorId, account.TotalIncome, ct)` (`:67`).
   - **قرار أساس العمولة:** إجمالي دخل الحساب للفترة `Account.TotalIncome` (وليس الإيراد بعد الخصم) — موثَّق في تعليق `:59-60`.
3. **إعادة الاحتساب التلقائي:** `RecalculateNetProfitAsync(int accountId, ct)`:
   - `:42-55` — الخطوة 1: حساب العمولات (`:48-49`)؛ الخطوة 2: خصمها وتخزين اللقطة (`:52`)؛ ثم `Update` + `SaveChangesAsync(ct)` (`:53-54`).
   - يُستدعى من معالج القيد النقدي `RecordCashTransactionCommandHandler` (كان يستهلك `RecalculateNetProfitAsync` مسبقًا حسب V4 سطر 84).
4. **`Account.NetProfit`:** يبقى قيمة لقطة (snapshot) تُخزَّن عند إقفال الفترة؛ تحديث التعليق التوثيقي في `src/MasrLab.Domain/Entities/Financial/Account.cs:12-14`.
5. **`ReferralCommissionService`:** دون تغيير سلوكي — `CalculateCommissionAsync` تقرأ `Doctor.CommissionPercent` (المقيد بين 0 و100 في `src/MasrLab.Domain/Entities/Administrative/Doctor.cs:14,20`) وتُرجع `visitTotal × (percent/100)` عند وجود الطبيب وإلا 0 (`src/MasrLab.Application/Services/ReferralCommissionService.cs:24-34`).

### البدائل المطروحة (Alternatives Considered)

- **حساب العمولة داخل `Doctor` / `Account` نفسيهما:** مرفوض — يتطلب وصولًا للبيانات من كيان متزامن؛ الجلب غير المتزامن بقي في طبقة الخدمات.
- **إبقاء توقيع `CalculateNetProfit(Account)` وإضافة خصم داخل الطريقة:** مرفوض — لا مصدر للعمولات المتزامن داخل طريقة نقيّة؛ تغيير التوقيع جعل «بند العمولة» صريحًا في عقد Domain.
- **خصم العمولات من أساس `TotalIncome − TotalDiscount`:** مؤجل/سؤال — انظر Q2.

### النتائج (Consequences)

- `IReferralCommissionService` أصبحت مستهلكة وظيفيًا (عبر `AccountingService` → `RecordCashTransactionCommandHandler`) — يقفل جزءًا من فجوة «الخدمات المسجلة بلا مستهلك» في V4.
- صافي نشاط أي حساب مقترن بطبيب يخصم الآن عمولته؛ الحسابات بلا طبيب بلا خصم.
- **ملاحظة صيغة:** استُبدلت المعادلة الأوسع السابقة (`− TotalOutsourcedCost − CashWithdrawals + CashDeposits`) بالمعادلة الجديدة — قرار يحتاج اعتماد أعمال (Q3).
- 7 اختبارات جديدة: `AccountingServiceTests.cs` — بدون عمولة (`:25-33`)، عمولة موجبة (`:35-43`)، حد العمولة يساوي الأساس (`:45-53`)، بلا طبيب لا عمولة (`:55-67`)، بوجود طبيب خصم (`:69-83`)، طبيب بصفر (`:85-97`)، حساب غير موجود (`:99-109`).
- 5 اختبارات جديدة: `ReferralCommissionServiceTests.cs` — طبيب null (`:17-25`)، طبيب غير موجود (`:27-38`)، نسبة 0 (`:40-51`)، نسبة 100 (`:53-64`)، نسبة 10 (`:66-77`).

### توثيق التتبع (Audit traceability)

- الاحتساب يحدث عند كل قيد نقدي، والقيد نفسه يُسجَّل في خطّ التدقيق (`AuditBehavior`) بمستخدم وتوقيت ونجاح/فشل.
- الصافي لا يُخزَّن كإدخال تدقيق مستقل؛ بل يُعاد اشتقاقه من `TotalIncome − TotalDiscount − CommissionsTotal`، وكل مكوناته قابلة لإعادة الحساب من البيانات المحفوظة.

### الروابط (Links)

- `src/MasrLab.Domain/Services/IAccountingService.cs`
- `src/MasrLab.Domain/Services/IReferralCommissionService.cs`
- `src/MasrLab.Application/Services/AccountingService.cs`
- `src/MasrLab.Application/Services/ReferralCommissionService.cs`
- `src/MasrLab.Domain/Entities/Financial/Account.cs`
- `src/MasrLab.Domain/Entities/Administrative/Doctor.cs`
- `src/MasrLab.Application/Features/Accounting/Commands/RecordCashTransaction/RecordCashTransactionCommandHandler.cs`
- `tests/MasrLab.Application.Tests/AccountingServiceTests.cs`
- `tests/MasrLab.Application.Tests/ReferralCommissionServiceTests.cs`

---

## 3. المهمة C — حارس العينة وتسجيل التجاوز الموثَّق

### السياق (Context)

- معالج إدخال النتيجة `EnterTestResultCommandHandler` كان يحقن ثلاث خدمات فقط ولا يتحقق من حالة العينة إطلاقًا: يمكن إدخال نتيجة لفحص عينته غير محلولة دون أي رقيب.
- `ISampleTrackingService` كانت مسجلة في DI بلا أي مستهلك (V4 سطر 95-103).
- لا توجد وسيلة تتبع «سبب التجاوز» لاحقًا؛ خطّ التدقيق المركزي لا يسلسل حمولة الطلب (راجع القسم 5).

### القرار (Decision)

1. **إضافة سبب التجاوز للأمر:** `EnterTestResultCommand` اكتسب معاملًا موضعيًا أخيرًا `string? OverrideReason` — `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommand.cs:16`.
2. **تقييد الطول في المدقق:** `EnterTestResultCommandValidator.cs:27-28` — `OverrideReason.MaximumLength(500)` (لا قاعدة جنس/فصل إضافية أُضيفت فعليًا — انظر ملاحظات الدقة).
3. **الحارس في المعالج** (`EnterTestResultCommandHandler.cs`):
   - `:37-38` — حل `VisitTest` عبر `IVisitRepository.GetVisitTestAsync(VisitTestId, ct)`؛ إن غاب → `InvalidOperationException` (اتساقًا مع المعالجات الشقيقة).
   - `:40-43` — فحص `ISampleTrackingService.IsSampleCollectedAsync(PatientVisitId, TestId, ct)`.
   - `:45-50` — **الحارس قبل التحقق (fail fast):** إذا كانت العينة غير محلولة **و** السبب فارغ/مسافات → `BusinessRuleViolationException` («Provide an override reason to proceed»).
   - `:59-64` — إنشاء النتيجة عبر `TestResult.Enter` ثم تعيين `Unit/ReferenceRange/Status/OverrideReason`.
   - `:66-67` — `AddAsync` + `SaveChangesAsync(ct)`.
   - `:69` — استدعاء التاريخ الطبي `ShouldAutoInsertHistoryAsync(PatientId, VisitTestId, ct)`.
4. **تخزين السبب مع النتيجة:** خاصية `OverrideReason` في `src/MasrLab.Domain/Entities/Core/TestResult.cs:18-22` (مع تعليق يصرّح بأنها مسار تدقيق محفوظ بجانب النتيجة، تبقى `null` في الإدخال العادي).
5. **خريطة EF وعمود DB:**
   - `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs:22` — `OverrideReason.HasMaxLength(500)`.
   - ميجريشن جديد `20260808223709_AddOverrideReasonToTestResult` — `AddColumn` عمود `nvarchar(500)` قابل للنول في جدول `TestResults` (`20260808223709_AddOverrideReasonToTestResult.cs:13-18`)، والتراجع `DropColumn` (`:24-26`)، وتحديث اللقطة `MasrLabDbContextModelSnapshot.cs` (~سطر 990-993).
6. **عقد المستودع:** `IVisitRepository.GetVisitTestAsync(int, CancellationToken)` — `src/MasrLab.Domain/Interfaces/IVisitRepository.cs:7`؛ والتنفيذ `AsNoTracking().FirstOrDefaultAsync` — `src/MasrLab.Infrastructure/Persistence/Repositories/VisitRepository.cs:14-19`.

### البدائل المطروحة (Alternatives Considered)

- **منع الإدخال كليًا بدون استثناء:** مرفوض — هناك حالات مشروعة (مثل عينة محجوزة تُسحب دفعةً واحدة أو إدخال متأخر) تحتاج مسارًا موثقًا.
- **اكتفاء خطّ التدقيق المركزي بتوثيق التجاوز:** مرفوض — `AuditBehavior` لا يسلسل حمولة الطلب ولا السبب؛ التخزين الصريح في `OverrideReason` هو الأثر الدائم الوحيد (راجع القسم 5).
- **وضع شرط السبب في المدقق بدل المعالج:** مرفوض — الشرط يعتمد على حالة العينة (غير قابلة للتحقق في المدقق النقي)؛ المدقق يقيّد الطول فقط والحارس في المعالج.

### النتائج (Consequences)

- لا إدخال لنتيجة على عينة غير محلولة إلا بسبب موثَّق غير فارغ؛ كل محاولة بدون سبب تُرفض قبل أي كتابة.
- `ISampleTrackingService` أصبح لها مستهلك فعلي في Features (`EnterTestResultCommandHandler.cs:16,24,40-43`) — يقفل جزءًا من فجوة V4.
- سبب التجاوز محفوظ دائمًا مع النتيجة وقابل للاستعلام لاحقًا عبر مسارات `TestResult` القائمة.
- **ملاحظة سلوكية:** الحالة (`Status`) في التنفيذ الفعلي هي ناتج `ValidateResultAsync` دائمًا (وليس مفروضًا `Abnormal` عند التجاوز)، و`OverrideReason` تُكتب حتى في المسار العادي (بقيمة null) — راجع Q4 وملاحظات الدقة.
- 4 اختبارات جديدة: `EnterTestResultCommandHandlerTests.cs` — إدخال عادي لعينة محلولة (`:66-85`)، رفض بدون سبب (`:87-99`)، قبول مع سبب وحفظه (`:101-120`)، رفض السبب الفارغ (`:122-134`).

### توثيق التتبع (Audit traceability)

- **أثر دائم:** العمود الجديد `TestResults.OverrideReason` يحفظ السبب مع النتيجة، و`EnteredByUserId` يحدد من أدخل، و`EnteredAt` التوقيت — قابل للاستعلام دون الاعتماد على سجلات التدقيق.
- **سجل الطلب:** أمر `EnterTestResult` يمر عبر `AuditBehavior`؛ الرفض (بدون سبب) يُسجَّل كطلب فاشل (`Success=false` + نص الاستثناء في `ErrorMessage`)؛ والنجاح كطلب ناجح.
- **حدود الخطّ:** `AuditBehavior` يسجل `RequestName/UserId/ActionTimeUtc/Success/ErrorMessage` فقط (راجع القسم 5) ولا يلتقط السبب — لذا كان التخزين الصريح إلزاميًا.

### الروابط (Links)

- `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommand.cs`
- `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandValidator.cs`
- `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandHandler.cs`
- `src/MasrLab.Domain/Entities/Core/TestResult.cs`
- `src/MasrLab.Domain/Services/ISampleTrackingService.cs`
- `src/MasrLab.Domain/Interfaces/IVisitRepository.cs`
- `src/MasrLab.Infrastructure/Persistence/Repositories/VisitRepository.cs`
- `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs`
- `src/MasrLab.Infrastructure/Persistence/Migrations/20260808223709_AddOverrideReasonToTestResult.cs`
- `src/MasrLab.Infrastructure/Persistence/Migrations/MasrLabDbContextModelSnapshot.cs`
- `tests/MasrLab.Application.Tests/EnterTestResultCommandHandlerTests.cs`

---

## 4. القرارات المعمارية المتقاطعة وقرارات ضمن الحدود

1. **لا فرض حد أدنى لصافي النشاط:** بعد خصم العمولات قد يكون الصافي سالبًا؛ لم يُفرض `Math.Max(0, …)` في المحاسبة لأنه سيدفن حالة «العمولات تجاوزت الإيراد» — القرار يحتاج اعتمادًا (جزء من Q3).
2. **`CalculateNetProfit` بقي متزامنًا نقيًا:** الجلب غير المتزامن للعمولة نُقل إلى `RecalculateNetProfitAsync` (`AccountingService.cs:62-68`)، فبقي عقد Domain متزامنًا واختُبر مباشرة.
3. **غياب `VisitTest` → `InvalidOperationException`:** اتساقًا مع أسلوب المعالجات الشقيقة (مثل `DeliverResultsCommandHandler` حسب مسار المراجعة السابق)؛ سؤال مفتوح Q5 عن التعاقد الأنسب.
4. **الحارس قبل التحقق (fail fast):** فحص العينة يسبق `ValidateResultAsync` (`EnterTestResultCommandHandler.cs:45` قبل `:52`) — لا عمل غير ضروري ولا كتابة عند الرفض.
5. **دَين تقني مسجّل:** `PricingService` (src/MasrLab.Application/Services/PricingService.cs:11-14) لا يزال يحقن `IPriceListResolverService` دون استخدامها في `CalculateSubtotal/CalculateTotal` (`:20-34`)؛ لم تُلمس في هذه المرحلة لارتباطها بتيارات الـQuotations المؤجلة — قيد مفتوح Q6.
6. **لا تغيير في تسجيل DI:** الاعتمادان الجديدان (`IReferralCommissionService` و`ISampleTrackingService`) كانا مسجَّلين أصلًا في `DependencyInjection.cs` — لم تُضف أي خدمة جديدة ولا أي تعديل على الحاوية.
7. **الحالة مقابل معايير قبول المرحلة 1 في V4 (سطر 234):** البحث عن مستهلكي الخدمات العشر في `Features` بعد هذه المرحلة:
   - `ISampleTrackingService` ✅ مستهلك جديد (`EnterTestResultCommandHandler.cs:16,24,40-43`).
   - `IReferralCommissionService` 🟡 مستهلك غير مباشر مبرَّر عبر `AccountingService.cs:16,21,25,67` (تستهلكه خدمة محاسبة تُستهلك بدورها من `RecordCashTransactionCommandHandler`).
   - `IPriceListResolverService` / `IPricingService` / `IReceiptCalculationService` ❌ لا يزالون بلا مستهلك في Features (بحث فعلي أعاد صفر نتائج) — القيد مفتوح Q6.

---

## 5. خطّ التدقيق (Audit traceability) — التغطية الكاملة

خطّ التدقيق المركزي `AuditBehavior` (src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs):

- `:28-48` — `Handle`: يقرأ اسم الطلب (`:30`) والمستخدم (`:31`) والتوقيت (`:32`)؛ على النجاح يسجل `success:true` (`:38`) وعلى الاستثناء `success:false` + `ErrorMessage` (`:44`) ثم يعيد الرمي (`:46`).
- `:50-72` — `PersistAuditEntryAsync`: يُنشئ `RequestAuditLog` بالحقول **الخمسة فقط** (`:56-63`): `RequestName`، `UserId`، `ActionTimeUtc`، `Success`، `ErrorMessage`، ويحفظها (`:65-66`).
- `:68-71` — أي فشل في حفظ التدقيق **يُبتلع** («must not abort the request») — سياسة قائمة بدون مراقبة/بديل موثق (مخاطرة C5 في V4 سطر 111، لا تزال مفتوحة).

| السلوك | الوسيلة | توثيق «من فعل ماذا» | توثيق «القيمة/السبب» |
|---|---|---|---|
| **A — تعديل السعر** | الأمر يمر عبر `AuditBehavior` | ✅ `RequestName + UserId + ActionTimeUtc + Success` | لا يُخزَّن السعر في التدقيق؛ يُستعاد من جدول البنود نفسه (قيمة ثابتة بالسجل) |
| **B — احتساب صافي النشاط** | يحدث عند كل قيد نقدي؛ القيد يمر عبر `AuditBehavior` | ✅ تسجيل القيد النقدي (مستخدم + توقيت + نجاح) | الصافي غير مخزَّن كإدخال؛ يُعاد اشتقاقه من `TotalIncome − TotalDiscount − CommissionsTotal` |
| **C — تجاوز الإدخال** | **تخزين صريح** في `TestResult.OverrideReason` + `EnteredByUserId` + `EnteredAt`، وسجل الطلب عبر `AuditBehavior` | ✅ على المستويين: سطر النتيجة (من/متى) وسجل الأمر (نجاح/فشل) | ✅ السبب محفوظ دائمًا بجانب النتيجة؛ الرفض بدون سبب = طلب فاشل في التدقيق |

**القرار الإجرائي:** بما أن `AuditBehavior` لا يسلسل حمولة الطلب (الحقول الخمسة فقط في `:56-63`)، فإن «سبب التجاوز» (المهمة C) لا يمكن استرجاعه من التدقيق؛ لذلك اعتُمد التخزين الصريح في عمود `OverrideReason` كالأثر الدائم، وأُلزم به عبر الشرط في المعالج (`EnterTestResultCommandHandler.cs:45-50`).

---

## 6. ملاحظات الدقة — الانحرافات بين الوصف المنصوص والمنفَّذ الفعلي

> هذه الملاحظات تعكس فحصًا مستقلاً للملفات الفعلية والـ `git diff`؛ وهي ليست عيوبًا بالضرورة، لكنها تستدعي قرارًا صريحًا:

| # | الوصف/المنصوص في تكليف المرحلة | ما هو موجود فعليًا على القرص | الأثر |
|---|---|---|---|
| D1 | **المهمة A:** حل مزدوج (قائمة النوع ← قائمة المعمل) مع استبعاد الاختبار عند الغياب في كليهما | **غير موجود:** `ResolvePriceAsync` محلّل لقائمة واحدة فقط؛ غياب البند → `BusinessRuleViolationException` (لا استبعاد صامت للاختبار) | إغلاق الخطر المالي تمّ عبر «الفشل الآمن» بدل «الاستبعاد»؛ هل السلوك المستهدف هو المزدوج؟ (Q1) |
| D2 | **المهمة B:** «الأصول والأقسام» (إضافة حساب طرف ثالث → قيد Debit لحساب الرصيد، وأصناف الأقسام: ثابتة / نسبة من صافي / نسبة من إجمالي) | **غير موجود إطلاقًا في الكود** (بحث `HasFields/FieldType/PercentageOf/Debit/Section` في src أعاد صفر نتائج) | نطاق B المنفَّذ هو بند «عمولات المحوِّلين» فقط من المصفوفة؛ بند الأقسام/الأصول بحاجة قرار تنفيذ/تأجيل (Q3) |
| D3 | **المهمة B:** «المصروف = نفقات الصندوق + نفقات الراتب + عمولات المحوِّلين» | `NetProfit = TotalIncome − TotalDiscount − CommissionsTotal`؛ المُكوّنان (نفقات الصندوق، نفقات الراتب) **غير مُدخلَيْن**، والمعادلة الأوسع السابقة استُبدلت | تغيير صيغة رسمي يحتاج اعتماد أعمال (Q3) |
| D4 | **المهمة B:** إضافة `Account.RecalculateNetProfit(commissionsTotal)` في كيان الحساب | **غير موجود:** `Account.cs` تغيّر تعليقه التوثيقي فقط (`:12-14`)؛ كل الحساب في `AccountingService` | لا حاجة لإعادة بنية؛ موثق كقرار في القسم 2 |
| D5 | **المهمة C:** عند التجاوز «تُدخَل النتيجة كحالة استثنائية (Abnormal)» | **غير منفذ:** الحالة دائمًا ناتج `ValidateResultAsync` (`:52-57,63`)؛ لا فرض لـ `Abnormal` عند التجاوز | قراءة النتائج الاستثنائية تعتمد على غير-فارغ `OverrideReason` لا على `Status`؛ قرار (Q4) |
| D6 | **المهمة C:** مدقق يشترط `Gender ∈ {male, female}` | **غير منفذ:** أُضيف فقط `OverrideReason.MaximumLength(500)` (`:27-28`) | لا قاعدة جنس إضافية؛ لا أثر وظيفي على المهمة (سؤال فرعي في Q4 إن رغبت) |
| D7 | **المهمة C:** سبب التجاوز يُحفظ فقط في المسار الاستثنائي | `OverrideReason` يُكتب في **كل** الإدخالات (`:64`) بقيمة `null` في المسار العادي | سلوك آمن؛ لا يلوث البيانات، فقط يُخزَّن null للعادي (موثق في D5/Q4) |

---

## 7. الأسئلة المفتوحة التي تحتاج قرار أعمال (Q1–Q6)

- **Q1 (تسعير):** سلوك «عدم وجود سعر» المستهدف: هل نُبقي «الفشل الآمن بالاستثناء» الحالي، أم ننفّذ **الحل المزدوج** (قائمة النوع ← قائمة المعمل ← استبعاد الاختبار من البيع) كما ورد في وصف المرحلة؟ (انظر D1)
- **Q2 (عمولة):** أساس العمولة = `Account.TotalIncome` (إجمالي الإيراد) كما نُفِّذ، أم `TotalIncome − TotalDiscount` (الإيراد بعد الخصم)؟ وأي الحسابات تُخصم عمولاتها: فقط تلك المقترنة بـ`DoctorId` أم تشمل حسابات Insurance/Drawer ذات `DoctorId`؟ (راجع `AccountingService.cs:59-60,64-67`)
- **Q3 (نطاق B):** هل ننفّذ بندَي «الأصول والأقسام» و«نفقات الصندوق/الراتب» الموصوفَين في المهمة B (غير موجودَين حاليًا)، أم نؤجلهما كمرحلة مستقلة؟ وهل نعتمد صيغة `NetProfit = TotalIncome − TotalDiscount − CommissionsTotal` كصيغة رسمية (مما يُسقط المكونات السابقة `OutsourcedCost/CashWithdrawals/CashDeposits`)؟ وهل يُسمح بصافي سالب أم نفرض حدًّا أدنى؟
- **Q4 (حارس العينة):** عند التجاوز: هل نفرض `Status = Abnormal` (حالة استثنائية ظاهرة) أم نكتفي بغير-فارغ `OverrideReason`؟ وهل نمنع إدخال `OverrideReason` في المسار العادي (عندما تكون العينة محلولة) أم نتركه اختياريًا كما هو الآن؟ وهل نضيف قاعدة `Gender ∈ {male, female}` في المدقق؟
- **Q5 (تعاقد الخطأ):** غياب `VisitTest` حاليًا → `InvalidOperationException`؛ هل نُبقيه أم نستبدله بـ`NotFoundException` / `BusinessRuleViolationException` لتوحيد عقد الأخطاء؟ (انظر القسم 4-3)
- **Q6 (قيد V4 المفتوح):** `IPriceListResolverService`/`IPricingService`/`IReceiptCalculationService` لا يزالون بلا مستهلك في `Features`، و`PricingService` يحمل حقنًا ميتًا للـ resolver (`PricingService.cs:11-14`): هل نربطهم بسيناريو CQRS (تدفقات الـQuotations/الإيصالات المؤجلة) أم نقيّد/نزيل العقود؟ (انظر القسم 4-7)

---

## 8. الأدلة التشغيلية

### 8.1 `dotnet build MasrLab.sln` — الناتج الفعلي الكامل

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  MasrLab.Domain -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Domain\bin\Debug\net8.0\MasrLab.Domain.dll
  MasrLab.Domain.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Domain.Tests\bin\Debug\net8.0\MasrLab.Domain.Tests.dll
  MasrLab.Application -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Application\bin\Debug\net8.0\MasrLab.Application.dll
  MasrLab.Infrastructure -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Infrastructure\bin\Debug\net8.0\MasrLab.Infrastructure.dll
  MasrLab.Application.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Application.Tests\bin\Debug\net8.0\MasrLab.Application.Tests.dll
  MasrLab -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Presentation\bin\Debug\net8.0-windows\MasrLab.dll
  MasrLab.Infrastructure.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Infrastructure.Tests\bin\Debug\net8.0\MasrLab.Infrastructure.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.40
```

> ملاحظة: `Directory.Build.props:3-5` يضبط `TreatWarningsAsErrors=true`؛ لذا «0 Warning» تعني صفر تحذير قائم لا صفر إنذار مفعلًا فحسب.

### 8.2 `dotnet test MasrLab.sln` — الناتج الفعلي الكامل

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  MasrLab.Domain -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Domain\bin\Debug\net8.0\MasrLab.Domain.dll
  MasrLab.Domain.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Domain.Tests\bin\Debug\net8.0\MasrLab.Domain.Tests.dll
Test run for C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Domain.Tests\bin\Debug\net8.0\MasrLab.Domain.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   167, Skipped:     0, Total:   167, Duration: 567 ms - MasrLab.Domain.Tests.dll (net8.0)
  MasrLab.Application -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Application\bin\Debug\net8.0\MasrLab.Application.dll
  MasrLab.Infrastructure -> C:\Users\LAP LINK\source\repos\MasrLab\src\MasrLab.Infrastructure\bin\Debug\net8.0\MasrLab.Infrastructure.dll
  MasrLab.Application.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Application.Tests\bin\Debug\net8.0\MasrLab.Application.Tests.dll
Test run for C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Application.Tests\bin\Debug\net8.0\MasrLab.Application.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    37, Skipped:     0, Total:    37, Duration: 2 s - MasrLab.Application.Tests.dll (net8.0)
  MasrLab.Infrastructure.Tests -> C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Infrastructure.Tests\bin\Debug\net8.0\MasrLab.Infrastructure.Tests.dll
Test run for C:\Users\LAP LINK\source\repos\MasrLab\tests\MasrLab.Infrastructure.Tests\bin\Debug\net8.0\MasrLab.Infrastructure.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - MasrLab.Infrastructure.Tests.dll (net8.0)
```

**الخلاصة:** 213/213 ناجحًا، 0 فاشل، 0 متخطَّى (Domain 167 + Application 37 + Infrastructure 9). اختبارات Application ارتفعت من 14 إلى 37 بإضافة 23 اختبارًا (4 + 3 + 7 + 5 + 4).

---

## 9. الملفات المعدَّلة والمضافة (شجرة العمل مقابل HEAD `12ea8cb`)

### معدَّلة (13)

1. `src/MasrLab.Application/Services/PriceListResolverService.cs`
2. `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListItems/UpdatePriceListItemsCommandValidator.cs`
3. `src/MasrLab.Application/Services/AccountingService.cs`
4. `src/MasrLab.Domain/Services/IAccountingService.cs`
5. `src/MasrLab.Domain/Entities/Financial/Account.cs`
6. `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommand.cs`
7. `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandValidator.cs`
8. `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandHandler.cs`
9. `src/MasrLab.Domain/Entities/Core/TestResult.cs`
10. `src/MasrLab.Domain/Interfaces/IVisitRepository.cs`
11. `src/MasrLab.Infrastructure/Persistence/Repositories/VisitRepository.cs`
12. `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs`
13. `src/MasrLab.Infrastructure/Persistence/Migrations/MasrLabDbContextModelSnapshot.cs`

### مضافة (7)

1. `src/MasrLab.Infrastructure/Persistence/Migrations/20260808223709_AddOverrideReasonToTestResult.cs`
2. `src/MasrLab.Infrastructure/Persistence/Migrations/20260808223709_AddOverrideReasonToTestResult.Designer.cs`
3. `tests/MasrLab.Application.Tests/PriceListResolverServiceTests.cs`
4. `tests/MasrLab.Application.Tests/UpdatePriceListItemsCommandValidatorTests.cs`
5. `tests/MasrLab.Application.Tests/AccountingServiceTests.cs`
6. `tests/MasrLab.Application.Tests/ReferralCommissionServiceTests.cs`
7. `tests/MasrLab.Application.Tests/EnterTestResultCommandHandlerTests.cs`

---

## 10. الروابط (Links)

- المرحلة المرجعية: `Docs/Application-Layer-Implementation-Roadmap-V4.md` (سطر 223-237؛ معايير القبول سطر 234)
- الخطّ المركزي: `src/MasrLab.Application/Common/Behaviors/AuditBehavior.cs`
- سجلات القرارات السابقة: `Docs/DecisionRecords/DD-08` … `DD-11`
- كل ملفات المصدر والاختبارات والميجريشن المذكورة في الأقسام 1-3 و9
