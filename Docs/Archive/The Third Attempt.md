اكتملت كل مراحل الفحص والتحقق. فيما يلي الرد الكامل المتصل:

تأكيد التحقق من البيئة

تم التحقق فعلياً قبل أي تحليل: المستودع github.com/El-ogra/MasrLab.git مُستنسخ، وHEAD الآن على الكوميت 4a2ab5c848a731fe1e8531e3b9df8263cf4805a6 برسالة "إصلاح تحذير القاعدة"، وهو ضمن الفرع origin/niamod — مطابقة تامة. لم يتم فتح أي ملف داخل مجلد Docs. الملف المرفق RL_Show.pdf (77 صفحة لقطات) تم تحويله لصور وتُصفّح كمرجع إلهامي فقط، دون نسخ أي نص أو شعار أو آلية ترخيص منه.

الجزء الأول — خطة إصلاح نافذتي "بيانات التحاليل" و"القيم المرجعية"

النافذتان الفعليتان هما Views/SystemSettings/TestsMasterDataWindow.xaml وViews/SystemSettings/ReferenceValuesWindow.xaml (وليسا الـ UserControl الفارغين في مجلد TestsMasterData — انظر البند 5-د). كل الأدلة التالية من الكود عند الكوميت المحدد.

1) عطل حرج: حفظ أي تحليل جديد يفشل دائماً

التأكيد من الكود:

- ViewModels/SystemSettings/TestsMasterDataViewModel.cs:231 (في SaveTestAsync) و:279 (في UpdateTestAsync) — يُمرَّر حرفياً string.Empty للمعاملين السابع (TurnaroundTime) والتاسع (Unit) من الأمر، مع false لـ LabToLabFlag.
- Features/TestsMasterData/Commands/AddTest/AddTestCommandValidator.cs:14-15 وUpdateTestCommandValidator.cs:15-16 — كلاهما يفرض RuleFor(x => x.TurnaroundTime).NotEmpty() وRuleFor(x => x.Unit).NotEmpty().
- النتيجة المؤكدة: أي حفظ/تعديل من النافذة يرمي ValidationException دائماً فيقع في catch ويظهر "خطأ في حفظ التحليل" (سطر 257).
- كيان Test (Domain/Entities/Core/Test.cs:14,16) يعرّف TurnaroundTime وUnit كحقول مطلوبة، والـ TestConfiguration.cs:21,23 يفرضهما IsRequired() — فالإصلاح الصحيح هو تمرير قيم حقيقية وليس إرخاء المدقق.

خطة الإصلاح عبر الطبقات:

- Presentation (الأساس):
  - إضافة خاصيتين في TestsMasterDataViewModel: Unit (سلسلة) وTurnaroundTime (سلسلة)، وتعبئتهما في OnSelectedTestChanged (بعد سطر 174) من value.Unit وvalue.TurnaroundTime (موجودتان في TestDto.cs:14,16)، وتصفيرهما في ClearForm (سطر 409).
  - في TestsMasterDataWindow.xaml: حقل "وحدة القياس" — الأفضل ComboBox قابل للتحرير عناصره من كيان وحدات القياس الموجود (نافذة "وحدات القياس" قائمة بالفعل، TestUnitsWindow)، بديلاً عن النص الحر — وحقل نصي "زمن التنفيذ" بجانب "Test time (Day)" في الشبكة (صف جديد في منطقة الحقول، السطر 179).
  - تمرير Unit وTurnaroundTime مكان string.Empty في موضعي الأمر (السطران 231 و279).
  - ملاحظة جانبية موثقة: النافذة لا تعرض حقلاً لـ Barcode الخام (يُمرَّر TestCode مكانه في الموضع الخامس للأمر، سطر 230) — يُترك كما هو أو يُضاف حقل "باركود" مستقل؛ يُحسم عند التنفيذ لأنه خارج نطاق العطل.
- Application: لا تغيير في الأمر/المدقق (القيد صحيح). يُضاف فقط اختبار.
- Domain / Infrastructure: لا تغيير (الحقول والأعمدة موجودة أصلاً).
- اختبارات حماية من الانحدار:
  - MasrLab.Application.Tests: اختبار أن AddTestCommand بقيم Unit/TurnaroundTime غير فارغة يجتاز المدقق، وأن الفارغة تفشل (تثبيت سلوك المدقق).
  - MasrLab.Presentation.Tests (المشروع موجود): اختبار ViewModel — ملء النموذج واستدعاء SaveTestCommand مع IMediator وهمي يتحقق أن الأمر المُرسل يحمل Unit وTurnaroundTime المُدخَلين وليس string.Empty (هذا الاختبار كان سيكشف العطل الحالي فوراً).

2) فجوة حرجة: دعم التحاليل متعددة المكونات

الوضع الحالي المؤكد: نتيجة واحدة لكل تحليل — TestResult مربوط بـ VisitTest (Domain/Entities/Core/TestResult.cs:10)، والقيم المرجعية مربوطة بـ TestId مباشرة (ReferenceValue.cs:8)، وResultValidationService يجلب القيم عبر GetByTestIdAsync(testId).

الخطة البنيوية الكاملة:

- Domain:
  - كيان جديد TestComponent : BaseEntity في Domain/Entities/Core/ بالحقول: Id، TestId (مفتاح أجنبي)، Name، Unit، DisplayOrder.
  - في Test: إضافة ICollection Components.
  - في ReferenceValue: إضافة int? TestComponentId قابل للعدم — null تعني "القيمة على مستوى التحليل ككل" (توافق كامل مع التحاليل أحادية المكون دون ترحيل بيانات إجباري).
  - في TestResult: إضافة int? TestComponentId (لكل نتيجة مكوّن).
- Application:
  - أوامر جديدة: AddTestComponent / UpdateTestComponent / DeleteTestComponent + استعلام GetTestComponentsByTestId، ومدققاتها (الاسم مطلوب، DisplayOrder ≥ 0، منع تكرار الاسم داخل نفس التحليل).
  - تعديل AddReferenceValueCommand وUpdateReferenceValueCommand (ومدققاتهما ومعالجاتهما) لقبول TestComponentId?، وتوسيع فحص التداخل ليكون ضمن نطاق (TestId + TestComponentId + Gender) لا على مستوى التحليل كله.
  - توسيع TestDto بقائمة المكونات، وReferenceValueDto بـ TestComponentId/اسم المكون.
  - تعديل ResultValidationService (حالياً يطابق بالجنس/العمر على قيم التحليل كاملة، السطور 30-82) ليختار القيمة المرجعية الخاصة بالمكوّن أولاً ثم يسقط على قيمة التحليل العامة عند غيابها — مع الحفاظ على سلوك التحاليل أحادية المكون دون تغيير.
  - تعديل EnterTestResultCommand لقبول إدخالات متعددة (مكوّن ← قيمة) عندما يكون للتحليل مكونات.
- Infrastructure:
  - TestComponentConfiguration: جدول TestComponents، Name مطلوب (200)، Unit (100)، فهرس مركّب فريد (TestId, Name)، علاقة HasOne(Test).WithMany(t => t.Components).HasForeignKey(c => c.TestId).OnDelete(Restrict).
  - تعديل ReferenceValueConfiguration وTestResultConfiguration: عمود TestComponentId القابل للعدم + علاقة اختيارية OnDelete(Restrict).
  - Migration جديد واحد: إنشاء TestComponents + إضافة العمودين + الفهارس. لا حاجة لترحيل بيانات (التحاليل الحالية تبقى بلا مكونات = أحادية).
- Presentation:
  - داخل TestsMasterDataWindow: قسم "مكونات التحليل" (شبكة صغيرة: الاسم/الوحدة/الترتيب + أزرار إضافة/تعديل/حذف) يُفعَّل عند اختيار تحليل، بنفس أسلوب النافذة الحالي.
  - في ReferenceValuesWindow: عمود "المكوّن" في الشبكة + ComboBox اختيار المكوّن في النموذج (يُملأ من GetTestComponentsByTestId؛ خيار "التحليل كاملاً" = null).
  - شاشة إدخال النتائج (EnterResultsView): عند تحليل متعدد المكونات تُعرض صفوف المكونات بدل حقل القيمة الواحد.
- الاختبارات: وحدة Domain (العلاقات وقواعد التحقق)، تطبيق (مدقق المكونات، تداخل النطاقات ضمن مكوّن، اختيار القيمة في ResultValidationService للمكوّن ثم السقوط للعامة)، وتكامل Infrastructure على قاعدة فعلية (العلاقة + الفهرس الفريد + فلتر الحذف المنطقي العام).

3) حذف فعلي بدل حذف منطقي

التأكيد: DeleteTestCommandHandler.cs:26 يستدعي _testRepository.Delete(test)، والمستودع العام GenericRepository.cs:37-40 ينفذ Remove() — حذف فيزيائي خالص. بينما BaseEntity يحقق ISoftDeletable (BaseEntity.cs:5,12)، وMasrLabDbContext.cs:79-88 يطبق HasQueryFilter عاماً على كل كيان قابل للحذف المنطقي، وTestConfiguration.cs:50 فيه فهرس على IsDeleted — أي البنية التحتية للحذف المنطقي جاهزة والمعالج لا يستخدمها. كذلك لا يوجد أي فحص قيود مرجعية رغم أن VisitTest.cs:10 وTestGroupItem.cs:8 وPriceListItem.cs:8 وReferenceValue.cs:8 كلها تحمل TestId.

خطة الإصلاح:

- Application — DeleteTestCommandHandler: قبل الحذف:
  1. فحص القيود عبر المستودعات: IRepository (أي زيارة تستخدم التحليل؟)، TestGroupItem (عضو في مجموعة؟)، PriceListItem (مسعّر في قائمة أسعار؟)، وReferenceValueRepository.GetByTestIdAsync (له قيم مرجعية نشطة؟). عند وجود أي مرجع → رمي BusinessRuleViolationException برسالة عربية توضح مانع الحذف (مثلاً: "لا يمكن حذف التحليل لارتباطه بزيارات مسجلة — يمكن إيقافه بدلاً من حذفه").
  2. عند خلوه من القيود: حذف منطقي — test.IsDeleted = true; ثم _testRepository.Update(test); ثم SaveChangesAsync. الفلتر العام في الـ DbContext سيخفيه تلقائياً من كل الاستعلامات.
- نفس المعالجة لـ DeleteReferenceValueCommandHandler.cs:28 (نفس العطل: حذف فيزيائي).
- Presentation: زر "حذف" في النافذة يعرض تأكيد MessageBox قبل الإرسال (لا يوجد أي استخدام لـ MessageBox حالياً في الـ ViewModels — يُضاف هنا كنمط أول)، ورسائل الأعمال العربية تظهر في ErrorMessage كما هو معمول.
- الاختبارات: معالج الحذف (يقيّد عند وجود مراجع / يضبط IsDeleted عند الخلو / يرمي EntityNotFoundException عند غياب الكيان)، وتكامل يثبت أن المحذوف منطقياً لا يظهر في GetTestsList بفضل فلتر الاستعلام العام.

4) خلل كشف تداخل النطاقات العمرية

التأكيد: في AddReferenceValueCommandHandler.cs:47-48 وUpdateReferenceValueCommandHandler.cs:52-53: if (existing.AgeUnit != request.AgeUnit) continue; — أي نطاقين بوحدتين مختلفتين (مثلاً 0–6 شهور مقابل 0–1 سنة) لا يُفحص تداخلهما إطلاقاً رغم تطابقهما فعلياً. وAgeUnit فيه Years/Months/Days (AgeUnit.cs).

خطة الإصلاح:

- Domain (الأفضل): إضافة دالة تحويل نقية في كيان ReferenceValue (أو خدمة نطاق صغيرة AgeRangeMath): تحويل (AgeMin, AgeMax, AgeUnit) إلى أيام: Days=1، Months=30، Years=365، مع توثيق التقريب في تعليق. معاملة (0,0) كـ "غير مقيد" تبقى كما هي قبل التحويل.
- Application: في المعالجين، استبدال شرط continue بمقارنة بعد التوحيد: حساب (minDays, maxDays) للطرفين ثم فحص existing.MinDays ) وViewModels/TestsMasterData/TestsMasterDataViewModel.cs (صنف فارغ، 7 أسطر) يتيم تماماً: لا يُنشأ ولا يُستضاف في أي مكان — النافذة الحقيقية المستخدمة هي TestsMasterDataWindow (تأكيد: MainViewModel.cs:74 يفتح TestsMasterDataWindow، وDependencyInjection.cs:65 يسجّل الـ ViewModel الفارغ بلا أي مستهلك، بينما سطر 74 يسجّل الحقيقي). التوصية: حذف الثلاثي + إزالة سطر التسجيل 65 — أو توثيقه كعنصر نائب مقصود إن كان مُخططاً له. أي مرجع آخر لنفس الاسم (WorkSheetsView في DI) سجّله طبيعي لأنه UserControl مستضاف فعلاً.
- هـ) تسجيل الدخول يكشف أسماء المستخدمين قبل المصادقة: LoginViewModel.cs:21 يستدعي LoadUsernamesAsync() في المنشئ قبل أي دخول، ويملأ Usernames (سطر 37) المربوطة بـ ComboBox في LoginWindow.xaml:30-33 عبر الاستعلام GetRegisteredUsernamesQuery. الإصلاح: استبدال الـ ComboBox بـ TextBox إدخال حر لاسم المستخدم، وحذف التحميل المسبق من المنشئ. الاستعلام GetRegisteredUsernames نفسه إما يُقيَّد بصلاحية مدير أو يُحذف إن لم يعد له مستهلك آمن (مكانه الطبيعي: شاشة إدارة المستخدمين بعد المصادقة، وهي موجودة أصلاً في UsersPermissionsView).

الجزء الثاني — اختيار وتخطيط 10 واجهات جديدة

الخطوة الأولى — نتيجة الفحص الخفيف بالعناوين فقط

ما هو موجود في الكود (من MainWindow.xaml وأسماء مجلدات Views فقط):

- الشريط العلوي: زران فقط — "المرضى" (MainWindow.xaml:22) و"إعدادات النظام" (:36).
- تحت "المرضى" (:55-66): إدخال وإضافة مريض جديد • إدخال نتائج التحاليل • البحث عن مريض • تسليم نتائج المرضى.
- تحت "إعدادات النظام" (:72-98): بيانات التحاليل • Barcode Types • Culture Antibiotics • الجهات الخارجية والمعامل • مجموعات التحاليل • وحدات القياس • Test Comments • ألقاب وتعريفات المرضى • طباعة قائمة أسعار التحاليل.
- مجلدات Views الموجودة: PriceLists، PatientSearch، Accounting، TestsMasterData، TestGroups، OutsourcedSamples، AttendanceAndAudit، SampleCollection، PatientHistory، FixedComments، SystemSettings، WorkSheets، PatientManagement، CasesFollowUp، Statistics، UsersAndPermissions، ResultsEntry، Cultures + LoginWindow/FirstRunSetupWindow/MainView.

عناوين نوافذ الملف المرفق (77 لقطة، تصفُّح عناوين فقط):

| الصفحات | العنوان/الوظيفة |
|---|---|
| 1–5 | تعريف بالنظام، شاشة دخول، إيصال استلام عينات |
| 6–11 | كشف المريض، أوراق عمل (بالمريض/بالتحليل)، سجل الهرمونات، سجل عدد التحاليل |
| 12–24 | تقارير نتائج بقوالب مختلفة (بول، مزرعة، CBC، كيمياء، تخثر، هرمونات، مصليات، سائل منوي) |
| 25–29 | قائمة المرضى/النتائج، كشف حساب عام، كشف حساب مريض، تفاصيل وملخص حساب |
| 30–33 | رسوم إحصائية (مرضى سنوياً/يومياً، توزيع تحاليل) |
| 34–41 | دخول، القائمة الرئيسية، تسجيل مريض، إدخال نتائج بعدة أقسام |
| 42–45 | إعداد قاعدة البيانات، قائمة تفاصيل التحاليل، إعداد المدى المرجعي، إعداد المضادات |
| 46–51 | مجموعات تحاليل خاصة، إحصائيات رئيسية، قائمة حسابات (حسابات/دفع سريع/مصروفات/تسويات)، حساب ومطالبات، قائمة أدوات (إعدادات قاعدة بيانات/صيانة/دليل/تغيير كلمة المرور) |
| 52–54، 74–77 | كتالوج مزايا، بوابة نتائج إلكترونية، مزايا ربط الأجهزة، سياسات ترخيص (مستبعدة كلها — توثيقية/تسويقية) |
| 55، 67 | واجهة ربط أجهزة التحليل (Interface) |
| 56–65 | قوالب تقارير CBC متنوعة (مستبعدة — تنويعات عرض لا وظيفة) |
| 66 | تاريخ نتائج المريض ورسوم الاتجاه (Trend) |
| 68 | شبكة إدارة نتائج وبيانات المرضى |
| 69–70 | مدى مرجعي + تعليقات التحاليل (موجودان) |
| 71 | عوامل التصحيح (Correction Factors) |
| 72 | مصمم تخطيط التقارير |
| 73 | بحث موسّع في المرضى والنتائج |

الاستبعاد: كل ما هو تحت "المرضى"/"إعدادات النظام" أو له مجلد موجود (تسجيل مريض، إدخال نتائج، بحث مريض، تسليم، أوراق عمل، إحصائيات، حسابات الأدراج، تاريخ مريض، متابعة حالات، مزارع، قوائم أسعار، تعليقات، مجموعات، عينات خارجية، حضور/تدقيق، مستخدمين، مدى مرجعي، مضادات) + الصفحات التوثيقية/التسويقية/الترخيصية + تنويعات قوالب التقارير (56–65) لأنها عرض لا وظيفة.

القائمة المرشحة بعد الاستبعاد (مرتبة تقديرياً بالاعتمادية)، والعشرة المختارة:

1. المصروفات والدفع السريع (ص 49–50) — الأساس النقدي اليومي، أقل اعتمادية.
2. الأدوات: النسخ الاحتياطي وصيانة قاعدة البيانات (ص 51) — يقف على ManageBackup الجاهز.
3. تغيير كلمة المرور (ص 51) — يقف على UpdateUser الجاهز.
4. عوامل التصحيح (ص 71) — بيانات مرجعية صغيرة، يخدم إدخال النتائج لاحقاً.
5. كشف الحساب والمطالبات (ص 50) — قراءة تجميعية فوق الحسابات الموجودة.
6. شبكة إدارة نتائج وبيانات المرضى (ص 68) — قراءة/تصحيح فوق الزيارات والنتائج الموجودة.
7. بحث موسّع في المرضى والنتائج (ص 73) — يمتد فوق (6) وفوق بحث المرضى الحالي.
8. تاريخ نتائج المريض مع رسوم الاتجاه (ص 66) — يمتد فوق PatientHistory.
9. مصمم تخطيط التقارير (ص 72) — يفعّل كيان ReportTemplate اليتيم.
10. ربط أجهزة التحليل (Analyzer Interface) (ص 55/67) — الأعلى اعتمادية والأكبر نطاقاً، يُختم به.

الخطوة الثانية — التحليل العميق وخطط التنفيذ للعشرة

الواجهة 1: المصروفات والدفع السريع (مرجع: ص 49–50)

- الوصف وسبب الترتيب: تسجيل مصروفات المعمل وحركات نقدية سريعة (سحب/إيداع) على الحساب/الدرج. أولاً لأنها لا تعتمد على أي واجهة جديدة أخرى وتبني عليها واجهة المطالبات (5).
- لقطات المرجع: قائمة الحسابات فيها "Quick Pay / Expenses / Calculations" وشاشة حساب فيها مبلغ وطرف الحركة وبيان وتاريخ.
- الموجود جزئياً (أدلة): كيان CashTransaction بحقول Type/Amount/AccountId/UserId/TransactionDate (Domain/Entities/Financial/CashTransaction.cs:8-26) ومعه TransactionType { Withdrawal, Deposit } (Common/Enums/TransactionType.cs)، وأمر RecordCashTransactionCommand(Type, Amount, EntityId, UserId, TransactionDate) + معالج + مدقق (Features/Accounting/Commands/RecordCashTransaction/)، وكيان Account (Financial/Account.cs:7-19). الناقص: لا يوجد مفهوم "مصروف" مخصص — التوصية كيان Expense : BaseEntity جديد (المبلغ، البند، البيان، التاريخ، المستخدم، الحساب) بدل تحميل CashTransaction ما لا يحتمله، لأن المصروف قرار إداري لا مجرد حركة درج.
- Application: RecordExpenseCommand + GetExpensesQuery(فترة) + مدقق (المبلغ > 0، البند مطلوب) — وإعادة استخدام RecordCashTransactionCommand للدفع السريع كما هو.
- Infrastructure: ExpenseConfiguration + Migration لجدول Expenses. لا جديد للدفع السريع.
- Presentation: ExpensesWindow عربية RTL بنفس نمط TestsMasterDataWindow (شبكة + نموذج سفلي + أزرار ملونة)، تُفتح من زر جديد في قائمة "المرضى" لا الإعدادات (عملية تشغيلية يومية)، عبر OpenWindow نفسها (MainViewModel.cs:125) التي تُخفي النافذة الرئيسية ثم ShowDialog.
- الاختبارات: مدقق المصروف، المعالج (رصيد الحساب يتأثر/لا يتأثر حسب التصميم)، وعدم قبول مبلغ سالب.
- الاعتماديات: لا شيء جديد.

الواجهة 2: النسخ الاحتياطي وصيانة قاعدة البيانات (مرجع: ص 51)

- الوصف وسبب الترتيب: شاشة "أدوات" لأخذ نسخة احتياطية/استعادة وفحص قاعدة البيانات. ثانياً لأن أمرها جاهز ولا تعتمد على شيء.
- الموجود جزئياً: ManageBackupCommand(BackupOperation Operation, string FilePath, string? RestoreConfirmation) + معالج + مدقق (Features/SystemSettings/Commands/ManageBackup/) — خدمة كاملة بلا واجهة.
- Application: إعادة استخدام الأمر كما هو + استعلام خفيف GetBackupHistoryQuery (اختياري، من سجل التدقيق AuditLog الموجود).
- Infrastructure: لا تغيير (النسخ على مستوى ملف/خدمة في المعالج الموجود).
- Presentation: BackupMaintenanceWindow بأزرار: اختيار مسار، نسخ الآن، استعادة (مع تأكيد نصي إلزامي عبر RestoreConfirmation) — تُفتح من "إعدادات النظام" لأنها إدارية، بنفس آلية OpenWindow.
- الاختبارات: المعالج يرفض الاستعادة دون تأكيد مطابق؛ نجاح النسخ ينشئ ملفاً.
- الاعتماديات: لا شيء.

الواجهة 3: تغيير كلمة المرور (مرجع: ص 51)

- الوصف وسبب الترتيب: شاشة صغيرة لتغيير المستخدم الحالي كلمة مروره. ثالثاً لأنها جزيرة مستقلة.
- الموجود جزئياً: UpdateUserCommand(Id, Username, Password, IsAdmin, IsActive) + معالج + مدقق (Features/UsersAndPermissions/Commands/UpdateUser/)، وخدمة مصادقة IAuthenticationService (LoginViewModel.cs:14,66).
- الفجوة: UpdateUser يستلزم معرفة IsAdmin/IsActive ولا يتحقق من كلمة المرور القديمة.
- Application: أمر مخصص ChangeOwnPasswordCommand(UserId, OldPassword, NewPassword) — يتحقق من القديمة عبر IAuthenticationService ثم يحدّث. مدقق: الجديدة ≥ 8 أحرف وتطابق التأكيد (التأكيد في الـ ViewModel).
- Infrastructure: لا شيء (جدول User موجود).
- Presentation: ChangePasswordWindow صغيرة (قديمة/جديدة/تأكيد + زر حفظ)، تُفتح من الشريط العلوي أو الإعدادات، بنفس آلية ShowDialog.
- الاختبارات: رفض كلمة قديمة خاطئة، رفض جديدة ضعيفة، نجاح التغيير يجعل الدخول بالقديمة يفشل.
- الاعتماديات: لا شيء.

الواجهة 4: عوامل التصحيح (مرجع: ص 71)

- الوصف وسبب الترتيب: تعريف عوامل حسابية تُطبَّق على النتائج الخام (معامل ضرب/إزاحة لكل تحليل/جهاز). رابعاً لأنها بيانات مرجعية مستقلة لكنها تسبق أي اعتماد لاحق عليها في إدخال النتائج.
- الموجود جزئياً: لا شيء — بحث "Correction" في src/ بلا نتيجة. أقرب ما يوجد ResultValidationService الذي يفسر القيم ولا يصححها.
- Application: كيان CorrectionFactor : BaseEntity (TestId، نوع العامل، قيمة الضرب، قيمة الإزاحة، فعّال)، أوامر Add/Update/DeleteCorrectionFactor + استعلام GetCorrectionFactorsByTestId + مدقق (تحليل موجود، قيمة غير صفرية للضرب).
- Infrastructure: CorrectionFactorConfiguration (فهرس فريد على TestId + فعّال) + Migration.
- Presentation: CorrectionFactorsWindow (شبكة: التحليل/النوع/الضرب/الإزاحة + نموذج)، تُفتح من "إعدادات النظام" — هي بيانات مرجعية.
- الاختبارات: مدقق، تطبيق المعامل رياضياً (اختبار وحدة نقي)، تفرد عامل فعّال واحد لكل تحليل.
- الاعتماديات: الجزء الأول (2) — إن طُبّق العامل على مستوى المكوّن يُضاف TestComponentId? لاحقاً؛ وإلا يبقى على التحليل.

الواجهة 5: كشف الحساب والمطالبات (مرجع: ص 50)

- الوصف وسبب الترتيب: شاشة تجميعية تعرض كشف حساب لفترة (إيرادات، خصومات، عمولات أطباء، صافي) مع قائمة مطالبات. خامساً لأنها قراءة فوق (1) وفوق كيانات الحساب الموجودة.
- الموجود جزئياً: Account (TotalIncome/TotalDiscount/NetActivityAfterCommission/AccountType/Period — Financial/Account.cs:7-19)، واستعلامات جاهزة: GetDrawerReport وGetDoctorReferralReport (Features/Accounting/Queries/)، وأوامر إنشاء أدراج (CreatePeriodDrawer وغيرها).
- الفجوة: لا يوجد استعلام "كشف حساب موحّد لفترة" يجمع الإيصالات + المصروفات (1) + العمولات.
- Application: GetAccountStatementQuery(AccountId?, From, To) يعيد بنوداً مرتبة زمنياً (إيصال/مصروف/حركة/عمولة) + إجماليات. مدقق تاريخ من ≤ إلى.
- Infrastructure: لا كيانات جديدة — تجميع عبر MasrLabDbContext مباشرة.
- Presentation: AccountStatementWindow (محدد فترة + شبكة بنود + صف إجماليات + زر طباعة يستخدم مسار Printing الموجود)، تُفتح من "المرضى" أو قائمة حسابات لاحقة.
- الاختبارات: حساب الإجماليات على بيانات معروفة، ترتيب البنود، فلتر الفترة.
- الاعتماديات: الواجهة 1 (لتظهر المصروفات في الكشف).

الواجهة 6: شبكة إدارة نتائج وبيانات المرضى (مرجع: ص 68)

- الوصف وسبب الترتيب: شبكة إدارية واسعة لاستعراض نتائج المرضى وتصحيح نتيجة/بيانات نهائياً مع تدقيق. سادساً لأنها قراءة/كتابة فوق VisitTest/TestResult الموجودين وتسبق البحث الموسع (7).
- الموجود جزئياً: TestResult مع حدث تعديل TestResultEdited (Core/TestResult.cs:29-54)، وGetTestResultForVisit + EnterTestResult (Features/ResultsEntry/)، وكيانات Patient/PatientVisit، وGetPatientHistory (Features/PatientHistory/).
- الفجوة: لا استعلام شبكة مسطّحة (مريض/زيارة/تحليل/نتيجة/حالة) ولا أمر "تصحيح نتيجة" مستقل عن شاشة الإدخال.
- Application: GetResultsGridQuery(فلاتر) + CorrectTestResultCommand(VisitTestId, NewValue, Reason) — السبب إلزامي ويُسجَّل في AuditLog. مدقق: قيمة غير فارغة، سبب غير فارغ.
- Infrastructure: لا جديد (يستخدم RequestAuditLog/AuditLog الموجودين).
- Presentation: ResultsGridWindow (فلاتر علوية + شبكة واسعة + زر تصحيح يفتح مربع قيمة+سبب)، صلاحية عبر CheckPermission الموجود. تُفتح من "المرضى".
- الاختبارات: التصحيح يكتب تدقيقاً بالقيمتين والسبب؛ رفض التصحيح بلا سبب؛ فلاتر الشبكة.
- الاعتماديات: لا شيء جديد (تستفيد من 2 إن وُجدت المكونات).

الواجهة 7: البحث الموسّع في المرضى والنتائج (مرجع: ص 73)

- الوصف وسبب الترتيب: محرك بحث متعدد المعايير (اسم/هاتف/فترة/تحليل/علم نتيجة High/Low/حالة) يعيد مرضى ونتائج. سابعاً لأنه امتداد مباشر لـ (6) ولبحث المرضى الحالي.
- الموجود جزئياً: SearchPatients + GetPatientVisitHistory (Features/PatientSearch/)، وCalculateHighLowStatus (Features/ResultsEntry/Queries/).
- الفجوة: البحث الحالي بالمريض فقط، لا بالنتيجة/العلم/التحليل.
- Application: AdvancedSearchQuery بمعايير اختيارية مركبة يعيد صفوف (مريض، زيارة، تحليل، نتيجة، علم). لا مدقق ثقيل (كل المعايير اختيارية، لكن يُشترط معيار واحد على الأقل).
- Infrastructure: استعلام تجميعي على DbContext — الانتباه لفلترة SQL لا الذاكرة (درس البند 5-ج في الجزء الأول).
- Presentation: AdvancedSearchWindow (نموذج معايير + شبكة نتائج + فتح سجل المريض من الصف)، تُفتح من "المرضى".
- الاختبارات: كل معيار منفرداً وتركيبات، شرط وجود معيار واحد على الأقل.
- الاعتماديات: الواجهة 6.

الواجهة 8: تاريخ نتائج المريض مع رسوم الاتجاه (مرجع: ص 66)

- الوصف وسبب الترتيب: عرض زمني لنتائج مريض محدد لتحليل محدد مع رسم اتجاه (Trend). ثامناً لأنه طبقة عرض فوق تاريخ المريض الموجود.
- الموجود جزئياً: GetPatientHistoryQuery (Features/PatientHistory/Queries/GetPatientHistory/) + PatientHistoryEntry DTO، ونافذة PatientHistoryView موجودة بالفعل — فهذه ليست بديلاً عنها بل واجهة اتجاه متخصصة (تحليل واحد عبر الزمن + رسم) وهو ما تظهره لقطة الاتجاهات.
- Application: GetTestTrendQuery(PatientId, TestId) يعيد نقاط (تاريخ، قيمة، علم) مرتبة. لا كيانات.
- Infrastructure: لا شيء.
- Presentation: PatientTrendWindow (اختيار تحليل + رسم خطي — مكتبة رسم WPF خفيفة أو رسم يدوي على Canvas التزاماً بعدم إضافة تبعيات ثقيلة) + جدول النقاط. تُفتح من سجل المريض/البحث الموسع (7).
- الاختبارات: ترتيب النقاط زمنياً، استبعاد القيم غير الرقمية من الرسم، التعامل مع نقطة واحدة.
- الاعتماديات: الواجهة 7 (منفذ الفتح الطبيعي).

الواجهة 9: مصمم تخطيط التقارير (مرجع: ص 72)

- الوصف وسبب الترتيب: شاشة ضبط قوالب التقارير (هوامش، مقاس ورق، ترويسة/تذييل نص وصورة، ألوان). تاسعاً لأنه يفعّل كياناً يتيماً ويخدم الطباعة الموجودة.
- الموجود جزئياً (مهم): كيان ReportTemplate : BaseEntity كامل الحقول (Margins, PaperSize, HeaderImage, HeaderText, FooterText, HeaderColor, FooterColor — Domain/Entities/Settings/ReportTemplate.cs:6-15) ومكوَّن في MasrLabDbContext (DbSet)، لكن لا يوجد له أي استخدام في طبقة Application إطلاقاً (البحث عن "ReportTemplate" في Application بلا نتيجة) — كيان يتيم بلا أوامر/استعلامات/واجهة. كما توجد UpdateReportSettingsCommand في SystemSettings (إعدادات عامة لا قوالب لكل تقرير).
- Application: GetReportTemplatesQuery + SaveReportTemplateCommand (إنشاء/تحديث قالب) + مدقق (مقاس ورق صالح، هوامش بصيغة صحيحة).
- Infrastructure: لا جديد (الجدول موجود) — ربما Migration إن اختلفت الأعمدة عن الحاجة.
- Presentation: ReportTemplateDesignerWindow (معاينة تخطيط + حقول الهوامش/الترويسة/التذييل/الألوان)، من "إعدادات النظام".
- الاختبارات: مدقق الهوامش/المقاس، حفظ واسترجاع القالب.
- الاعتماديات: لا شيء حرج؛ التكامل مع مسار الطباعة (Printing) مرحلة لاحقة.

الواجهة 10: ربط أجهزة التحليل — Analyzer Interface (مرجع: ص 55، 67، 74، 77)

- الوصف وسبب الترتيب: واجهة ربط أجهزة التحليل المخبرية لاستقبال النتائج آلياً (لقطات "RealLab Interface CBC/VIDAS" وقائمة الأجهزة المتوافقة). عاشراً وأخيراً لأنه الأكبر نطاقاً والأعلى اعتمادية — يلمس إدخال النتائج وعوامل التصحيح (4) والمكونات (الجزء الأول-2)، ولا يُبنى قبل استقرارها.
- الموجود جزئياً: لا شيء إطلاقاً — بحث "Analyzer/Device/Instrument" في Domain وApplication بلا نتيجة. الوحيد القريب هو EnterTestResultCommand اليدوي.
- Application: كيان AnalyzerDevice : BaseEntity (الاسم، الموديل، نوع الاتصال Serial/TCP، معالم الاتصال، تعيين رموز الجهاز↔التحاليل)، أوامر إدارة الأجهزة + أمر ImportAnalyzerResultCommand(DeviceId, RawResult) يحوّل النتيجة الخام إلى TestResult عبر تعيين الرموز. خدمة بروتوكول (ASTM/HL7) خلف واجهة IAnalyzerChannel لتبقى القراءة قابلة للاختبار بقناة وهمية.
- Infrastructure: جداول AnalyzerDevices + AnalyzerTestMap + Migration، ومستودع قراءة فعلي (سيريال/شبكة) معزول خلف الواجهة حتى لا تختبره اختبارات الوحدة.
- Presentation: AnalyzerInterfaceWindow (قائمة أجهزة + حالة اتصال + سجل نتائج واردة + زر قبول/رفض لكل نتيجة)، من "المرضى" أو قائمة مستقلة لاحقاً.
- الاختبارات: تحويل نتيجة خام → TestResult عبر قناة وهمية (لا عتاد حقيقي)، تعيين رموز صحيح/خاطئ، رفض نتيجة لتحليل غير معيّن.
- الاعتماديات: الواجهة 4 (تطبيق عوامل التصحيح على الوارد) والجزء الأول-2 (توجيه النتيجة للمكوّن الصحيح عند تعدد المكونات).

اكتملت المهمة بالكامل: خطة إصلاح الجزء الأول (النقاط 1–5 بكل بنودها)، ثم القائمة الأولية المستبعدة والمختارة، ثم خطط تنفيذ الواجهات العشر كاملة بالتتابع — كل نقطة مدعومة بأدلة من الكود عند الكوميت 4a2ab5c (مسار الملف ورقم السطر)، وكل الواجهات العشر غير مكررة لما تحت "المرضى" أو "إعدادات النظام"، وتتبع اصطلاحات المشروع الحالية (عربي RTL، نوافذ WPF تُفتح بإخفاء الرئيسية ثم ShowDialog).