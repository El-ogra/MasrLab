لدي الآن كل المعطيات اللازمة من الفحص الفعلي للكود عند الكوميت 613838d. سأنتقل مباشرة إلى إنتاج الوثيقة الهندسية النهائية.

📘 خارطة التنفيذ الاحترافية لاستكمال طبقة الدومين — مشروع MasrLab

مرجع الحقيقة: كود المستودع MasrLab على الفرع niamod عند الكوميت 613838de9e581941b10b1f1991c49aa67b883a25 ("تحليل الدومين").

الكوميت مؤكَّد عبر git log: 613838d تحليل الدومين ← 70757c3 الموجة الثانية ← d531b95 الموجة الأولى ← 602d80c بدء الماراثون ← 9b6fda5 Point of order.

مبدأ التقرير: لا شيء يُقبل كحقيقة إلا ما تم قراءته مباشرة من ملفات src/MasrLab.Domain/\*\* عند هذا الكوميت. وثائق Docs/Domaim1-10.md و Docs/Gaps-after-phase10.md والتقرير السابق تُعامَل كافتراضات فقط.

## 1. ملخص حالة الكود عند الكوميت المستهدف

### 1.1 الإحصائيات الفعلية (بعد العد المباشر لملفات .cs)

| المكوّن | العدد الفعلي | التوزيع/التفصيل |
|--------|:-------------:|-----------------|
| Entities | 35 | Core: 12 · Administrative: 7 · Culture: 4 · Financial: 6 · Settings: 6 |
| Enums | 18 (وليس 16 كما ذكر التقرير السابق) | AccountType, AgeUnit, AuditActionType, AuditEntityType, Gender, PaperSize, PermissionOperation, PrinterPurposeType, ReferenceValueGender, ReferralEntityType, ResultStatus, SampleStatus, ScreenType, SensitivityLevel, SettlementStatus, TransactionType, VisitStatus, WorkSheetType |
| Value Objects | 3 | Age, DateRange, EgyptianPhone — جميعها record |
| Repository Interfaces | 9 | IRepository عام + IAccountingRepository, IAuditLogRepository, ICultureRepository, IPatientRepository, IStatisticsRepository, ITestResultRepository, IUnitOfWork, IVisitRepository |
| Domain Exceptions | 4 | BusinessRuleViolationException, DuplicateLabIdException, EntityNotFoundException, InsufficientPermissionException |
| Read Models / DTOs | 2 | PatientHistoryEntry (13 حقلاً)، StatisticsDto (7 حقول) |
| Common contracts | 3 | BaseEntity (يرث IAuditableEntity + ISoftDeletable), IAuditableEntity, ISoftDeletable |
| Domain Events | 0 | لا وجود لأي IDomainEvent أو مجلد Events/ — تم التأكد عبر grep -rn "IDomainEvent\|DomainEvent" |
| Domain Services | 0 | لا واجهات ولا تنفيذات — التعليقات في Receipt.cs و Account.cs تشير إلى خدمات مستقبلية فقط |
| State Machines | 0 | لا انتقالات حالة محمية داخل الكيانات — الحالات موجودة كـ enums حرة |
| Business Invariants | 0 مُنفَّذة | تعليق واحد فقط في PatientVisit.cs يشير إلى INV-03 كنص توثيقي |
| Navigation Properties | 1 فقط | Receipt.ExtraServiceItems : ICollection — هي الوحيدة في كامل الدومين |
| EF Core Configurations | 0 | لا يوجد IEntityTypeConfiguration في طبقة الدومين (وهذا صحيح معمارياً لأنه ينتمي للـ Infrastructure) |
| Migration فعلية | 0 | لا مجلد Migrations تحت الدومين، ولم يتم إنشاء قاعدة بيانات فعلياً |

### 1.2 الاستنتاج الهيكلي

الكود الحالي نموذج بيانات (Anemic Data Model) يقترب من نمط Active Record بدون سلوك — وليس نموذج دومين (Rich Domain Model) بمعنى DDD. المكوّنات الغائبة كلياً هي: السلوك داخل الكيانات، الأحداث، الخدمات، آلات الحالة، خصائص التنقل، والثوابت التجارية. البنية الأساسية (Base Entity + Soft Delete + Audit) موجودة وسليمة، والأنواع الآمنة (Enums, VOs) مُنشأة لكن غير موصولة بالكيانات.

## 2. نتائج التحقق من الفجوات (Gaps-after-phase10.md)

| # | الوصف | الحالة الفعلية | الدليل المباشر من الكود | الإجراء المطلوب |
|:---:|-------|:--------------:|--------------------------|-----------------|
| G-01 | PatientHistory (Read Model + View + Service) | مؤكدة — تتطلب إجراء | Common/DTOs/PatientHistoryEntry.cs موجود كـ record بـ 13 حقلاً. لا Repository، لا Service، ولا View SQL | إنشاء واجهة IPatientHistoryRepository + Domain Service IMedicalHistoryService. الـ View SQL يبقى في Infrastructure |
| G-02 | ReferenceValueGender.Both | مغلقة | Common/Enums/ReferenceValueGender.cs: {Male, Female, Both} — و ReferenceValue.Gender يستخدمه | — |
| G-03 | Sample.CollectionStatus enum | مغلقة | Sample.cs: public SampleStatus CollectionStatus { get; set; } | — |
| G-04 | SettlementStatus enum | مغلقة | OutsourcedSample.cs: public SettlementStatus SettlementStatus { get; set; } والـ enum بـ 3 قيم | — |
| G-05 | PatientVisit.DoctorId? + ReferralEntityId? | مغلقة | PatientVisit.cs: public int? DoctorId { get; set; } و public int? ReferralEntityId { get; set; } | — |
| G-06 | Patient.DoctorId? + ReferralEntityId? | مغلقة | Patient.cs: كلاهما int? | — |
| G-07 | VisitTest — حذف ExternalLabId/CostPrice/Notes | مغلقة | VisitTest.cs يحوي فقط: PatientVisitId, TestId, Price, IsOutsourced | — |
| G-08 | TestGroup — حذف TestIds | مغلقة | TestGroup.cs = GroupName + GroupPrice فقط. TestGroupItem منفصل | — |
| G-09 | CashTransaction.EntityId → AccountId | مغلقة | CashTransaction.cs: public int AccountId { get; set; } | — |
| G-10 | Permission.ScreenId/OperationId → enum | مؤكدة — تتطلب إجراء | ScreenType (13 قيمة) و PermissionOperation (6 قيم) موجودان لكن Permission.cs لا يزال: public int ScreenId; public int OperationId | تغيير النوع في Permission.cs — مع مراعاة تأثير على UI Layer (قيم ScreenType بدأت من 1 لتوافق أرقام الشاشات القديمة) |
| G-11 | Account.NetProfit مخزَّن | مؤكدة — تتطلب إجراء (مؤجَّل) | Account.cs سطر 11: public decimal NetProfit { get; set; } مع تعليقين يشرحان أن الحساب يتم عبر AccountingService مستقبلاً | يُحوَّل إلى internal set مع حماية، وتُبنى الخدمة في مرحلة Domain Services |
| G-12 | Value Objects غير موصولة | مؤكدة — تتطلب إجراء | Age.cs, DateRange.cs, EgyptianPhone.cs موجودة كاملة. لكن Patient.cs لا يزال يستخدم AgeYears/AgeMonths/AgeDays كـ int، و Phone string، و AttendanceLog.LoginTime/LogoutTime منفصلة | ربط لا إنشاء — تعديل الكيانات لاستخدام VOs الموجودة |
| G-13 | حقول التاريخ المرضي (12 حقل) | مغلقة | Patient.cs: 8 bools + ChronicDiseases + DrugAllergy + Pregnancy + BloodThinning = 12 | — |
| G-14 | ExtraServiceItem + Receipt collection | مغلقة | ExtraServiceItem.cs موجود. Receipt.ExtraServiceItems : ICollection هي الـ Navigation Property الوحيدة في كامل الدومين | — |
| G-15 | VisitTest.Price → internal set | مغلقة | VisitTest.cs: public decimal Price { get; internal set; } مع تعليق snapshot | — |
| G-16 | CardSetting (10 حقول) | مغلقة | CardSetting.cs بـ: CardTitle + 7 bools + HeaderColor + FontSize = 10 | — |
| G-17 | OutsourcedSample.ReceivedAt | مغلقة | public DateTime? ReceivedAt { get; set; } | — |
| G-18 | Sample.CollectedByUserId | مغلقة | public int? CollectedByUserId { get; set; } | — |
| G-19 | CommentTemplate | مغلقة | CommentTemplate.cs: TestId + Text | — |
| G-20 | ReferralEntity.ContactPhone | مغلقة | ReferralEntity.cs: ContactPerson + ContactPhone موجودان | — |
| G-21 | Patient.NationalId | مغلقة (وليست وهمية — التصنيف الصحيح "مغلقة" لأن الحقل يحقق المتطلب) | Patient.cs سطر: public string? NationalId { get; set; } | — لا إجراء |
| G-22 | Receipt.ChangeDue + RefundToPatient | مغلقة | Receipt.cs: كلا الحقلين موجودان | — |
| G-23 | ExternalLab | مغلقة | ExternalLab.cs: Name, Address, Phone, ContactPerson | — |

### 2.1 ملخص التصنيف

| التصنيف | العدد | الفجوات |
|--------|:-----:|---------|
| مغلقة | 19 | G-02..09, G-13..23 |
| مغلقة (كانت تُصنَّف وهمية خطأً) | 1 | G-21 — الحقل موجود، فالحالة الصحيحة "مغلقة"، وليس "وهمية" |
| مؤكدة تتطلب إجراء | 3 | G-01, G-10, G-12 |
| مؤكدة تتطلب إجراء (مؤجَّل لـ Domain Services) | 1 | G-11 |

## 3. نتائج التحقق من Domaim1-10.md

| القسم | الحالة الفعلية في الكود | نسبة التنفيذ | ما هو ناقص |
|-------|------------------------|:------------:|------------|
| 1 — سد الفجوات في الكيانات | مطبَّق بالكامل تقريباً | ~95% | فقط جانب Service لـ PatientHistory (1.1) |
| 2 — العلاقات (27 علاقة) | FKs مطبَّقة، Navigation Properties مفقودة تماماً إلا في Receipt→ExtraServiceItems | ~50% | ICollection ناقصة في: PatientVisit, Test, Culture, PriceList, User, Account. علاقة R-12 (ReferralEntity.PriceListId) لا تزال NOT NULL بينما التوصية nullable |
| 3 — أنواع البيانات الصحيحة | مطبَّق جزئياً | ~30% | Test.TurnaroundTime لا يزال string · Permission.ScreenId/OperationId لا يزالان int · AttendanceLog.BreakPeriods لا يزال string · Account.NetProfit لا يزال stored · Doctor.CommissionPercent بدون validation |
| 4 — Aggregate Roots (17 جذر) | البنية الملائمة موجودة في الـ Repositories لكن التجميع الفعلي غير موجود | ~20% | لا يوجد Root يجمع Children كـ ICollections. لا حدود Aggregate مفعَّلة سلوكياً |
| 5 — Value Objects | موجودة لكن غير موصولة بأي كيان | 0% وصل | Patient (Age/Phone) · Doctor (Phone) · ReferralEntity (Phone) · AttendanceLog (DateRange) |
| 6 — Domain Events (20 حدث E-01..E-20) | غير مطبَّق | 0% | كل شيء: IDomainEvent, حاوية أحداث داخل BaseEntity, INotificationHandler لاحقاً |
| 7 — Domain Services (10 خدمات DS-01..DS-10) | غير مطبَّق | 0% | كل الواجهات + التصميم المبدئي |
| 8 — State Machines | Enums الحالة موجودة، دوال الانتقال غير موجودة | 0% | Sample.Collect(), Receipt.Issue(), Visit.Close(), OutsourcedSample.Send()... |
| 9 — Business Invariants (INV-01..INV-20) | تعليق توثيقي واحد فقط عن INV-03 | 0% تنفيذ | كل الـ 20 invariant تحتاج كوداً فعلياً موزعاً على الكيانات |
| 10 — مراجعة حقول PDF مقابل الكود | مطبَّق | ~90% | الحقول المُشار إليها كناقصة أُضيفت في الموجتين |

## 4. تحليل الأخطاء المنهجية في التقرير السابق وطريقة تجنّبها

| # | الخطأ المنهجي السابق | التصحيح المتبنَّى هنا |
|:-:|----------------------|----------------------|
| 1 | ترتيب DDD خاطئ — وضع Domain Services (المرحلة 5) قبل Navigation Properties (المرحلة 6) | Navigation Properties + Aggregate Roots تسبق Domain Services؛ لأن PricingService يحتاج PatientVisit.VisitTests كـ ICollection، لا كـ IDs |
| 2 | إغفال EF Core Configurations | تُخصَّص مرحلة كاملة لتوثيق IEntityTypeConfiguration، Cascade/Restrict rules، Owned Types للـ VOs، و Unique Constraints (خاصة Receipt.PatientVisitId) — حتى لو التنفيذ الفعلي في Infrastructure |
| 3 | تجميع Invariants دفعة واحدة في مرحلة واحدة | كل Invariant مربوطة بكيانها ومندمجة داخل مرحلة تعديل ذلك الكيان — التنفيذ تدريجي وقابل للبناء |
| 4 | G-21 صُنِّفت "وهمية" | تُصنَّف "مغلقة" — لأن Patient.NationalId : string? يحقق المتطلب |
| 5 | اقتراح إنشاء Value Objects من جديد | VOs موجودة سلفاً (Age, DateRange, EgyptianPhone) — الخطة الحالية ربط فقط لا إعادة إنشاء |
| 6 | إهمال Circular Dependencies | تحليل صريح: PatientVisit ⇄ VisitTest ⇄ TestResult علاقة أحادية الاتجاه من الأب للابن فقط. Culture ⇄ Sensitivity أحادية من Aggregate Root |
| 7 | إهمال التأثير على باقي الطبقات | جدول مخاطر مخصَّص للـ DTOs, Mappers, Serialization, Reports, UI |
| 8 | ذكر تقديرات زمنية بالساعات | ممنوع كلياً — كل مرحلة معرَّفة بـ Checkpoint قابل للقياس، لا بالوقت |
| 9 | إحصائيات غير دقيقة — التقرير السابق ذكر 16 enum | العدد الفعلي 18 enum (تم عده بأمر find على Common/Enums/*.cs) |

## 5. القرار الاستراتيجي

النهج المعتمد: نهج هجين طبقي تصاعدي (Layered Ascending Hybrid)

كل مرحلة تُغلق فجوة أو أكثر من Gaps-after-phase10.md وتنفّذ في الوقت نفسه قسماً أو أكثر من Domaim1-10.md، وترتيب المراحل يحترم القاعدة الحتمية في DDD:

الأنواع الآمنة → القيم المرتبطة → البنية التجميعية (Navigation) → الإعدادات (EF) → السلوك (Invariants) → التزامن (Events/State) → التنسيق (Services).

تبرير الاختيار (3 أسطر)

- إغلاق الفجوات المتبقية (G-01, G-10, G-11, G-12) لا يقف بذاته: كل فجوة منها تنتمي لقسم أعمق في Domaim1-10.md وتُغلَق كأثر جانبي عند تنفيذ ذلك القسم بشكل صحيح.
- البنية السلوكية (Events/Services) تتطلب Navigation Properties: لا يمكن كتابة PricingService.CalculateTotal(visit) بينما visit.VisitTests غير موجودة.
- الترتيب هرمي تراكمي: كل مرحلة تُنتج كوداً قابلاً للبناء (Build-Green) دون كسر ما سبق، ودون فرض إعادة كتابة على مرحلة سابقة.

## 6. خارطة العمل التنفيذية

قاعدة عامة لكل مرحلة: التنفيذ ينتهي بـ dotnet build أخضر + عدم كسر أي فحص سابق. لا Migration فعلية في أي مرحلة. Migration الحقيقية تُوثَّق فقط وتُنفَّذ في مرحلة Infrastructure اللاحقة.

### 🟩 المرحلة 0 — التأسيس البنيوي للأحداث والإلغاء المرن

الهدف: إعداد أساس السلوك بدون أي كسر — إنشاء واجهة الأحداث وحاويتها داخل BaseEntity، بدون إطلاق أي حدث بعد. هذه المرحلة صفرية الأثر عملياً لكنها تفتح الباب للمراحل التالية.

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | لا شيء (تمكين) |
| أقسام Domain1-10 | تحضير القسم 6 |
| الملفات المتأثرة | إضافة Domain/Events/IDomainEvent.cs · تعديل Common/BaseEntity.cs لإضافة IReadOnlyCollection DomainEvents + AddDomainEvent() + ClearDomainEvents() |
| الاعتماديات | لا شيء |
| Checkpoint | Build أخضر · BaseEntity يعرِّف قائمة أحداث محمية · لا يوجد إطلاق فعلي بعد |

### 🟩 المرحلة 1 — إصلاح أنواع البيانات (Primitive Obsession Elimination)

الهدف: استبدال int/string الحرة بأنواع الدومين الآمنة (Enums الموجودة).

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | G-10 (Permission types) |
| أقسام Domain1-10 | القسم 3 (Data Types) |
| الكيانات المتأثرة | Permission.cs: ScreenId → ScreenType, OperationId → PermissionOperation |
| ملفات مصاحبة | (لا شيء داخل الدومين — تأثيرات UI تُوثَّق كمخاطر) |
| Circular Dependencies | لا شيء — Enums قيم |
| الاعتماديات | المرحلة 0 |
| Checkpoint | Build أخضر · Permission.ScreenId : ScreenType · Permission.OperationId : PermissionOperation · قيم الـ enum ScreenType تبدأ من 1 (متوافقة مع القيم القديمة) |

### 🟩 المرحلة 2 — توصيل Value Objects بالكيانات

الهدف: ربط الـ VOs الموجودة (Age, DateRange, EgyptianPhone) بالكيانات — دون إعادة إنشائها.

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | G-12 |
| أقسام Domain1-10 | القسم 5 (Value Objects) بالكامل |
| ربط Age | Patient.cs — إزالة AgeYears/AgeMonths/AgeDays واستبدالها بـ Age Age { get; set; } |
| ربط EgyptianPhone | Patient.Phone, Doctor.Phone, ReferralEntity.Phone, ReferralEntity.ContactPhone — يبقى string داخلياً ليقبل null، ويُضاف تحقق عبر factory method عند التعيين |
| ربط DateRange | AttendanceLog — استبدال LoginTime + LogoutTime بـ DateRange WorkPeriod. Account.PeriodStart + PeriodEnd بـ DateRange Period. WorkSheet.PeriodStart + PeriodEnd بـ DateRange Period |
| ملاحظة EF Core | كل VO سيُطبَّق كـ Owned Type في IEntityTypeConfiguration (يُوثَّق في المرحلة 4) — لا تغيير في هيكل الجدول |
| Circular Dependencies | لا شيء — VOs جذرية |
| Invariants المُدمَجة (INV-07, INV-08) | Age.Years/Months/Days ≥ 0 && !(كلها صفر) تُنقل من التقرير إلى Constructor Guard في Age · DateRange.Start ≤ DateRange.End guard داخل constructor DateRange |
| الاعتماديات | المرحلة 1 |
| Checkpoint | Build أخضر · لا يظهر AgeYears في Patient.cs · تمرير قيمة عمر سالبة أو DateRange عكسي يرمي ArgumentException |

### 🟩 المرحلة 3 — Navigation Properties + Aggregate Roots

الهدف: بناء البنية التجميعية الفعلية داخل Aggregates — قبل بناء الخدمات، حتى تستطيع الخدمات لاحقاً استدعاء visit.VisitTests مباشرة.

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | لا شيء مباشرة (بنية) |
| أقسام Domain1-10 | القسم 2 (Relationships) والقسم 4 (Aggregate Roots) |

#### 3.1 خريطة الـ Aggregates النهائية

| Aggregate Root | Child Entities (Navigation) | اتجاه العلاقة | تبرير |
|----------------|-----------------------------|:-------------:|-------|
| Patient | لا يضم Visits (يبقى مرجعياً) | أحادي إلى الخارج | Visit كيان مستقل مالياً |
| PatientVisit | ICollection · ICollection · ICollection · Receipt (1:1) | من الأب للأبناء فقط | Aggregate الجوهر لدورة الحياة |
| VisitTest | TestResult? (1:1 في 99% من الحالات، مصمَّمة كـ 1:1 داخل الـ Aggregate) | من الأب للابن | Result جزء لا يتجزأ من VisitTest |
| Test | ICollection · ICollection | من الأب للأبناء | Test هو Master Data Root |
| TestGroup | ICollection | من الأب للأبناء | مجموعة قابلة للتوسيع |
| PriceList | ICollection | من الأب للأبناء | القائمة تجمع بنودها |
| Culture | ICollection | من الأب للأبناء | Sensitivity لا معنى لها بدون Culture |
| Receipt | ICollection ✅ موجودة سلفاً | من الأب للأبناء | مطبَّقة بالفعل |
| Account | (يبقى مرجعياً — لا يضم CashTransactions مباشرة لأن الاستعلام دائماً بـ Repository) | — | Aggregate صغير |
| User | لا Navigation (استعلام دائم عبر Repository) | — | تجنّب تحميل كامل الصلاحيات |

#### 3.2 قرارات صريحة حول Circular Dependencies

- PatientVisit → VisitTest → TestResult — الاتجاه أحادي فقط من الأعلى للأسفل. لا VisitTest.PatientVisit عكسية، ولا TestResult.VisitTest عكسية. الوصول من الأسفل يتم عبر Repositories لا Navigation.
- Culture → Sensitivity — أحادي من Culture لـ Sensitivity.
- PriceList → PriceListItem — أحادي.
- Test ↔ VisitTest — لا Navigation إطلاقاً في أي من الاتجاهين (Test مرجعي).
- ReferralEntity ↔ PriceList — يُحوَّل ReferralEntity.PriceListId إلى int? (nullable) لدعم القائمة الافتراضية، بدون Navigation عكسية.

| البُعد | التفاصيل |
|-------|----------|
| الملفات المُعدَّلة | PatientVisit.cs, VisitTest.cs, Test.cs, TestGroup.cs, PriceList.cs, Culture.cs, ReferralEntity.cs (تعديل PriceListId إلى nullable) |
| الاعتماديات | المرحلة 2 |
| Checkpoint | Build أخضر · PatientVisit يعرض VisitTests, Samples, OutsourcedSamples كـ ICollection · لا Navigation عكسية · ReferralEntity.PriceListId أصبح int? |

### 🟩 المرحلة 4 — توثيق EF Core Configurations (مستهدف Infrastructure)

الهدف: إنتاج وثيقة تصميم لكل IEntityTypeConfiguration سيتم تنفيذها في Infrastructure لاحقاً — بدون كتابتها الآن. هذه المرحلة تحمي التصميم الحالي من انفجار Migration المستقبلي.

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | لا شيء مباشرة (توثيقية) |
| أقسام Domain1-10 | ملحقات القسم 2 (Relationships) |
| Cascade Rules — القرارات النهائية | PatientVisit → VisitTest Cascade · VisitTest → TestResult Cascade · PatientVisit → Sample Cascade · PatientVisit → OutsourcedSample Restrict (سجل مالي) · PatientVisit → Receipt Restrict · Culture → Sensitivity Cascade · Test → ReferenceValue/Comment Cascade · PriceList → PriceListItem Cascade · Patient → PatientVisit Restrict + Soft Delete · User → AuditLog/AttendanceLog Restrict |
| Owned Types | Patient.Age (Age) · AttendanceLog.WorkPeriod (DateRange) · Account.Period (DateRange) · WorkSheet.Period (DateRange) — تُخزَّن أعمدتها متضمَّنة داخل جدول الأب |
| Unique Constraints | Receipt.PatientVisitId UNIQUE (1:1) · Patient.LabId UNIQUE · PatientVisit.LabId UNIQUE · User.Username UNIQUE · (TestGroupItem.TestGroupId, TestGroupItem.TestId) composite UNIQUE · (PriceListItem.PriceListId, PriceListItem.TestId) composite UNIQUE · (Permission.UserId, ScreenId, OperationId) composite UNIQUE · (Sensitivity.CultureId, AntibioticId) composite UNIQUE |
| Global Query Filter | IsDeleted == false مطبَّق على كل الكيانات (وارثة ISoftDeletable) |
| Value Converters | Age, DateRange, EgyptianPhone كـ Owned Types (لا Converters) — لكن EgyptianPhone.Value يُخزَّن كعمود string مفرد |
| الاعتماديات | المرحلة 3 |
| Checkpoint | ملف Docs/EFCore-Configuration-Plan.md مكتوب يحتوي القرارات الـ 4 أعلاه · لا تعديلات كود |

### 🟩 المرحلة 5 — Business Invariants الموزَّعة على الكيانات

الهدف: تحويل الـ 20 Invariant من تعليقات توثيقية إلى كود قابل للتنفيذ موزَّع كل واحدة على كيانها لا مجمَّعة.

#### 5.1 خريطة التوزيع

| Invariant | الكيان الحاضن | آلية التنفيذ | يعتمد على Domain Service؟ |
|:---------:|---------------|--------------|:------------------------:|
| INV-06 (CommissionPercent ∈ [0,100]) | Doctor | property setter guard | لا |
| INV-07 (Age ≥ 0) | Age VO (المرحلة 2) | constructor guard | لا |
| INV-08 (DateRange.Start ≤ End) | DateRange VO (المرحلة 2) | constructor guard | لا |
| INV-19 (CashTransaction.Amount > 0) | CashTransaction | property guard | لا |
| INV-20 (Comment.Text length) | Comment, CommentTemplate | property guard | لا |
| INV-04 (VisitTest.Price snapshot) | VisitTest (internal set موجود) | تعزيز التعليق + guard | لا |
| INV-15 (Sensitivity requires Culture) | Sensitivity | constructor guard | لا |
| INV-13 (OutsourcedSample.PatientPrice ≥ CostPrice) | OutsourcedSample | method guard | لا |
| INV-09 (Receipt.Paid ≤ Total) | Receipt | AddPayment() method | لا |
| INV-10 (Discount ≤ Subtotal) | Receipt | ApplyDiscount() method | لا |
| INV-01 (Receipt.Total = Σ VisitTests + Σ Extras − Discount) | Receipt | method داخل Receipt.Recalculate() — يحتاج PatientVisit.VisitTests | ✅ في Service |
| INV-02 (VisitTest.IsOutsourced ⟺ OutsourcedSample موجود) | PatientVisit | method guard على AddOutsourcedSample() | ✅ في Service |
| INV-03 (Visit.DoctorId null → Patient.DoctorId) | PatientVisit | factory method | لا |
| INV-11 (Sample.CollectedAt ⟺ IsCollected) | Sample | Collect()/Revert() methods | لا |
| INV-12 (TestResult ⟹ Sample.IsCollected) | TestResult | تُفحص في Service | ✅ في Service |
| TestResult | تُفحص في Service | ✅ في Service |
| INV-14 (Culture مرتبطة بـ Microbiology) | Culture | يحتاج نوع Test — سيُنفَّذ في مرحلة Services | ✅ في Service |
| INV-16 (ReferralEntity.PriceListId فعّال) | ReferralEntity | يحتاج Repository check | ✅ في Service |
| INV-17 (لا حذف Patient مع Receipt) | Patient | يحتاج Repository check | ✅ في Service |
| INV-18 (VisitTest → Patient واحد) | ضمني عبر FK | يُضمن هيكلياً | لا |
| INV-05 (NetProfit يُعاد حسابه) | Account | لا كود مباشر — يُدار في Service | ✅ في Service |

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | لا شيء مباشرة (سلوك) |
| أقسام Domain1-10 | القسم 9 بالكامل |
| الملفات المتأثرة | Doctor.cs, CashTransaction.cs, Comment.cs, CommentTemplate.cs, Receipt.cs, PatientVisit.cs, Sample.cs, OutsourcedSample.cs, Sensitivity.cs |
| Circular Dependencies | لا — كل Invariant داخل حدود Aggregate |
| الاعتماديات | المرحلة 3 (يحتاج Navigation لبعض الـ INVs) والمرحلة 2 (VOs) |
| Checkpoint | Build أخضر · محاولة تعيين Doctor.CommissionPercent = 150 ترمي BusinessRuleViolationException · محاولة receipt.AddPayment(receipt.Total + 1) ترمي استثناء · 10 Invariants على الأقل مُنفَّذة في هذه المرحلة (الباقية تنتظر Services) |

### 🟩 المرحلة 6 — State Machines + Domain Events

الهدف: تحويل الحالات (SampleStatus, VisitStatus, SettlementStatus) من enums حرة إلى انتقالات محمية داخل الكيانات، وربطها بالأحداث المُنشرة.

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | لا شيء |
| أقسام Domain1-10 | القسم 6 (Events) والقسم 8 (State Machines) |
| الأحداث المُنفَّذة | E-01..E-20 كسجلات (record) في Domain/Events/ — كل حدث ينفذ IDomainEvent |
| State Machines المُنفَّذة داخل الكيانات | Sample.Collect() / Sample.RevertCollection() · OutsourcedSample.Send() / ReceiveResult() / Complete() · PatientVisit.Open() / EnterAllResults() / IssueReceipt() / Close() · Culture.Record() / RecordSensitivity() · Receipt.Issue() / AddPayment() / ApplyDiscount() |
| Guards | كل انتقال محمي بشروط الجدول 8.1..8.5 من Domaim1-10.md |
| إطلاق الأحداث | كل transition ينادي AddDomainEvent(new SampleCollected(...)) — لا مشتركون بعد (يُدار في Infrastructure) |
| Circular Dependencies | Events لا تُشير للكيانات — تحمل IDs فقط، لا مراجع Object |
| الاعتماديات | المرحلة 5 + المرحلة 0 |
| Checkpoint | Build أخضر · استدعاء sample.Collect(userId) وهو بالفعل Collected يرمي استثناء · بعد visit.IssueReceipt() يحوي visit.DomainEvents واحداً من نوع ReceiptIssued · 5 كيانات على الأقل تحتوي state machine كامل |

### 🟩 المرحلة 7 — Domain Services (DS-01..DS-10)

الهدف: إضافة الواجهات فقط (بدون تنفيذ) للخدمات النطاقية العشر، مع مسؤولية واضحة لكل خدمة.

| Service | يحتاج ICollections من المرحلة 3؟ | تنتمي لأي Invariant | صيغة MVP في هذه المرحلة |
|---------|:-------:|-------------------|-------------------------|
| DS-01 PricingService | ✅ | INV-01 | Interface فقط |
| DS-02 AccountingService | ✅ | INV-05 | Interface فقط |
| DS-03 MedicalHistoryService | ✅ | — | Interface + تنفيذ أولي بناءً على IPatientHistoryRepository → يُغلق G-01 |
| DS-04 SampleTrackingService | ✅ | INV-11 | Interface فقط |
| DS-05 OutsourcingService | ✅ | INV-02, INV-13 | Interface فقط |
| DS-06 ReceiptCalculationService | ✅ | INV-01, INV-09, INV-10 | Interface فقط |
| DS-07 ResultValidationService | جزئياً | INV-12 | Interface فقط |
| DS-08 CultureSensitivityService | ✅ | INV-14 | Interface فقط |
| DS-09 ReferralCommissionService | ✅ | — | Interface فقط |
| DS-10 PriceListResolverService | ✅ | INV-16 | Interface فقط |

| البُعد | التفاصيل |
|-------|----------|
| الفجوات المُغلَقة | G-01 (جزئياً) — عبر MedicalHistoryService |
| الفجوات المؤجَّلة الآن تُغلق | G-11 — Account.NetProfit يصبح internal set مع IAccountingService.RecalculateNetProfit() |
| أقسام Domain1-10 | القسم 7 |
| الملفات المضافة | Domain/Services/IPricingService.cs, IAccountingService.cs, IMedicalHistoryService.cs, ...إلخ (10 ملفات) + إضافة IPatientHistoryRepository.cs في Interfaces/ |
| الاعتماديات | المرحلة 6 |
| Checkpoint | Build أخضر · 10 واجهات Domain Services موجودة · IMedicalHistoryService.BuildHistory(patientId) واجهة مكتوبة · Account.NetProfit أصبح internal set |

### 🟩 المرحلة 8 — التكامل والتحقق النهائي

الهدف: التأكد من ثبات الحدود، عدم وجود Circular Dependencies، وأن الدومين يبني بمفرده دون أي مرجع خارجي.

| البُعد | التفاصيل |
|-------|----------|
| فحص Circular Dependencies | تشغيل tool تلقائي على المشروع للتأكد أن MasrLab.Domain لا يعتمد إلا على System.* |
| فحص Aggregate Boundaries | كل كيان child لا يُعدَّل مباشرة — فقط عبر Aggregate Root |
| مراجعة dotnet build | warnings-as-errors مفعّل |
| توثيق Migration المستقبلية | ملف Docs/Future-Migrations.md يصف Schema المتوقع بعد استقرار الدومين — دون تنفيذ |
| ما يبقى مؤجَّلاً بوعي | تنفيذ فعلي لـ Domain Services (يذهب لـ Application) · Event Bus (يذهب لـ Infrastructure) · SQL View لـ PatientHistory (Infrastructure) |
| الاعتماديات | كل المراحل السابقة |
| Checkpoint | dotnet build MasrLab.Domain -warnaserror ناجح · لا حزم إضافية على MasrLab.Domain.csproj (يبقى net8.0 + ImplicitUsings) · وثيقتان مكتوبتان: EFCore-Configuration-Plan.md, Future-Migrations.md |

## 7. معايير القبول النهائية

طبقة الدومين تُعتبر مكتملة عندما تتحقق جميع المعايير الست:

### 7.1 السلامة الهيكلية (Structural)

- ✅ كل حقل حالة يستخدم Enum لا int (Permission.ScreenId/OperationId تحديداً)
- ✅ كل حقل عمر/هاتف/فترة زمنية يستخدم Value Object الملائم
- ✅ حدود Aggregates واضحة — كل Root يحتوي Children كـ ICollection
- ✅ لا Navigation عكسية (لا VisitTest.PatientVisit)

### 7.2 السلامة السلوكية (Behavioral)

- ✅ 20 Invariant مُنفَّذة ككود قابل للتنفيذ (كل واحدة في كيانها الطبيعي)
- ✅ محاولات خرق Invariants ترمي BusinessRuleViolationException أو ArgumentException بحسب النوع
- ✅ Setters المكشوفة على حقول الحالة الحسّاسة أصبحت internal set أو مُخفية خلف methods

### 7.3 السلامة الحدثية (Domain Events)

- ✅ BaseEntity يحتوي DomainEvents collection + methods الإدارة
- ✅ كل الأحداث الـ 20 (E-01..E-20) موجودة كـ record تنفّذ IDomainEvent
- ✅ كل تحويل حالة جوهري يُطلق حدثه

### 7.4 السلامة الخدمية (Services)

- ✅ 10 Interfaces واضحة تحت Domain/Services/
- ✅ IMedicalHistoryService + IPatientHistoryRepository موجودان لدعم G-01
- ✅ كل Service مصمَّم للعمل على Aggregate Root، لا على IDs

### 7.5 السلامة المعمارية (Architectural)

- ✅ لا Circular Dependencies بين الكيانات
- ✅ MasrLab.Domain.csproj لا يعتمد على Application/Infrastructure
- ✅ لا NotImplementedException في أي ملف داخل MasrLab.Domain

### 7.6 السلامة التقنية (Technical)

- ✅ dotnet build MasrLab.Domain -warnaserror ناجح 100%
- ✅ لا تعليق يشير إلى "مؤجَّل" (كل تأجيل موثَّق في Docs/Future-Migrations.md)
- ✅ Docs المُوثَّقة (EFCore-Configuration-Plan.md, Future-Migrations.md) متسقة مع الكود

## 8. المخاطر والتوصيات

### 8.1 جدول المخاطر الإلزامي

| # | المخطر | التأثير | الطبقة المتأثرة | المعالجة |
|:-:|--------|:-------:|:---------------:|----------|
| R-01 | ReceiptDto لا يحتوي ChangeDue, RefundToPatient, ExtraServiceItems | AutoMapper سيفشل عند التنفيذ الفعلي | Application | إضافة الحقول الثلاثة + ExtraServiceItemDto كنوع مضمَّن — في مرحلة Application اللاحقة |
| R-02 | Migration ستُغيّر Schema بشكل كبير بعد تحويل Permission.ScreenId إلى enum و Patient.Age* إلى Owned Type | كسر أي بيانات موجودة | Infrastructure | تأجيل أول Migration حقيقية حتى انتهاء المرحلة 6 على الأقل — ثم EnsureCreated جديد لأن لا بيانات حقيقية بعد |
| R-03 | PatientHistory Report يعتمد على View SQL غير موجود بعد | تقرير التاريخ المرضي معطَّل | Infrastructure + Reports | إنشاء View في Infrastructure Migration + ربطه بـ IPatientHistoryRepository (Domain جاهز لهذا) |
| R-04 | Serialization مع Value Objects — EgyptianPhone كـ record قد يتسبب بمشاكل عند التحويل لـ JSON | Reports / DTO Layer | Application + Infrastructure | AutoMapper Config: EgyptianPhone → string و string → EgyptianPhone factory · System.Text.Json converter مخصص |
| R-05 | Circular Dependency خفية في PatientVisit → VisitTest → TestResult | ممكن حدوث EF navigation loops | Domain (مُتوقَّى بالتصميم أحادي الاتجاه) | القرار المتَّخذ في المرحلة 3 يمنع هذا كلياً — Navigation من الأب للابن فقط |
| R-06 | تغيير Permission.ScreenId من int إلى enum يكسر أي كود UI حالي يستعلم بأرقام صريحة | UI / Application | UI Layer | القيم في ScreenType مرقَّمة صراحةً من 1..13 لتوافق الأرقام القديمة — لكن يبقى بحث/استبدال في XAML/Converters لازم |
| R-07 | Owned Types للـ VOs قد تسبب تغيير أسماء أعمدة (Age_Years, Age_Months) | Infrastructure Migration | Infrastructure | في IEntityTypeConfiguration نستخدم HasColumnName صريحاً للحفاظ على أسماء الأعمدة القديمة (AgeYears, AgeMonths, AgeDays) |
| R-08 | NetProfit المخزَّن قد يخالف قيمة الحساب المباشر | Reports (تقارير الحسابات) | Application (AccountingService) | internal set + إعادة الحساب في كل CashTransaction/Receipt Save (INV-05) |
| R-09 | الـ 20 Domain Event لا يُستهلكها أحد بعد | لا أثر مباشر — لكن استهلاك مستقبل قد يفتقد أحداثاً تاريخية | Infrastructure (Event Bus) | تأجيل Event Bus لـ Infrastructure؛ لا خسارة بيانات لأن الأحداث تُبثّ عند SaveChanges لاحقاً فقط |
| R-10 | Age VO لا يقبل null بينما بعض المرضى قد لا يعرفون عمرهم | UI + Application | Application/UI | حل: إما Age? (nullable VO) أو إجبار على قيمة افتراضية new Age(0,0,0) — القرار يبقى في Application |

### 8.2 توصيات المراحل التالية

#### 🔷 Application Layer (المرحلة التالية مباشرة)

- تحديث ReceiptDto ليشمل ChangeDue, RefundToPatient, ICollection
- تنفيذ Domain Services العشرة (PricingService, AccountingService, ...) — التنفيذ الفعلي، لا الواجهات
- إزالة جميع NotImplementedException في Handlers
- AutoMapper Profiles لكل Value Object (Age, DateRange, EgyptianPhone) لتفادي مشاكل Serialization
- ربط MediatR (أو مشابه) لتوزيع Domain Events كـ Integration Events

#### 🔷 Infrastructure Layer

- كتابة IEntityTypeConfiguration لكل كيان وفق Docs/EFCore-Configuration-Plan.md المُنتَج في المرحلة 4
- إنشاء DbContext مع IsDeleted == false Global Query Filter
- إنشاء SQL View لـ PatientHistory + تنفيذ PatientHistoryRepository عليها
- Event Dispatcher في SaveChangesInterceptor (نمط EF Core 8 المفضَّل — يتوافق مع تفضيلات هذا المشروع)
- الآن فقط: تشغيل أول Migration حقيقية بعد استقرار Domain عند نهاية المرحلة 8

#### 🔷 UI Layer (WPF/MVVM)

- استبدال int ScreenId في XAML/Converters بـ ScreenType enum
- ربط ViewModels بـ Application Services (وليس Entities مباشرة)
- عرض BusinessRuleViolationException.Message كـ Validation Message في IErrorInfo/INotifyDataErrorInfo
- تفعيل State Transition Buttons ديناميكياً بناءً على الحالة الحالية (زر "إصدار إيصال" يظهر فقط لو Visit.CanIssueReceipt())

## 9. جدول توقيت Migration (توضيح صريح)

| النقطة الزمنية | Migration؟ | السبب |
|----------------|:----------:|-------|
| بعد المرحلة 0..3 | ❌ لا | البنية لا تزال تتغير |
| بعد المرحلة 4 | ❌ لا | الوثيقة جاهزة لكن Configurations لم تُكتب في Infrastructure بعد |
| بعد المرحلة 5..7 | ❌ لا | الدومين يستقر لكن كتابة Configurations نفسها في Infrastructure |
| بعد المرحلة 8 (نهاية طبقة الدومين) | ⚠️ توثيق فقط | كتابة Future-Migrations.md مع Schema المتوقع |
| بعد كتابة EF Configurations في Infrastructure | ✅ أول Migration حقيقية | dotnet ef migrations add InitialCreate — تنشئ Schema كاملاً من الصفر |

الجداول والأعمدة المتأثرة عند أول Migration ستشمل:

- Patients — إعادة تسمية أعمدة العمر لتناسب Owned Type
- Permissions — تحويل ScreenId, OperationId إلى tinyint وربطها بـ Check Constraint
- Accounts / AttendanceLogs / WorkSheets — تجميع فترات التاريخ في Owned DateRange
- إضافة Unique Indexes الجديدة (Receipt.PatientVisitId, TestGroupItem composite, ...)
- إضافة Sequence Numbers لأعمدة LabId (تحسين مستقبلي)

## 10. خاتمة

خارطة العمل أعلاه مبنية بالكامل على الكود الفعلي عند 613838d، وتحترم القاعدة الجوهرية في DDD: البنية قبل السلوك، والسلوك قبل التنسيق. النتيجة النهائية — بعد المرحلة 8 — طبقة دومين غنية بالسلوك (Rich Domain Model)، مغلقة الحدود، خالية من الاعتماديات الخارجية، جاهزة لأن تُبنى فوقها Application و Infrastructure بثقة، ومحتوية على كل الأدوات اللازمة (Events, Services, Invariants, State Machines) لتنفيذ متطلبات RealLab كاملة كما ورد في PDF و Spec.