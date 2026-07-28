ممتاز، لديّ الآن كل ما أحتاجه من المصادر الثلاثة (المواصفات، PDF، ومستودع GitHub عند Commit 9b6fda5). سأقدم الآن المرحلة الأولى: الأقسام 1-5 بالتنسيق المطلوب.

📘 استخراج معرفة Domain Layer لنظام MasrLab — المرحلة الأولى (الأقسام 1-5)

المصادر المستخدمة:
- MasrLab Spec (MasrLab_Specifications_and_Audit.md) — المرجع المعياري (1245 سطراً)
- RealLab PDF (RLS_Learn1 + RLS_Learn2) — المرجع الوظيفي
- GitHub Repo (فرع niamod @ Commit 9b6fda5) — التحقق من الكود الفعلي (62 ملف Domain)
بنية Domain Layer الفعلية في الكود: 30 كياناً + 14 Enum + Interface واحد للـ Base (BaseEntity + IAuditableEntity + ISoftDeletable) + 4 استثناءات (Exceptions) + 8 واجهات Repository.

🟦 القسم 1 — سد الفجوات في الكيانات الموجودة

1.1 PatientHistory — كيان مشتق (Derived Entity / SQL View)

المصدر: استنتاج من أكثر من مصدر (MasrLab Spec + RealLab PDF + GitHub Repo)

التفاصيل:

| البُعد | النتيجة |
|---|---|
| في المواصفات (Entity 3) | كيان مُشتق (SQL View) بـ 16 عموداً — ليس جدولاً مخزَّناً |
| في RealLab PDF (صفحات 70-78) | يظهر تلقائياً في التقرير إذا أجرى المريض نفس التحليل سابقاً؛ يمكن تعطيل الإدراج التلقائي من شاشة "إعدادات التقارير"؛ ويوجد زر Patient History لعرض جميع الزيارات السابقة يدوياً |
| التعريف بالمريض | Lab ID أو الاسم (PDF: "يقوم البرنامج بالتعرف على التاريخ المرضي عن طريق كود المعمل أو عن طريق الاسم") — لكن Lab ID هو المفضّل لأنه فريد |
| نطاق العرض | جميع الزيارات السابقة لنفس التحليل مرتّبة بـ VisitDate تصاعدياً (وليس آخر زيارة فقط) |
| في GitHub Repo (فرع niamod @ 9b6fda5) | ❌ غير موجود إطلاقاً في Domain Layer — لا Entity ولا Interface ولا Service. لا يظهر في مجلد Entities/ أو Interfaces/ |
| الفجوة | يجب استحداث كل من: (أ) SQL View في Infrastructure، (ب) DTO / Read Model في Domain (مثلاً PatientHistoryEntry كـ record) للاستعلام عنه — دون أعمدة تدقيق لأنه View |

أهميته لـ Domain: حرج — يجب تنفيذه (وحدة Module 6 كاملة تعتمد عليه، والإدراج التلقائي في التقارير مذكور صراحة في PDF).

ملاحظات إضافية:
- المقترح: إنشاء PatientHistoryEntry كـ immutable record (Value-like Read Model) في Domain.ReadModels أو Domain.Common.DTOs بجانب StatisticsDto الموجود فعلاً.
- الحقول الـ 16 المذكورة في المواصفات تكفي، بشرط ألا يرث من BaseEntity (لأنه ليس Entity قابلاً للتعديل).
- الإعداد الخاص بتعطيل الإدراج التلقائي يجب أن يُخزَّن كـ SystemSetting بمفتاح مثل AutoInsertPatientHistory = "true"/"false".

1.2 Doctor.DiscountPercent (ناقص في الكود)

المصدر: GitHub Repo + MasrLab Spec + RealLab PDF

التفاصيل:

| البُعد | النتيجة |
|---|---|
| في المواصفات (Entity 23) | الحقل مذكور كـ: DiscountPercent / CommissionPercent (نسبة الخصم/العمولة) — يوحي بأنهما مفهوم واحد أو أحدهما |
| في RealLab PDF (القسم 3-7) | لا يذكر "DiscountPercent" أو "CommissionPercent" صراحةً كأسماء تقنية؛ يشير فقط إلى ربط الجهات بـ قوائم أسعار خاصة (تعاقدات/إحالة) وإلى "عمل درج لطبيب معالج معين" (القسم 6-3) — أي أن الطبيب يستفيد مالياً من مرضاه |
| في GitHub Repo (Entities/Administrative/Doctor.cs) | يوجد فقط: public decimal CommissionPercent { get; set; } — لا يوجد DiscountPercent |
| في Receipt | حقل Discount (decimal) موجود على مستوى الإيصال — أي أن خصم المريض يُطبَّق على الإيصال مباشرة، وليس مشتقاً من الطبيب |
| الاستنتاج | الكود اختار التمييز الصحيح: الطبيب له CommissionPercent فقط (عمولة تُدفع للطبيب من دخل المعمل)، والخصم يُدخَل يدوياً على الإيصال (Receipt.Discount). الاسم المزدوج في المواصفات كان مضللاً |

أهميته لـ Domain: مهم — يستحق التوضيح في المواصفات.

ملاحظات إضافية:
- قرار مُقترَح: ثبّت Doctor.CommissionPercent فقط، واحذف DiscountPercent من نص Entity 23 (تعارض بين المواصفات والكود لصالح الكود).
- خصم المريض ثنائي الطبيعة يمكن تركه في مكانين: Receipt.Discount (رقم مطلق أو محسوب) + خصم ضمني عبر PriceList مخصصة لجهة الإحالة (المريض المرسَل من جهة X يحصل تلقائياً على أسعار قائمة X).
- لا حاجة لإضافة Doctor.DiscountPercent لأنها ستُنشئ مسار تسعير موازٍ يُربك حساب VisitTest.Price.

1.3 Gender.Both

المصدر: GitHub Repo + MasrLab Spec + RealLab PDF

التفاصيل:

| البُعد | النتيجة |
|---|---|
| في المواصفات (Entity 6 — ReferenceValue) | Gender (النوع: ذكر/أنثى/كلاهما) — يذكر "كلاهما" صراحةً |
| في RealLab PDF | لا يظهر خيار "كلاهما" صراحةً في شاشات إضافة القيم المرجعية المذكورة (الصفحات المُستكشَفة لا تحتوي على هذا التفصيل) |
| في GitHub Repo (Common/Enums/Gender.cs) | public enum Gender { Male, Female } — قيمتان فقط، Both غير موجود |
| استخدام Gender | يظهر في: Patient.Gender (منطقي: قيمتان)، و ReferenceValue.Gender (يحتاج قيمة ثالثة "كلاهما" ليدل على أن القيمة المرجعية تنطبق على الجنسين — مثلاً Hemoglobin range للأطفال) |

أهميته لـ Domain: حرج — يجب تنفيذه.

ملاحظات إضافية:
- تعارض واضح بين المواصفات والكود لصالح المواصفات.
- قرار مقترح: لا تُضِف Both إلى Gender enum (سيربك Patient.Gender). بدلاً من ذلك، أنشئ enum منفصل خاص بـ ReferenceValue مثلاً:
  ReferenceValueGender { Male, Female, Both }
  أو استخدم Gender? Gender (nullable) في ReferenceValue، حيث null = ينطبق على الجنسين. الحل الأول (enum منفصل) أنظف من ناحية Domain.

1.4 Sample.CollectionStatus

المصدر: GitHub Repo + RealLab PDF

التفاصيل:

| البُعد | النتيجة |
|---|---|
| في المواصفات (Entity 9) | CollectionStatus (مسحوبة/غير مسحوبة) — قيمتان فقط |
| في RealLab PDF (صفحات 208-210) | حالتان فقط: "مسحوبة" و"غير مسحوبة". تغيير الحالة يتم بالنقر على العينة، فتنتقل إلى قائمة "العينات المسحوبة" أسفل النافذة. لا يوجد ذكر لحالة "مرفوضة" (Rejected) أو "تالفة" أو زر تراجع صريح. الحالة لا تمنع إدخال النتائج (وظيفة تنظيمية فقط) |
| في GitHub Repo (Entities/Core/Sample.cs) | public string CollectionStatus { get; set; } = string.Empty; — من نوع string، بينما يوجد enum جاهز Common/Enums/SampleStatus.cs بقيمتين { Collected, NotCollected } غير مستخدَم في Sample |
| الاستخدام الفعلي لـ SampleStatus enum | يُستخدَم في PatientVisit.SampleStatus (وليس في Sample.CollectionStatus) — تناقض داخل الكود نفسه |

أهميته لـ Domain: حرج — يجب تنفيذه.

ملاحظات إضافية:
- فجوة تصميمية: Sample.CollectionStatus يجب أن يكون من نوع SampleStatus (Enum) وليس string. القيمة string.Empty كافتراضي خطرة (تسمح بحالات غير معرَّفة).
- قرار مقترح: غيّر النوع إلى SampleStatus واحذف الحقل من PatientVisit (بما أنه معلومة تخص العينة، وليس الزيارة).
- لا حاجة لإضافة حالة Rejected في هذا الإصدار لأن PDF لا يذكرها.

1.5 OutsourcedSample.SettlementStatus

المصدر: GitHub Repo + RealLab PDF + MasrLab Spec

التفاصيل:

| البُعد | النتيجة |
|---|---|
| في المواصفات (Entity 18) | SettlementStatus (حالة التصفية) — بلا قائمة قيم محددة |
| في RealLab PDF (القسم 6-4 / 6-5) | يذكر فقط: "تصفية حساب العينات المرسلة للخارج" + مصطلح "خالص" عند التسديد الكامل. لم تُذكر مصطلحات Pending/Partial/Cleared بالإنجليزية |
| في GitHub Repo (Entities/Financial/OutsourcedSample.cs) | public string SettlementStatus { get; set; } = string.Empty; — من نوع string بدون enum مقابل |
| الاستنتاج المنطقي | من سير عمل PDF يمكن استخلاص 3 حالات ضمنية: Pending (مُرسَل، لم يُدفع للمعمل الخارجي)، PartiallySettled (دُفع جزء)، Settled (خالص) |

أهميته لـ Domain: حرج — يجب تنفيذه.

ملاحظات إضافية:
- فجوة enum مفقود: يجب إنشاء SettlementStatus enum في Common/Enums/.
- قرار مقترح:
  SettlementStatus { Pending, PartiallySettled, Settled }
  ويصبح OutsourcedSample.SettlementStatus من نوع SettlementStatus بدلاً من string.
- ملاحظة: الاعتماد على string يمنع أي فحص برمجي على انتقالات الحالة (State Machine — انظر القسم 8 في المرحلة الثانية).

🟦 القسم 2 — العلاقات (Relationships)

المصدر: MasrLab Spec (Section 3) + GitHub Repo (فحص Foreign Keys في الكيانات الفعلية) + RealLab PDF

ملاحظة تمهيدية على الكود الفعلي: طبقة Domain في niamod @ 9b6fda5 لا تحتوي على أي ICollection navigation properties — كل العلاقات مُعبَّر عنها فقط بـ Foreign Key IDs (int/int?). هذا القرار يترك تحميل العلاقات لـ EF Configurations في Infrastructure، وهو خيار DDD مشروع، لكنه يعني أن Aggregates لا تحفظ بنية شجرية داخل Domain.

جدول العلاقات الـ 27 مع الإجابات المدمجة

| # | العلاقة | Cardinality النهائية | Nullable FK | Cascade Delete | Navigation في Domain؟ | ملاحظات |
|---|---|---|---|---|---|---|
| R-01 | Patient → PatientVisit | 1:N | PatientVisit.PatientId = NOT NULL | ⚠️ Restrict (Soft Delete على Patient لا يحذف الزيارات) | لا (FK فقط في الكود) | Aggregate Root = Patient لا يملك Visits (Visit مستقل — انظر القسم 4) |
| R-02 | Patient → PatientHistory | ليس علاقة Entity — بل Read Query | — | — | لا يوجد | View مُشتق (سبق شرحه في 1.1) |
| R-03 | PatientVisit → TestResult (عبر VisitTest) | 1:N بشكل حقيقي (نادر)، 1:1 في 99% من الحالات | TestResult.VisitTestId = NOT NULL | Restrict (Soft Delete) | لا | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-04 | PatientVisit → Sample | 1:N | Sample.PatientVisitId = NOT NULL | Restrict | لا | كل زيارة قد تحتاج عدة عينات (Blood + Urine مثلاً) |
| R-05 | Sample → SampleCollection | 1:1 | SampleCollection.SampleId = NOT NULL | Cascade مقبول (السجل تابع للعينة) | لا | لكن الكود يفصلهما — قرار غريب: عملية السحب موجودة داخل Sample.CollectionStatus، فوجود SampleCollection تكرار — انظر ملاحظة القسم 4 |
| R-06a | PatientVisit → VisitTest | 1:N | VisitTest.PatientVisitId = NOT NULL | Cascade (VisitTest داخل حدود PatientVisit Aggregate) | يجب إضافتها | العلاقة الجوهرية — كل زيارة = مجموعة VisitTests |
| R-06b | Test → VisitTest | 1:N | VisitTest.TestId = NOT NULL | Restrict (Test كيان مرجعي) | لا | Test يظهر في كل زيارة تطلبه |
| R-06c | VisitTest → TestResult | 1:N (99% = 1:1) | TestResult.VisitTestId = NOT NULL | Cascade (Result داخل VisitTest Aggregate) | يجب إضافتها | راجع R-03 |
| R-07 | Test → ReferenceValue | 1:N | ReferenceValue.TestId = NOT NULL | Cascade | لا في الكود، يجب إضافتها | لكل تحليل عدة نطاقات (Male-Adult, Female-Adult, Child) |
| R-08 | Test → Comment | 1:N | Comment.TestId = NOT NULL | Cascade | لا | PDF يؤكد: عدة كومنتات لنفس التحليل ممكنة |
| R-09 | Test ↔ TestGroup | M:N | يُحل عبر TestGroupItem | Cascade من TestGroup إلى Items | لا | ⚠️ مشكلة في الكود: TestGroup.TestIds هو string (قائمة نصية) بينما يوجد أيضاً TestGroupItem — تكرار مربك؛ يجب حذف TestGroup.TestIds والاعتماد على TestGroupItem |
| R-10 | Test → PriceListItem | 1:N | PriceListItem.TestId = NOT NULL | Restrict | لا | التحليل يظهر في عدة قوائم أسعار |
| R-11 | PriceList → PriceListItem | 1:N | PriceListItem.PriceListId = NOT NULL | Cascade | لا في الكود | Items جزء من PriceList Aggregate |
| R-12 | PriceList → ReferralEntity | 1:N | ReferralEntity.PriceListId = NOT NULL في الكود، ⚠️ يجب أن يكون nullable | Restrict | لا | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-13 | ReferralEntity → Doctor | ليست علاقة كيانية — بل تمييز نوعي | — | — | لا | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-14 | Patient → Doctor | N:1 | Patient.DoctorId — في الكود NOT NULL (int وليس int?) | Restrict | لا | ⚠️ يجب أن يكون nullable (مرضى بلا طبيب معالج) |
| R-15 | Patient → ReferralEntity | N:1 | Patient.ReferralEntityId — في الكود NOT NULL | Restrict | لا | ⚠️ يجب أن يكون nullable (مرضى نقدي مباشر) |
| R-16 | PatientVisit → Doctor | N:1 | مفقود في الكود ❌ (لا يوجد DoctorId في PatientVisit) | — | — | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-17 | PatientVisit → ReferralEntity | N:1 | مفقود في الكود ❌ (لا يوجد ReferralEntityId في PatientVisit) | — | — | نفس ملاحظة R-16 |
| R-18 | Culture → Organism | صورياً 1:N، فعلياً حقول نصية | — | — | لا | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-19 | Culture ↔ Antibiotic (Sensitivity) | M:N (مع attribute = SensitivityLevel) | يُحل عبر Sensitivity | Cascade من Culture إلى Sensitivities | يجب إضافتها | العلاقة صحيحة في الكود |
| R-20 | PatientVisit → OutsourcedSample | 1:N | OutsourcedSample.PatientVisitId = NOT NULL | Restrict | لا | ⚠️ تكرار مع VisitTest.IsOutsourced — راجع القسم 3 |
| R-21 | PatientVisit → Receipt | 1:1 | Receipt.PatientVisitId = NOT NULL (unique) | Restrict | لا | راجع الملاحظة التفصيلية أدناه ⬇ |
| R-22 | Account → CashTransaction | 1:N | ⚠️ مفقود في الكود: CashTransaction يحوي EntityId عام (بلا FK لـ Account) | — | — | فجوة تصميمية — يجب إضافة AccountId أو حذف العلاقة من المواصفات |
| R-23 | User → Permission | 1:N | Permission.UserId = NOT NULL | Cascade | لا | صحيح في الكود |
| R-24 | User → AttendanceLog | 1:N | AttendanceLog.UserId = NOT NULL | Restrict (سجلات مالية لا تُحذف) | لا | صحيح |
| R-25 | User → AuditLog | 1:N | AuditLog.UserId = NOT NULL | Restrict | لا | صحيح |
| R-26 | Report → ReportTemplate | N:1 | لا يوجد Entity "Report" في الكود | — | — | Report مفهوم افتراضي (طباعة فقط) — العلاقة نظرية |
| R-27 | Printer → PurposeType | N:1 (Enum) | Printer.PurposeType = enum PrinterPurposeType | — | — | صحيح؛ ليست علاقة كيانية بل Enum |

الملاحظات التفصيلية للعلاقات المشكوك فيها

🔴 R-03 & R-06c — هل VisitTest → TestResult حقاً 1:N؟

المصدر: RealLab PDF + المواصفات

التحليل:
- المواصفات (Section 3, R-6c): "معظم الحالات تكون 1:1، لكن العلاقة معلَنة 1:N للتحاليل متعددة المعاملات مثل CBC"
- PDF: يعرض CBC، Lipid Profile، Kidney Function كتحاليل واحدة داخل الشاشة، لكن كل معامل يُدخَل في سطر مستقل داخل التقرير (Hemoglobin, WBC, RBC ...) — أي أن CBC هو TestGroup وليس Test واحد بحسب النموذج
- الكود: TestResult.VisitTestId (FK) — تقنياً يسمح بعدة TestResult لنفس VisitTest

القرار المُقترَح:
- 1:N على مستوى Model (لدعم CBC كتحليل واحد مع 8 نتائج معاملات — البديل هو تفكيك CBC إلى 8 Tests في TestGroup، وهو نمط الكود الحالي).
- من ناحية DDD، إبقاء 1:N يعطي مرونة حتى مع بقاء الاستخدام العام 1:1.

🔴 R-12 — PriceList → ReferralEntity Cardinality

المصدر: MasrLab Spec + RealLab PDF + GitHub Repo

التحليل:
- المواصفات: 1:N (قائمة واحدة لعدة جهات) — قائمة الأسعار "التعاقدية" مثلاً تُطبَّق على 10 شركات تأمين دفعة واحدة
- الكود: ReferralEntity.PriceListId = int (NOT NULL) — يدعم N:1 من ReferralEntity إلى PriceList (المعنى الصحيح)
- ⚠️ مشكلة: الحقل NOT NULL — يعني كل جهة إحالة مُلزَمة بقائمة أسعار، لكن ماذا عن الجهات التي تستخدم القائمة الافتراضية (Cash)؟

القرار المُقترَح:
- Cardinality: 1:N (PriceList → ReferralEntity) — كما في المواصفات ✅
- Nullability: يجب أن يكون PriceListId nullable (int?) في ReferralEntity — الجهة قد تستخدم القائمة الافتراضية
- جهة واحدة لا يمكن أن يكون لها أكثر من PriceList (تناقض مالي محتمل).

🔴 R-13 — ReferralEntity → Doctor (هل هي تبعية حقيقية؟)

المصدر: MasrLab Spec + GitHub Repo + RealLab PDF

التحليل:
- المواصفات: 1:N (تبعية) — الطبيب نوع من جهات الإحالة
- الكود: Doctor كيان مستقل تماماً (Entities/Administrative/Doctor.cs) بـ DoctorId منفصل — لا يرث من ReferralEntity ولا يشير إليه
- في نفس الوقت: ReferralEntityType.TreatingDoctor موجود في enum — يعني ReferralEntity قد يُستخدَم أيضاً كطبيب معالج
- Patient يحوي كلاً من DoctorId و ReferralEntityId بشكل منفصل

القرار المُقترَح (تعارض واضح بين المواصفات والكود):
- الكود يعتمد نموذج الكيانين المستقلين — وهذا أوضح مفاهيمياً (الطبيب المعالج ≠ جهة الإحالة تجارياً):
  - Doctor = طبيب معالج فردي (اسم، هاتف، عمولة)
  - ReferralEntity = شركة/جهة (تأمين، تعاقد، معمل خارجي)
- قيمة ReferralEntityType.TreatingDoctor في enum هي حالة حدّية (بعض العيادات تعتبر الطبيب "جهة إحالة" لأغراض التسعير)
- قرار: أبقِ الكيانين منفصلين كما في الكود، واحذف "الطبيب نوع من جهات الإحالة" من المواصفات، أو أعد صياغتها كـ "يمكن اعتبار الطبيب جهة إحالة عند تفعيل TreatingDoctor في ReferralEntityType"

🔴 R-14/R-15/R-16/R-17 — Doctor و ReferralEntity على المريض أم على الزيارة؟

المصدر: MasrLab Spec + GitHub Repo + RealLab PDF

التحليل:

| الكيان | Doctor | ReferralEntity |
|---|---|---|
| Patient (في الكود) | ✅ DoctorId NOT NULL | ✅ ReferralEntityId NOT NULL |
| PatientVisit (في الكود) | ❌ مفقود | ❌ مفقود |
| المواصفات | تنص على R-14 (Patient→Doctor) و R-16 (PatientVisit→Doctor) — أي كلاهما | نفس الشيء (R-15 و R-17) |

المنطق العملي من PDF:
- المريض عادةً له طبيب "معتاد" (Referring Physician) — يُخزَّن على Patient
- لكن زيارة معينة قد تتم عبر طبيب آخر (استبدال، طبيب طوارئ، ...) — لذا Visit يحتاج FK اختياري

القرار المُقترَح:
- أضِف DoctorId (nullable) و ReferralEntityId (nullable) إلى PatientVisit — كما تقول المواصفات
- اجعل Patient.DoctorId و Patient.ReferralEntityId nullable — Default fallback
- قاعدة عملية (Business Invariant): إذا كانت Visit.DoctorId == null، يُستخدَم Patient.DoctorId. هذا Invariant يجب توثيقه في القسم 9 (المرحلة الثانية).

🔴 R-18 — Culture → Organism (كيان أم نص؟)

المصدر: GitHub Repo + RealLab PDF + المواصفات

التحليل:
- الكود: Culture.OrganismA/B/C هي string? (نص حر nullable) — ليست FK لكيان Organism
- لكن Organism كيان موجود في Entities/Culture/Organism.cs (Id + Name فقط)
- PDF: يذكر "إضافة مضاد حيوي للمزرعة" (Master Data) لكن لا يذكر صراحةً "إضافة كائن حي جديد للنظام"؛ ولا يوضح إن كانت الحقول قوائم منسدلة أم نصاً حراً — يوحي فقط بوجود قائمة مرجعية

القرار المُقترَح:
- تصميم هجين مقبول (كما في الكود): Organism كيان مرجعي (Master Data) لكن Culture.OrganismA/B/C تُخزَّن كنص للسرعة والمرونة (اسم البكتيريا قد يكون علمياً معقداً)
- البديل الأنظف: تحويلها إلى OrganismAId (FK, nullable), OrganismBId (FK, nullable), OrganismCId (FK, nullable) — أفضل لبنك بيانات Master Data ومتّسق مع Sensitivity
- قرار توفيقي مُقترَح: أبقِ string كما في الكود (لأن PDF لم يؤكد وجود قائمة صارمة)، لكن أضف Domain Service OrganismSuggestionService يستعلم Organism للاقتراحات (Autocomplete)

🔴 R-20 — تكرار OutsourcedSample مع VisitTest.IsOutsourced

المصدر: GitHub Repo + المواصفات

التحليل:
- الكود يحوي كلا الآليتين:
  - VisitTest فيه: IsOutsourced, ExternalLabId, CostPrice, Notes
  - OutsourcedSample كيان منفصل فيه: PatientVisitId, TestId, ExternalLabId, CostPrice, SettlementStatus
- تكرار واضح — نفس المعلومات (ExternalLabId, CostPrice) في مكانين

القرار المُقترَح:
- ✅ الاحتفاظ بـ OutsourcedSample كـ Financial Aggregate مستقل يحوي دورة حياة التصفية (SettlementStatus)
- ✅ الاحتفاظ بـ IsOutsourced flag في VisitTest كـ علم استرجاع سريع (denormalization متعمَّد)
- ❌ احذف من VisitTest: ExternalLabId, CostPrice, Notes — تُنقَل نهائياً إلى OutsourcedSample
- قاعدة (Invariant): VisitTest.IsOutsourced == true ⟺ يوجد OutsourcedSample مرتبط بنفس (PatientVisitId, TestId)

🔴 R-21 — PatientVisit → Receipt (1:1 حقاً؟)

المصدر: RealLab PDF + المواصفات + الكود

التحليل:
- المواصفات: 1:1 — لكل زيارة إيصال مالي واحد
- PDF (صفحات 17-21): شاشة الحساب الواحدة تعرض إجمالي/خصم/مدفوع سابقاً/مدفوع الآن — كلها ضمن إيصال واحد. لكن الشاشة تدعم إعادة الفتح لتسديد المتبقي (المدفوع سابقاً يُنقل إلى الحقل بذلك الاسم) — أي أن الإيصال يُحدَّث، لا يُصدَر إيصال جديد.
- إعدادات الإيصال في PDF (صفحة 204) تذكر "إمكانية طباعة الإيصال مرة واحدة أو أكثر" — هذا إعادة طباعة، لا إصدار جديد

القرار المُقترَح:
- 1:1 مؤكد — Receipt.PatientVisitId يجب أن يحمل قيد UNIQUE إضافة لكونه FK
- كل تسديدات المريض تُحدَّث داخل نفس Receipt عبر تحديث PaidNow/PaidPrevious/Remaining
- بديل مستقبلي (خارج نطاق هذا الإصدار): لو احتيج تتبع كل عملية دفع كسجل منفصل، يُنشَأ Payment ككيان تابع لـ Receipt (1:N) — لكن حالياً مدمج داخل Receipt

🟦 القسم 3 — أنواع البيانات الصحيحة لكل حقل

3.1 Test.TurnaroundTime

المصدر: GitHub Repo + RealLab PDF + MasrLab Spec

التفاصيل:
- في المواصفات: TurnaroundTime (المدة الزمنية للإنجاز) — بدون نوع محدد
- في الكود: public string TurnaroundTime { get; set; } = string.Empty; — string حر
- في PDF: لم يظهر تفصيلياً في الصفحات المُستكشَفة — القسم 3-1 (إضافة تحليل) يحوي الحقل لكن دون تحديد الشكل

أهميته لـ Domain: تحسيني.

القرار المُقترَح:
- TimeSpan أفضل من string — يسمح بحسابات (مثلاً: هل التحليل متأخر؟)
- بديل عملي: int TurnaroundHours (عدد ساعات، أسهل للإدخال في الواجهة)
- الاختيار الحالي (string) مقبول للـ MVP لكنه يمنع أي منطق "مدة متأخرة" لاحقاً

3.2 Permission.ScreenId

المصدر: GitHub Repo + MasrLab Spec

التفاصيل:
- في المواصفات (Entity 20): ScreenId/OperationId (نعم/لا) — القسم 5 يعدّد 13 صلاحية مرقّمة
- في الكود: public int ScreenId { get; set; } و public int OperationId { get; set; } — كلاهما int
- في PDF: الصلاحيات هي "شاشات" (Screens) مسماة (Patient Screen, Cases Screen, Accounts Screen…) بدون أرقام صريحة

أهميته لـ Domain: مهم.

القرار المُقترَح:
- حوّل ScreenId إلى enum ScreenType { Patient, Cases, Accounts, Statistics, Settings, Laboratory, Users, ... } — أكثر أماناً وتوثيقاً
- حوّل OperationId إلى enum PermissionOperation { View, Add, Edit, Delete, Print, ... }
- الاعتماد على int هو رائحة كود سيئة — يسمح بقيم غير معرَّفة

3.3 AttendanceLog.Overtime / Delays / BreakPeriods

المصدر: GitHub Repo + RealLab PDF (القسم 4-3)

التفاصيل:
- في الكود:
  - Overtime: TimeSpan? ✅
  - Delays: TimeSpan? ✅
  - BreakPeriods: string? ⚠️
- في PDF (القسم 4-3 غير مكشوف مباشرةً): الحضور والانصراف يُسجَّل تلقائياً بأوقات الدخول/الخروج، والعمل الإضافي والتأخير يُحسبان تلقائياً من مقارنة LoginTime/LogoutTime مع دوام قياسي

أهميته لـ Domain: مهم.

القرار المُقترَح:
- TimeSpan? صحيح لـ Overtime/Delays (ساعات:دقائق كافية — لا تحتاج أيام)
- BreakPeriods كـ string غير كافٍ — يجب تحويله إلى:
  ICollection BreakPeriods
  حيث BreakPeriod { Start: DateTime, End: DateTime }
  أو Value Object (انظر المرحلة الثانية — قسم 5)
- قاعدة (Invariant): Overtime + WorkedHours = TotalDurationBetween(LoginTime, LogoutTime) - SumOfBreaks

3.4 Receipt.Currency

المصدر: MasrLab Spec + GitHub Repo + RealLab PDF

التفاصيل:
- في المواصفات (Entity 15): ثابتة = "EGP"، غير قابلة للتعدُّد
- في الكود: public string Currency { get; set; } = "EGP"; — string مع قيمة افتراضية
- في PDF (صفحة 204): ⚠️ إعدادات الإيصال تذكر: "تغيير نوع العملة المستخدمة" — أي أن RealLab يدعم تعدد العملات فعلاً في إعدادات النظام!

أهميته لـ Domain: مهم — تعارض بين المواصفات و PDF.

القرار المُقترَح:
- المواصفات صريحة: "فرع واحد في مصر، لا حاجة لعملات متعددة" → ثبِّت "EGP" كقيمة نظام
- لكن لا تحذف الحقل — أبقِه string ثابت في Receipt (لأغراض العرض على الإيصال)
- خزّن رمز العملة الفعلي في SystemSetting["CurrencySymbol"] = "ج.م" أو "EGP" (المصادر المرجعية للطباعة)
- تعارض PDF: RealLab نظام تجاري متعدد الدول، بينما MasrLab تخصيص مصري بحت — قرار MasrLab نافذ

3.5 Account.NetProfit

المصدر: RealLab PDF (صفحات 174-189) + MasrLab Spec + GitHub Repo

التفاصيل:
- في المواصفات (Entity 16): NetProfit (صافي الربح) — بدون معادلة صريحة
- في PDF (صفحة 175): "لمعرفة إجمالي عينات المرضى وقيمة الخصومات وما تم تحصيله وما لم يحصل، والمتاح في الخزينة، وصافي الربح *بعد التسديد والتحصيل*" — أي أن NetProfit يُحسَب:
  NetProfit = TotalCollected - TotalDiscount - CashOutflows (صرف نقدي)
  وليس TotalIncome - TotalDiscount فقط
- في الكود: decimal NetProfit مخزَّن مباشرة — يجب أن يكون محسوباً وليس مخزَّناً (Denormalization خطرة)

أهميته لـ Domain: حرج.

القرار المُقترَح:
- المعادلة الصحيحة:
  NetProfit = TotalIncome − TotalDiscount − TotalOutsourcedCost − TotalCashWithdrawals + TotalCashDeposits
  حيث:
  - TotalIncome = مجموع Receipt.Total للفترة
  - TotalDiscount = مجموع Receipt.Discount للفترة
  - TotalOutsourcedCost = مجموع OutsourcedSample.CostPrice للفترة
  - CashWithdrawals/Deposits = من CashTransaction
- قرار Domain: اجعل NetProfit محسوباً في Domain Service (AccountingService.CalculateNetProfit(Account)) وليس مخزَّناً كحقل — أو خزّنه لكن مع تعليق واضح "Snapshot عند إغلاق الفترة"

3.6 Doctor.CommissionPercent vs DiscountPercent

المصدر: GitHub Repo + RealLab PDF + MasrLab Spec

التفاصيل: (أُجيب في القسم 1.2 أعلاه)

الخلاصة:
- الطبيب: CommissionPercent فقط (decimal — نسبة مئوية من 0 إلى 100)
- خصم المريض: يُدخَل يدوياً في Receipt.Discount (قيمة مطلقة)
- خصم الجهة: ضمني عبر ربط ReferralEntity → PriceList (المريض يحصل على أسعار قائمة الجهة)

أهميته لـ Domain: مهم.

القرار المُقترَح:
- ✅ الكود صحيح — أبقِ Doctor.CommissionPercent فقط
- ⚠️ لكن أضف قيداً: CommissionPercent يجب أن تكون في [0, 100] — Value Object أفضل من decimal خام (انظر المرحلة الثانية)

🟦 القسم 4 — Aggregate Roots

المصدر: استنتاج من MasrLab Spec + RealLab PDF + GitHub Repo (فحص الوصول من الواجهات والـ Repositories الموجودة)

فحص Repositories الفعلية في الكود (Interfaces): — تدل على Aggregate Roots الفعلية
- IPatientRepository ✅
- IVisitRepository ✅
- ICultureRepository ✅
- IAccountingRepository ✅ (لكيان Account)
- IAuditLogRepository, IStatisticsRepository, ITestResultRepository, IUnitOfWork

قاعدة DDD: Aggregate Root = كيان له Repository مستقل + يُشار إليه من خارج الحدود.

جدول Aggregate Roots مع القرارات

| Aggregate Root | هل هو Aggregate Root؟ | الكيانات التي يملكها (داخل الحدود) | المبرر |
|---|---|---|---|
| Patient | ✅ نعم | لا شيء (Patient كيان مستقل بلا أطفال داخليين) | له IPatientRepository؛ PDF يظهر شاشة "إضافة مريض جديد" كنقطة دخول رئيسية؛ Visit تُدار من Aggregate آخر (وليست تابعة لـ Patient) |
| PatientVisit | ✅ نعم — الأهم | VisitTest (Cascade Delete)، TestResult (عبر VisitTest)، Sample (Cascade)، SampleCollection (عبر Sample)، Receipt (1:1)، OutsourcedSample (Cascade) | له IVisitRepository؛ PDF: زيارة كاملة تُنشَأ كوحدة واحدة في شاشة إضافة المريض (Visit + Tests + Receipt = معاملة واحدة). Invariants حرجة (Receipt.Total يحسب من VisitTests) — كلها داخل حدود Visit |
| Test | ✅ نعم | ReferenceValue، Comment، (TestGroupItem غالباً) | Master Data — يُدار مستقلاً من شاشة System → Tests. لا يوجد Repository مخصص لكن يظهر عبر IRepository |
| User | ✅ نعم | Permission، AttendanceLog، AuditLog (كلها كيانات مستقلة عملياً) | User له شاشة إدارة مستقلة (Users icon). لكن Permission تابع بالكامل لـ User (Cascade Delete)، بينما AuditLog ليس تابعاً لـ User (سجل تدقيق يبقى حتى لو حُذف المستخدم) — AuditLog هو Aggregate منفصل |
| Account | ✅ نعم | CashTransaction (بشرط ربطها بـ AccountId — انظر R-22) | له IAccountingRepository؛ PDF: الجرد يُنشَأ كوحدة فترية |
| Culture | ✅ نعم | Sensitivity (Cascade Delete — Sensitivity لا تعيش بدون Culture) | له ICultureRepository؛ Culture ليست تابعة لـ Visit تصميمياً (يُشار إليها من Sample) — قابلة للنقاش لكن الكود الحالي يعتبرها منفصلة |

كيانات ليست Aggregate Roots (كيانات تابعة أو Master Data)

| الكيان | ينتمي إلى Aggregate | مبرر |
|---|---|---|
| VisitTest | PatientVisit | لا معنى له خارج زيارة |
| TestResult | PatientVisit (عبر VisitTest) | تابع للزيارة |
| Receipt | PatientVisit | 1:1 مع الزيارة |
| Sample | PatientVisit | تابع للزيارة |
| SampleCollection | PatientVisit (عبر Sample) | نمط سجل عملية |
| OutsourcedSample | PatientVisit | جزء من فاتورة الزيارة |
| ReferenceValue | Test | لا يعيش بدون Test |
| Comment | Test | لا يعيش بدون Test |
| TestGroupItem | TestGroup | كيان وسيط |
| Sensitivity | Culture | لا يعيش بدون Culture |
| Permission | User | لا يعيش بدون User |
| AttendanceLog | Aggregate Root مستقل | سجل مستقل (لا يُحذَف مع User) — نمط "Log" |
| AuditLog | Aggregate Root مستقل | نفس السبب |
| Doctor | Aggregate Root مستقل (Master Data) | مُشار إليه من Patient/Visit/Account |
| ReferralEntity | Aggregate Root مستقل (Master Data) | نفس السبب |
| PriceList | Aggregate Root مستقل | تحوي PriceListItem (Cascade) |
| TestGroup | Aggregate Root مستقل (Master Data) | تحوي TestGroupItem |
| Organism / Antibiotic | Aggregate Roots مستقلة (Master Data صغيرة) | مرجعية |
| WorkSheet | Aggregate Root مستقل | تقرير مُنشأ |
| CashTransaction | Account (بعد إصلاح R-22) | حالياً مفكوك — يجب ربطه |
| SystemSetting / Printer / ReportTemplate | Aggregate Roots صغيرة (كل واحدة مستقلة) | إعدادات — لا Cascade |

إجمالي Aggregate Roots المقترحة: 17 root

أهميته لـ Domain: حرج — يحدد بنية Repositories و Unit of Work.

ملاحظة إضافية:
- الكود الفعلي يحوي فقط 5 Repositories مخصصة (IPatient/IVisit/ICulture/IAccounting/ITestResult) + IRepository عام + IAuditLog/IStatistics/IUnitOfWork
- ⚠️ ITestResultRepository مشكوك فيه — TestResult ليس Aggregate Root؛ يجب الوصول إليه عبر IVisitRepository. الاحتفاظ به مبرَّر فقط للاستعلامات الإحصائية عبر مرضى متعددين (كما يظهر: GetByPatientIdAsync)

🟦 القسم 5 — Value Objects

المصدر: استنتاج من MasrLab Spec + RealLab PDF + GitHub Repo

جدول Value Objects المُقترَحة

| المفهوم | هل Value Object؟ | مبرر Domain + دليل RealLab PDF |
|---|---|---|
| Age (Years/Months/Days) | ✅ نعم — قوي جداً | يتكرر في Patient (AgeYears/AgeMonths/AgeDays) و ReferenceValue (AgeMin/AgeMax + AgeUnit). PDF يوضح دخول السن بأي من الوحدات الثلاث. VO مقترح: Age(Years, Months, Days) أو Age(Value, AgeUnit) مع تحويلات (سنوات → أيام). حالياً في الكود ثلاثة حقول منفصلة في Patient — أفضل تجميعها. Invariant: جميعها ≥ 0، ولا يمكن جميع القيم = 0 |
| Phone | ✅ نعم — مفيد | يظهر في Patient, Doctor, ReferralEntity. Validation مصري: يجب أن يبدأ بـ 010, 011, 012, 015 ويكون 11 رقم. PDF لا يذكر Validation صراحة لكن سياق مصر يفرضه. VO: EgyptianPhone(Value) مع Static Factory EgyptianPhone.Create(string) يرمي BusinessRuleViolationException |
| Period (Start/End) | ✅ نعم — قوي | يظهر في Account.PeriodStart/End، WorkSheet.PeriodStart/End، وفي شاشات كل التقارير الإحصائية. VO: DateRange(Start, End) مع عمليات: Duration, Contains(date), Overlaps(other). Invariant: Start ≤ End |
| NormalRange (Min/Max) | ⚠️ مفضَّل — لكن حالياً string حر | في الكود: ReferenceValue.NormalRange = string (نص مثل "12-16" أو "> 3.5"). PDF يظهر أن بعض النطاقات ليست عددية (نطاقات نصية للتحاليل النوعية مثل "Positive/Negative"). قرار: أبقِه string لكن أضف VO منفصل NumericRange(Min, Max) للاستخدام في المقارنة التلقائية لتحديد ResultStatus. Parser يترجم string → NumericRange عند الإمكان |
| Money (Amount/Currency) | ⚠️ مفيد لكن ليس حرجاً | Currency ثابتة EGP → decimal + قيمة نظام كافيان. VO Money(Amount, Currency) overkill في هذا الإصدار. بديل: أضف VO فقط لضمان Amount ≥ 0 (مثلاً PositiveMoney) — استخدمه في Receipt.Total, PaidNow, Discount |
| Address | ❌ لا | يظهر كـ string واحد في Patient/Doctor/ReferralEntity. PDF: خانة واحدة "العنوان" بلا تقسيم لمحافظة/مدينة/شارع. VO غير مبرَّر حالياً |
| SensitivityLevel | ❌ لا | موجود بالفعل كـ enum في الكود (SensitivityLevel { HighlySensitive, Moderate, Low, Resistant }). Extension Method .ToShortCode() تكفي للتحويل إلى S/I/R، بدلاً من VO كامل |
| LabId (إضافي مُقترَح) | ✅ نعم | كود فريد للمريض (Business Identity). VO يمنع Primitive Obsession ويضع Validation (تنسيق: مثلاً LAB-2026-000123). Uniqueness constraint في القاعدة |
| CommissionPercent (إضافي مُقترَح) | ⚠️ مفيد | ضمان [0, 100] — VO Percentage(Value) قابل لإعادة الاستخدام (يستفيد منه Discount كنسبة مئوية أيضاً) |
| BreakPeriod (إضافي مُقترَح) | ✅ نعم | حل مشكلة AttendanceLog.BreakPeriods = string. Collection of VO |

قواعد عامة لـ Value Objects في MasrLab

1. Immutable (record types في C# 9+)
2. Value Equality (record يوفرها تلقائياً)
3. Self-Validating عبر Static Factory (Create(...)) يرمي BusinessRuleViolationException
4. No Identity — لا Id ولا يرث من BaseEntity
5. تُخزَّن في القاعدة كأعمدة على الكيان الحاوي (EF Owned Types) وليس كجداول منفصلة

أهميته لـ Domain: حرج (Age, DateRange, EgyptianPhone) — تحسيني (Money, Percentage, LabId).

ملاحظات إضافية:
- الكود الحالي لا يحوي أي Value Objects — جميعها Primitive Types. هذه فرصة كبيرة للتحسين.
- بدء التحول تدريجي — ابدأ بـ Age و DateRange لأنهما الأعلى قيمة والأقل مخاطرة.

📊 ملخص المرحلة الأولى — أهم الفجوات المكتشفة

| # | الفجوة | الحدة | الأثر |
|---|---|---|---|
| 1 | PatientHistory غائب كلياً من Domain (View + Read Model مطلوبان) | 🔴 حرج | Module 6 غير قابل للتنفيذ |
| 2 | Gender.Both مفقود من enum → ReferenceValue لا يدعم "كلا الجنسين" | 🔴 حرج | Master Data للتحاليل ناقصة |
| 3 | Sample.CollectionStatus من نوع string بينما SampleStatus enum جاهز غير مستخدَم | 🔴 حرج | تناقض داخلي في الكود |
| 4 | SettlementStatus لا يوجد له enum (string حر) | 🔴 حرج | لا State Machine ممكنة |
| 5 | PatientVisit يفتقد DoctorId و ReferralEntityId (nullable) | 🔴 حرج | لا يمكن تسجيل زيارة بطبيب مختلف عن طبيب المريض |
| 6 | Patient.DoctorId و ReferralEntityId غير nullable → لا يدعم مرضى نقدي بلا جهة | 🟠 مهم | خرق سيناريو شائع |
| 7 | تكرار OutsourcedSample مع VisitTest (ExternalLabId/CostPrice في مكانين) | 🟠 مهم | ازدواج بيانات ومخاطر تعارض |
| 8 | TestGroup.TestIds = string يوجد بجانب TestGroupItem (تكرار) | 🟠 مهم | حل M:N مضاعف — احذف TestIds |
| 9 | CashTransaction.EntityId بلا FK صريح إلى Account | 🟠 مهم | R-22 مكسورة |
| 10 | Permission.ScreenId/OperationId = int بلا enum | 🟡 تحسيني | ضعف Type Safety |
| 11 | Account.NetProfit مخزَّن (يجب أن يكون محسوباً) | 🟠 مهم | مخاطر عدم اتساق البيانات |
| 12 | لا Value Objects في الكود (Age, DateRange, Phone…) | 🟡 تحسيني | Primitive Obsession |

## القسم 6 — Domain Events (الأحداث النطاقية)

| # | اسم الحدث | Aggregate المصدر | الـ Payload | المشتركون المحتملون | متى يُنشر | شرط الإطلاق |
|---|---|---|---|---|---|---|
| E-01 | PatientRegistered | Patient | PatientId, FullName, Gender, Age, CreatedAt | MedicalHistoryService, AuditLog | بعد حفظ مريض جديد | إنشاء Patient ناجح |
| E-02 | PatientUpdated | Patient | PatientId, ChangedFields, UpdatedAt | AuditLog | بعد تعديل بيانات المريض | تعديل حقل واحد على الأقل |
| E-03 | PatientVisitCreated | PatientVisit | VisitId, PatientId, DoctorId, VisitDate, Tests[] | SampleTrackingService, PricingService, ReceiptGenerator | عند حفظ زيارة جديدة | Visit.Save() ناجح |
| E-04 | VisitTestAdded | PatientVisit | VisitId, TestId, Price, IsOutsourced | SampleTrackingService, AccountingService | إضافة تحليل للزيارة | تحليل جديد يُلحق بالزيارة |
| E-05 | VisitTestRemoved | PatientVisit | VisitId, TestId | ReceiptRecalculator, SampleTrackingService | إزالة تحليل | حذف VisitTest قبل إغلاق الزيارة |
| E-06 | SampleCollected | Sample | SampleId, VisitId, TestId, CollectedAt, CollectedBy | ResultEntryService, WorkflowMonitor | ضغط زر "مسحوبة" | Sample.IsCollected تحوَّل true |
| E-07 | SampleUncollectedReverted | Sample | SampleId, VisitId, RevertedAt | AuditLog | إلغاء سحب العينة | حالة عادت لغير مسحوبة |
| E-08 | TestResultEntered | TestResult | ResultId, VisitTestId, Value, EnteredBy, EnteredAt | ReportPrintService, AuditLog | حفظ نتيجة تحليل | Result غير فارغ |
| E-09 | TestResultEdited | TestResult | ResultId, OldValue, NewValue, EditedBy | AuditLog | تعديل نتيجة موجودة | Result سبق حفظه |
| E-10 | ReceiptIssued | Receipt | ReceiptId, VisitId, Total, Discount, Paid, Remaining | AccountingService, CashTransactionService | إصدار فاتورة | Receipt.Save() ناجح |
| E-11 | ReceiptPaymentAdded | Receipt | ReceiptId, Amount, PaidAt | AccountingService | إضافة دفعة | Paid تغيَّر |
| E-12 | DiscountApplied | Receipt | ReceiptId, DiscountValue, AppliedBy | AccountingService, AuditLog | تطبيق خصم | Discount > 0 |
| E-13 | OutsourcedSampleSent | OutsourcedSample | OutsourcedSampleId, VisitTestId, ExternalLabId, CostPrice, PatientPrice, SentAt | AccountingService, WorkflowMonitor | تحديد "Taken outside lab" | IsOutsourced = true |
| E-14 | OutsourcedResultReceived | OutsourcedSample | OutsourcedSampleId, ReceivedAt, Result | ResultEntryService | استلام نتيجة خارجية | Result وصلت من المعمل الخارجي |
| E-15 | CultureRecorded | Culture | CultureId, VisitTestId, Organisms, ColonyCount | SensitivityService | حفظ بيانات المزرعة | Culture.Save() |
| E-16 | SensitivityRecorded | Sensitivity | SensitivityId, CultureId, Antibiotics{H,M,L,R} | ReportPrintService | حفظ حساسية | Sensitivity مرتبطة بـ Culture |
| E-17 | CashDeposited | CashTransaction | TransactionId, Amount, PerformedBy, Date | AccountingService | إيداع نقدي | Type = Deposit |
| E-18 | CashWithdrawn | CashTransaction | TransactionId, Amount, PerformedBy, Date | AccountingService | سحب نقدي | Type = Withdrawal |
| E-19 | CommentAttachedToResult | Comment | CommentId, TestId, Text | ReportPrintService | إرفاق كومنت بنتيجة | Comment مرتبط بـ TestResult |
| E-20 | VisitClosed | PatientVisit | VisitId, ClosedAt, FinalTotal | AccountingService, ReportService | إغلاق الزيارة | كل النتائج مُدخلة والفاتورة مسددة/مؤكدة |

---

## القسم 7 — Domain Services (الخدمات النطاقية)

| # | اسم الخدمة | المسؤولية | المدخلات | المخرجات | Aggregates المتأثرة |
|---|---|---|---|---|---|
| DS-01 | PricingService | حساب إجمالي الزيارة مع الخصم و Extra Services؛ يستخدم snapshot السعر وقت الإنشاء | List<VisitTest>, List<ExtraService>, Discount | Total, Subtotal, DiscountAmount | PatientVisit, Receipt |
| DS-02 | AccountingService | حساب NetProfit وفق: TotalIncome − TotalDiscount − TotalOutsourcedCost − CashWithdrawals + CashDeposits | DateRange, BranchId | NetProfit, TotalIncome, TotalDiscount, TotalOutsourcedCost | Receipt, OutsourcedSample, CashTransaction |
| DS-03 | MedicalHistoryService | تجميع التاريخ المرضي وإدراجه تلقائياً في التقرير | PatientId | AggregatedHistory (Diabetes, HTN, HCV, HBV, Renal, SLE...) | Patient, PatientVisit, TestResult |
| DS-04 | SampleTrackingService | تتبع حالة سحب العينة (مسحوبة/غير مسحوبة) لكل تحليل بزيارة | VisitId | List<SampleStatus> | PatientVisit, Sample |
| DS-05 | OutsourcingService | إدارة إرسال العينات لمعامل خارجية وحساب فارق التكلفة | VisitTestId, ExternalLabId, CostPrice, PatientPrice | OutsourcedSample | PatientVisit, OutsourcedSample, Receipt |
| DS-06 | ReceiptCalculationService | إعادة حساب الفاتورة عند أي تغيير في التحاليل أو الخصم أو الدفع | ReceiptId | Total, Paid, Remaining | Receipt, PatientVisit |
| DS-07 | ResultValidationService | التحقق من قيم النتائج ضد النطاق المرجعي للتحليل | VisitTestId, Value | ValidationFlag (Normal/High/Low/Critical) | TestResult |
| DS-08 | CultureSensitivityService | ربط الكائنات الحية بمستويات الحساسية (H/M/L/R) | CultureId, OrganismList, AntibioticList | Sensitivity Matrix | Culture, Sensitivity |
| DS-09 | ReferralCommissionService | حساب عمولة جهة الإحالة/الطبيب على الزيارة | VisitId, ReferralEntityId | CommissionAmount | ReferralEntity, PatientVisit |
| DS-10 | PriceListResolverService | حل السعر المطبق (List للجهة أو الافتراضي) وقت إنشاء الزيارة | PatientId, DoctorId, ReferralEntityId, TestId | ResolvedPrice | Doctor, ReferralEntity, PriceList, VisitTest |

---

## القسم 8 — State Machines (آلات الحالة)

### 8.1 Sample

| Current State | Trigger | Guard | Next State | Event Emitted |
|---|---|---|---|---|
| NotCollected | CollectSample | User has permission | Collected | SampleCollected |
| Collected | RevertCollection | Result not yet entered | NotCollected | SampleUncollectedReverted |

> ملاحظة ملزمة: PDF يذكر حالتين فقط (مسحوبة/غير مسحوبة). لم تُضَف حالات "مرفوضة" أو "تالفة".

### 8.2 OutsourcedSample

| Current State | Trigger | Guard | Next State | Event Emitted |
|---|---|---|---|---|
| Marked | SendToExternalLab | ExternalLabId ≠ null ∧ CostPrice ≥ 0 | Sent | OutsourcedSampleSent |
| Sent | ReceiveResult | Result ≠ null | ResultReceived | OutsourcedResultReceived |
| ResultReceived | AttachToReport | نتيجة موقَّعة | Completed | — |

### 8.3 PatientVisit

| Current State | Trigger | Guard | Next State | Event Emitted |
|---|---|---|---|---|
| Draft | SaveVisit | Tests.Count ≥ 1 | Open | PatientVisitCreated |
| Open | AddTest | Visit not closed | Open | VisitTestAdded |
| Open | EnterAllResults | كل VisitTest لها TestResult | Completed | — |
| Open/Completed | IssueReceipt | Receipt غير موجود | Open/Completed (+Receipt) | ReceiptIssued |
| Completed | CloseVisit | Receipt.Remaining == 0 أو مؤكَّد | Closed | VisitClosed |

### 8.4 Culture

| Current State | Trigger | Guard | Next State | Event Emitted |
|---|---|---|---|---|
| Pending | RecordCulture | Sample = Collected | Recorded | CultureRecorded |
| Recorded | RecordSensitivity | Organisms.Count ≥ 1 | WithSensitivity | SensitivityRecorded |

### 8.5 Receipt

| Current State | Trigger | Guard | Next State | Event Emitted |
|---|---|---|---|---|
| Draft | Issue | Total محسوب | Issued | ReceiptIssued |
| Issued | AddPayment | Amount > 0 ∧ Amount ≤ Remaining | PartiallyPaid/Paid | ReceiptPaymentAdded |
| Issued/PartiallyPaid | ApplyDiscount | لديه صلاحية الخصم | Issued (Recalculated) | DiscountApplied |
| PartiallyPaid | AddPayment | Paid == Total | Paid | ReceiptPaymentAdded |

---

## القسم 9 — Business Invariants (الثوابت التجارية)

| المعرِّف | الوصف الدقيق | Aggregate المسؤول | متى يُفحص | عند الخرق |
|---|---|---|---|---|
| INV-01 | Receipt.Total == Σ(VisitTest.Price) + Σ(ExtraServices.Price) − Discount | Receipt | عند كل تغيير في VisitTest أو ExtraService أو Discount | رفض العملية + إعادة حساب |
| INV-02 | VisitTest.IsOutsourced == true ⟺ يوجد OutsourcedSample مرتبط بنفس (PatientVisitId, TestId) | PatientVisit | عند حفظ VisitTest أو OutsourcedSample | رفض الحفظ (منع تناقض G-07) |
| INV-03 | PatientVisit.DoctorId == null → يُستخدم Patient.DoctorId افتراضياً | PatientVisit | عند إنشاء الزيارة | استبدال تلقائي بـ Patient.DoctorId |
| INV-04 | VisitTest.Price هو snapshot وقت إنشاء الزيارة، لا يتغير عند تعديل قائمة الأسعار (G-15) | VisitTest | عند تعديل PriceList لاحقاً | تجاهل التعديل على VisitTest القديمة |
| INV-05 | NetProfit يجب إعادة حسابه بعد كل CashTransaction أو Receipt | AccountingService | بعد كل Event مالي | إعادة حساب فوري |
| INV-06 | CommissionPercent ∈ [0, 100] | ReferralEntity / Doctor | عند إدخال العمولة | رفض القيمة |
| INV-07 | Age ≥ 0 لكل مكون (Years, Months, Days) ولا يمكن أن تكون كلها 0 معاً | Patient (Age VO) | عند إنشاء/تعديل Patient | ValidationException |
| INV-08 | DateRange.Start ≤ DateRange.End لكل الفترات الزمنية | DateRange VO | عند إنشاء DateRange | ValidationException |
| INV-09 | Receipt.Paid ≤ Receipt.Total (عدم السماح بدفع زائد بدون رصيد مسبق) | Receipt | عند AddPayment | رفض الدفعة الزائدة |
| INV-10 | Discount ≤ Subtotal (لا خصم أكبر من الإجمالي) | Receipt | عند ApplyDiscount | رفض الخصم |
| INV-11 | Sample.CollectedAt ≠ null ⟺ Sample.IsCollected == true | Sample | عند تحديث حالة العينة | تصحيح تلقائي أو رفض |
| INV-12 | TestResult موجودة ⟹ Sample.IsCollected == true | TestResult | عند حفظ TestResult | رفض إدخال النتيجة قبل السحب |
| INV-13 | OutsourcedSample.PatientPrice ≥ OutsourcedSample.CostPrice (لا خسارة افتراضية) | OutsourcedSample | عند الحفظ | تحذير أو رفض حسب السياسة |
| INV-14 | Culture مرتبطة فقط بـ VisitTest من نوع Microbiology | Culture | عند إنشاء Culture | رفض الإنشاء |
| INV-15 | Sensitivity.CultureId ≠ null (لا حساسية بدون مزرعة) | Sensitivity | عند الحفظ | ValidationException |
| INV-16 | ReferralEntity.PriceListId يجب أن يكون موجوداً وفعّالاً | ReferralEntity | عند الربط | رفض الربط |
| INV-17 | لا يمكن حذف Patient/Visit إذا كان لديه Receipt مصدَّرة (Soft Delete فقط) | Patient/PatientVisit | عند الحذف | تحويل إلى Soft Delete |
| INV-18 | كل VisitTest يجب أن يرتبط بـ Patient واحد فقط عبر PatientVisit | VisitTest | عند الحفظ | رفض الحفظ |
| INV-19 | CashTransaction.Amount > 0 (لا معاملات بمبلغ صفر أو سالب) | CashTransaction | عند الإنشاء | ValidationException |
| INV-20 | Comment.Text.Length > 0 ∧ ≤ MaxCommentLength | Comment | عند الحفظ | ValidationException |

---

## القسم 10 — مراجعة حقول PDF مقابل الكود

> ملاحظة: محاولة قراءة ملفات الكود مباشرة من GitHub raw فشلت في هذه الجلسة. المقارنة تعتمد على بنية الكود الموصوفة في `domain1.md` (30 كيان + 14 Enum) والتي تمثل حالة الفرع `niamod @ 9b6fda5`.

### 10.1 Patient (PDF ص 10-14)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| الرقم القومي | NationalId | غير موجود في الكود (لم يذكره domain1.md) | متوسطة |
| قائمة تاريخ مرضي مفصّلة (سكر/ضغط/HCV/HBV/فشل كلوي/ذئبة حمراء) | PatientHistory (SQL View مشتق) | مختلف — Spec يعرّفه كـ View فقط وليس Flags مباشرة على Patient | عالية |
| ملاحظات عامة | Notes / Remarks | غير مؤكد ذكره في domain1.md | منخفضة |

### 10.2 Receipt (PDF ص 17-21)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| المدفوع سابقاً (Previously Paid) | PreviouslyPaid | غير موجود صراحة — يُحسب ديناميكياً من CashTransactions | متوسطة |
| الباقي للمريض (رد باقي) | ChangeDue / RefundToPatient | مفقود | متوسطة |
| ExtraServices على مستوى Receipt | ExtraService entity | غير مؤكد الوجود في الكود | عالية |

### 10.3 PatientVisit + VisitTest (PDF ص 22-28)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| Taken Outside Lab (Checkbox على مستوى VisitTest) | VisitTest.IsOutsourced | موجود، لكن INV-02 غير مضمون هيكلياً | عالية |
| VisitTest.Price كـ snapshot | VisitTest.Price | موجود لكن Immutability غير مضمونة عبر Setter محمي | عالية |
| DoctorId على مستوى الزيارة (Override لطبيب المريض) | PatientVisit.DoctorId (Nullable) | موجود؛ INV-03 يحتاج تنفيذ في Domain Service | متوسطة |

### 10.4 TestResult (PDF ص 70-78)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| Print History Flag على التقرير | TestResult.PrintHistory | مفقود | متوسطة |
| Comments متعددة لكل نتيجة | Comment (كيان منفصل) | موجود، لكن ربط Many-to-Many مع TestResult غير مؤكد | متوسطة |
| Reference Range (نطاق مرجعي) | Test.ReferenceRange | غير مؤكد على مستوى TestResult snapshot | عالية |

### 10.5 OutsourcedSample (PDF ص 140-148)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| Cost Price | OutsourcedSample.CostPrice | موجود | — |
| Patient Price | OutsourcedSample.PatientPrice | موجود | — |
| ExternalLab (المعمل المرسل إليه) | OutsourcedSample.ExternalLabId | غير مؤكد وجود كيان ExternalLab مستقل | عالية |
| ReceivedAt | OutsourcedSample.ReceivedAt | مفقود (State Machine 8.2 تحتاجه) | عالية |

### 10.6 Culture + Sensitivity (PDF ص 38-39، 128-139)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| Organism A / B / C (ثلاثة حقول منفصلة) | Culture.Organisms (Collection) | مختلف — الكود قد يستخدم Collection بدل ثلاثة حقول ثابتة | متوسطة |
| Culture Condition | Culture.Condition | غير مؤكد الوجود | متوسطة |
| Colony Count | Culture.ColonyCount | غير مؤكد الوجود | متوسطة |
| Highly/Moderate/Low/Resistant For (4 قوائم) | Sensitivity.Level (Enum) | مختلف — Enum بدل 4 قوائم منفصلة (مقبول تصميمياً) | منخفضة |

### 10.7 Account + CashTransaction (PDF ص 174-189)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| نوع التقرير (مجمع/مفصل/بالأسعار/بالنتائج) | ReportType (Enum) | غير مؤكد وجوده في Domain | منخفضة (Reporting Concern) |
| NetProfit محسوب | AccountingService (Domain Service) | يحتاج تنفيذ صريح — INV-05 | عالية |
| CashTransaction.PerformedBy | CashTransaction.UserId | غير مؤكد الوجود | متوسطة |

### 10.8 Sample (PDF ص 208-210)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| CollectedAt (Timestamp السحب) | Sample.CollectedAt | غير مؤكد | متوسطة |
| CollectedBy (المستخدم الساحب) | Sample.CollectedByUserId | مفقود | متوسطة |
| حالتان فقط (مسحوبة/غير مسحوبة) | Sample.IsCollected (bool) | متوافق | — |

### 10.9 Comment (PDF ص 111-115)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| ربط الكومنت بتحليل بعينه (Test) | Comment.TestId | موجود على الأرجح | — |
| قوالب كومنتات جاهزة (Templates) | CommentTemplate entity | مفقود | متوسطة |

### 10.10 Doctor + ReferralEntity (القسم 3-7)

| حقل PDF | حقل الكود | الحالة | الخطورة |
|---|---|---|---|
| نوع الجهة (طبيب/عينة مرسلة/جهة إحالة) | ReferralEntityType (Enum) | متوقع وجوده ضمن 14 Enum | — |
| Fax | ReferralEntity.Fax | مفقود على الأرجح | منخفضة |
| اسم المسؤول + تليفونه | ContactPerson, ContactPhone | مفقود | متوسطة |
| قائمة الأسعار المرتبطة بالجهة | ReferralEntity.PriceListId | موجود (INV-16) | — |
| CommissionPercent | Doctor.Commission / ReferralEntity.Commission | موجود (INV-06) | — |