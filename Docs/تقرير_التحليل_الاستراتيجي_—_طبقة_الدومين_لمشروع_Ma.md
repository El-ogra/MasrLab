# تقرير التحليل الاستراتيجي — طبقة الدومين لمشروع MasrLab

> **ملاحظة:** هذا التقرير مبني بالكامل على فحص الكود الفعلي عند الكوميت `613838de9e581941b10b1f1991c49aa67b883a25` على الفرع `niamod` من مستودع [MasrLab](https://github.com/El-ogra/MasrLab.git). لم يتم تعديل أي ملف أو تنفيذ أي أمر Git.

---

## 1. ملخص حالة الكود عند الكوميت المستهدف

تم استنساخ المستودع والانتقال إلى الفرع `niamod` والتحقق من الكوميت `613838d` بعنوان "تحليل الدومين". هذا الكوميت هو أحدث كوميت في الفرع ويتبع مباشرة كوميتي "الموجة الثانية" و"الموجة الأولى" اللذين أضافا إصلاحات كثيرة لطبقة الدومين.

الجدول التالي يلخص الإحصائيات الفعلية لطبقة الدومين:

| المكون | العدد | الملاحظة |
|--------|-------|----------|
| الكيانات (Entities) | 35 | موزعة على 5 مجلدات: Core (12), Administrative (7), Culture (4), Financial (6), Settings (6) |
| قوائم التعداد (Enums) | 16 | تشمل Gender, SampleStatus, VisitStatus, SettlementStatus, ReferenceValueGender, وغيرها |
| الكائنات القيمية (Value Objects) | 3 | Age, DateRange, EgyptianPhone — مكتوبة كـ C# records |
| الواجهات (Interfaces) | 9 | IRepository<T> عام + 8 واجهات متخصصة |
| الاستثناءات (Exceptions) | 4 | BusinessRuleViolation, DuplicateLabId, EntityNotFound, InsufficientPermission |
| نماذج القراءة (DTOs/Records) | 2 | PatientHistoryEntry, StatisticsDto |
| مجلدات فرعية داخل Domain | 8 | Entities, Common, Interfaces, Exceptions, ValueObjects, Common/DTOs, Common/Enums |

**الملاحظة الجوهرية:** طبقة الدومين تفتقر بالكامل إلى الأحداث النطاقية (Domain Events)، والخدمات النطاقية (Domain Services)، وآلات الحالة (State Machines)، والثوابت التجارية (Business Invariants). كما أن الكيانات لا تحتوي على خصائص التنقل (Navigation Properties) — بل تعتمد بالكامل على مفاتيح الأجانب (Foreign Keys). هذا يعني أن الكود الحالي يمثل نموذج بيانات (Data Model) وليس نموذج دومين (Domain Model) بالمعنى الحقيقي لـ DDD.

---

## 2. نتائج التحقق من الفجوات (23 فجوة)

تم فحص كل فجوة من الـ 23 فجوة المذكورة في ملف `Gaps-after-phase10.md` ومقارنتها بالكود الفعلي عند الكوميت `613838d`:

| رقم الفجوة | الوصف | الحالة | التبرير التفصيلي |
|-----------|--------|--------|------------------|
| G-01 | PatientHistory (Read Model + View + Service) | **مُغلقة جزئياً** | ملف `PatientHistoryEntry.cs` موجود كـ record في `Common/DTOs/` يحتوي على 14 حقلاً. لكن لا يوجد SQL View ولا Repository مخصص ولا Domain Service. الجذور الناقصة هي Infrastructure + Repository + Service فقط. |
| G-02 | Gender.Both / ReferenceValueGender | **مُغلقة** | enum `ReferenceValueGender` موجود بـ 3 قيم: Male, Female, Both. كيان `ReferenceValue` يستخدم هذا النوع. |
| G-03 | Sample.CollectionStatus | **مُغلقة** | `Sample.CollectionStatus` من نوع `SampleStatus` (enum بـ Collected, NotCollected). |
| G-04 | SettlementStatus enum | **مُغلقة** | enum `SettlementStatus` موجود بـ 3 قيم. `OutsourcedSample.SettlementStatus` يستخدمه. |
| G-05 | PatientVisit.DoctorId? + ReferralEntityId? | **مُغلقة** | كلا الحقلين موجودان كـ `int?` في `PatientVisit.cs`. |
| G-06 | Patient.DoctorId? + ReferralEntityId? | **مُغلقة** | كلا الحقلين موجودان كـ `int?` في `Patient.cs`. |
| G-07 | VisitTest: حذف ExternalLabId, CostPrice, Notes | **مُغلقة** | `VisitTest` يحتوي فقط على PatientVisitId, TestId, Price, IsOutsourced. |
| G-08 | TestGroup: حذف TestIds string | **مُغلقة** | `TestGroup` يحتوي فقط على GroupName و GroupPrice. الاعتماد على `TestGroupItem` فقط. |
| G-09 | CashTransaction.EntityId → AccountId | **مُغلقة** | `CashTransaction.AccountId` من نوع `int` (موجود). |
| G-10 | Permission.ScreenId/OperationId → enum | **مؤكدة — تتطلب إجراء** | enums `ScreenType` (13 قيمة) و `PermissionOperation` (6 قيم) موجودة. لكن `Permission.cs` لا يزال يستخدم `int ScreenId` و `int OperationId`. |
| G-11 | Account.NetProfit مخزّن | **مؤكدة — تتطلب إجراء** | `NetProfit` حقل `decimal` مخزّن. التعليقات توضح أنه يجب حسابه عبر `AccountingService` لكنه يبقى مخزّناً. |
| G-12 | VOs غير موصلة بالكيانات | **مؤكدة — تتطلب إجراء** | `Age.cs`, `DateRange.cs`, `EgyptianPhone.cs` موجودة كـ records. لكن `Patient.cs` لا يزال يستخدم `AgeYears`, `AgeMonths`, `AgeDays` كـ int منفصلة. |
| G-13 | حقول التاريخ المرضي (12 حقل) | **مُغلقة** | `Patient` يحتوي على 8 حقول بوليانية + `ChronicDiseases` + `Pregnancy` + `BloodThinning` + `DrugAllergy`. |
| G-14 | ExtraServiceItem entity + Receipt collection | **مُغلقة** | `ExtraServiceItem` موجود. `Receipt.ExtraServiceItems` من نوع `ICollection<ExtraServiceItem>`. |
| G-15 | VisitTest.Price → internal set | **مُغلقة** | `Price { get; internal set; }` مع تعليق Snapshot. |
| G-16 | CardSetting entity (10 حقول) | **مُغلقة** | الكيان موجود بجميع حقوله: CardTitle + 7 bools + HeaderColor + FontSize. |
| G-17 | OutsourcedSample.ReceivedAt | **مُغلقة** | `DateTime? ReceivedAt` موجود. |
| G-18 | Sample.CollectedByUserId | **مُغلقة** | `int? CollectedByUserId` موجود. |
| G-19 | CommentTemplate entity | **مُغلقة** | الكيان موجود بـ `TestId` و `Text`. |
| G-20 | ReferralEntity.ContactPhone | **مُغلقة** | `ContactPhone` و `ContactPerson` موجودان. |
| G-21 | Patient.NationalId | **وهمية** | الحقل كان موجوداً مسبقاً كـ `string?`. |
| G-22 | Receipt.ChangeDue + RefundToPatient | **مُغلقة** | كلا الحقلين موجودان في `Receipt.cs`. |
| G-23 | ExternalLab entity | **مُغلقة** | الكيان موجود بـ Name, Address, Phone, ContactPerson. |

### ملخص التصنيف

| التصنيف | العدد | الفجوات |
|---------|-------|---------|
| مُغلقة بالكامل | 19 | G-02, G-03, G-04, G-05, G-06, G-07, G-08, G-09, G-13, G-14, G-15, G-16, G-17, G-18, G-19, G-20, G-22, G-23 + G-21 (وهمية) |
| مُغلقة جزئياً | 1 | G-01 (Read Model موجود، باقي المكونات مفقودة) |
| مؤكدة — تتطلب إجراء | 3 | G-10, G-11, G-12 |

---

## 3. نتائج التحقق من الأقسام العشرة

### القسم 1: سد الفجوات في الكيانات

| النقطة | الحالة | الملاحظة |
|--------|--------|----------|
| 1.1 PatientHistory | مطبق جزئياً | `PatientHistoryEntry` موجود كـ record. لا يوجد View ولا Repository ولا Service. |
| 1.2 Doctor.DiscountPercent | مطبق | الكود يستخدم `CommissionPercent` فقط — القرار صحيح. |
| 1.3 Gender.Both | مطبق | تم استخدام `ReferenceValueGender` منفصل (التوصية الصحيحة). |
| 1.4 Sample.CollectionStatus | مطبق | تم تحويله إلى enum. |
| 1.5 OutsourcedSample.SettlementStatus | مطبق | تم تحويله إلى enum. |

**التقييم:** 4/5 مطبق بالكامل، 1 مطبق جزئياً.

### القسم 2: العلاقات (Relationships)

| العلاقة | الحالة | الملاحظة |
|---------|--------|----------|
| R-01 Patient → PatientVisit | مطبق | FK موجود. |
| R-02 PatientHistory View | غير مطبق | لا يوجد View. |
| R-03 إلى R-06c (Visit/Test) | مطبق | جميع FKs موجودة. |
| R-07 Test → ReferenceValue | مطبق | FK موجود. |
| R-08 Test → Comment | مطبق | FK موجود. |
| R-09 Test ↔ TestGroup | مطبق | `TestIds` حُذف، الاعتماد على `TestGroupItem` فقط. |
| R-10 إلى R-11 (PriceList) | مطبق | FKs موجودة. |
| R-12 PriceList → ReferralEntity | مطبق جزئياً | `PriceListId` موجود لكن `NOT NULL` (لم يُغيّر إلى nullable). |
| R-13 إلى R-17 | مطبق | جميع FKs الموجودة (DoctorId, ReferralEntityId) nullable. |
| R-18 Culture → Organism | مطبق | نهج هجين (string + entity). |
| R-19 Culture ↔ Antibiotic | مطبق | `Sensitivity` كيان وسيط موجود. |
| R-20 إلى R-27 | مطبق | جميع العلاقات موجودة. |

**ملاحظة حرجة:** لا توجد `ICollection` navigation properties في أي كيان. الوثيقة توصي بإضافتها لعدة علاقات (R-06a, R-06c, R-07, R-11, R-19) لتفعيل التجميع في مستوى الدومين.

**التقييم:** 25/27 مطبق بالكامل، 1 مطبق جزئياً، 1 غير مطبق.

### القسم 3: أنواع البيانات الصحيحة

| النقطة | الحالة | الملاحظة |
|--------|--------|----------|
| 3.1 Test.TurnaroundTime | غير مطبق | لا يزال `string`. |
| 3.2 Permission.ScreenId/OperationId | غير مطبق | لا يزالان `int` رغم وجود الـ enums. |
| 3.3 AttendanceLog.BreakPeriods | غير مطبق | لا يزال `string`. |
| 3.4 Receipt.Currency | مطبق | `string Currency = "EGP"` صحيح. |
| 3.5 Account.NetProfit | غير مطبق | لا يزال `decimal` مخزّناً. |
| 3.6 Doctor.CommissionPercent | مطبق جزئياً | الحقل موجود لكن بدون validation لـ [0, 100]. |

**التقييم:** 1/6 مطبق بالكامل، 1 مطبق جزئياً، 4 غير مطبق.

### القسم 4: Aggregate Roots

**التقييم:** مطبق جزئياً. هيكل المستودعات يتوافق مع الجذور المقترحة (17 جذر)، لكن لا يوجد أي تجميع فعلي للكيانات التابعة (Child Entities) كـ Collections داخل الكيانات الأم.

### القسم 5: Value Objects

| الكائن القيمي | موجود في الكود | موصّل بكيان |
|---------------|----------------|-------------|
| Age | نعم | لا — Patient لا يزال يستخدم int منفصلة |
| DateRange | نعم | لا — لم يُستخدم في أي كيان |
| EgyptianPhone | نعم | لا — Phone لا يزال string |

**التقييم:** غير مطبق (0/3 موصّل).

### القسم 6: Domain Events

**التقييم:** غير مطبق. لا يوجد أي ملف أو واجهة أو آلية للأحداث النطاقية. الوثيقة تقترح 20 حدثاً (E-01 إلى E-20).

### القسم 7: Domain Services

**التقييم:** غير مطبق. لا توجد أي خدمات نطاقية. الوثيقة تقترح 10 خدمات (DS-01 إلى DS-10).

### القسم 8: State Machines

**التقييم:** غير مطبق. الـ Enums للحالات موجودة (SampleStatus, SettlementStatus, VisitStatus) لكن لا توجد دوال انتقالية أو شروط حماية (Guards).

### القسم 9: Business Invariants

**التقييم:** غير مطبق. توجد تعليقات توثيقية (مثل `INV-03` في PatientVisit) لكن لا يوجد أي كود لفرض هذه القواعد.

### القسم 10: مراجعة حقول PDF مقابل الكود

**التقييم:** مطبق بشكل كبير. الغالبية العظمى من الحقول التي كانت "مفقودة" في الوثيقة (التي كُتبت ضد الكوميت `9b6fda5`) تم إضافتها في الموجتين السابقتين.

### ملخص الأقسام العشرة

| القسم | التقييم | نسبة التطبيق |
|-------|---------|--------------|
| 1 — Entity Gaps | مطبق جزئياً | 80% |
| 2 — Relationships | مطبق جزئياً | 90% |
| 3 — Data Types | غير مطبق | 17% |
| 4 — Aggregate Roots | مطبق جزئياً | 50% |
| 5 — Value Objects | غير مطبق | 0% (موجود لكن غير موصّل) |
| 6 — Domain Events | غير مطبق | 0% |
| 7 — Domain Services | غير مطبق | 0% |
| 8 — State Machines | غير مطبق | 0% |
| 9 — Invariants | غير مطبق | 0% |
| 10 — PDF Review | مطبق | 90% |

---

## 4. القرار الاستراتيجي

**النهج المختار: نهج هجين — تطبيق الأقسام العشرة بالترتيب مع إغلاق الفجوات ضمن نطاق كل قسم.**

**التبرير المنطقي:**

الكود الحالي يمثل نموذج بيانات (Data Model) وليس نموذج دومين (Domain Model) بالمعنى الحقيقي لـ DDD. الفجوات الثلاث المتبقية (G-10, G-11, G-12) تتوافق بشكل مباشر مع الأقسام 3 و 5 و 9 من الوثيقة المرجعية، مما يعني أن تنفيذ هذه الأقسام سينتج عنه بطبيعة الحال إغلاق هذه الفجوات. النهج الهجين هو الأنسب لأن الأقسام العشرة تمثل طبقات بناء منطقية متصاعدة: لا يمكن بناء State Machines (القسم 8) دون وجود Value Objects (القسم 5) وثوابت تجارية (القسم 9)، ولا يمكن بناء Domain Services (القسم 7) دون أن تكون الكيانات نفسها تمتلك السلوكيات اللازمة. البدء بالفجوات المتبقية كخطوة منفصلة سيكون تكراراً لعمل سيتم تنفيذه ضمن الأقسام.

---

## 5. خارطة العمل (The Roadmap)

### المرحلة 1: إصلاح أنواع البيانات وتفعيل الأنواع الآمنة

**الوصف:** تحويل الحقول التي تستخدم `int` أو `string` إلى أنواعها الصحيحة (Enums, Value Objects, Constraints).

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | G-10 (Permission types) |
| الأقسام المنفذة | القسم 3 (Data Types) |
| الملفات المتأثرة | `Permission.cs`, `AttendanceLog.cs`, `Test.cs` |
| الاعتماديات | لا يوجد |
| التقدير الزمني | 3-4 ساعات |
| نقطة التحقق | Build ناجح + `Permission.ScreenId` أصبح من نوع `ScreenType` + `Permission.OperationId` أصبح من نوع `PermissionOperation` |

### المرحلة 2: توصيل الكائنات القيمية بالكيانات

**الوصف:** استبدال الحقول الأولية (Primitive Fields) بالكائنات القيمية (Value Objects) الموجودة مسبقاً في الكود.

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | G-12 (VOs wiring) |
| الأقسام المنفذة | القسم 5 (Value Objects) |
| الملفات المتأثرة | `Patient.cs` (Age), `AttendanceLog.cs` (DateRange), `Patient.cs`/`Doctor.cs`/`ReferralEntity.cs` (EgyptianPhone) |
| الاعتماديات | المرحلة 1 |
| التقدير الزمني | 4-5 ساعات |
| نقطة التحقق | Build ناجح + `Patient.cs` لا يحتوي على `AgeYears/AgeMonths/AgeDays` بل يستخدم كائن `Age` + `BreakPeriods` أصبح Collection بدلاً من string |

### المرحلة 3: تفعيل الثوابت التجارية (Business Invariants)

**الوصف:** إضافة منطق التحقق (Validation) داخل الكيانات لضمان عدم حفظ بيانات تخالف قواعد العمل الأساسية.

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | G-11 (NetProfit — تحويله من stored إلى computed مع حماية) |
| الأقسام المنفذة | القسم 9 (Business Invariants) |
| الملفات المتأثرة | `Receipt.cs`, `PatientVisit.cs`, `VisitTest.cs`, `Sample.cs`, `CashTransaction.cs`, `Account.cs`, `OutsourcedSample.cs`, `Culture.cs`, `Comment.cs`, `Doctor.cs`, `Patient.cs` |
| الاعتماديات | المرحلة 2 |
| التقدير الزمني | 8-10 ساعات |
| نقطة التحقق | Build ناجح + 5 اختبارات على الأقل تُثبت رمي `BusinessRuleViolationException` عند خرق Invariants (مثلاً: خصم > الإجمالي، عمر سالب، تاريخ بداية > نهاية) |

### المرحلة 4: بناء البنية التحتية للأحداث النطاقية وآلات الحالة

**الوصف:** إنشاء إطار عمل الأحداث النطاقية (Domain Events) وآلات الحالة (State Machines) لإدارة تدفق العمليات.

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | لا يوجد (بناء أساسي) |
| الأقسام المنفذة | القسم 6 (Domain Events), القسم 8 (State Machines) |
| الملفات المتأثرة | إنشاء مجلد `Events/` مع واجهة `IDomainEvent` + إنشاء مجلد أو منطق State Machines داخل الكيانات (Sample, OutsourcedSample, PatientVisit, Culture, Receipt) |
| الاعتماديات | المرحلة 3 |
| التقدير الزمني | 8-10 ساعات |
| نقطة التحقق | Build ناجح + كل كيان حالة (Sample, Receipt, etc.) يحتوي على دوال انتقالية محمية (مثل `Collect()`, `IssueReceipt()`) تتحقق من الشروط (Guards) قبل الانتقال |

### المرحلة 5: بناء الخدمات النطاقية

**الوصف:** إنشاء واجهات وتطبيقات للخدمات النطاقية التي تنسق العمليات المعقدة بين عدة كيانات.

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | استكمال G-01 (PatientHistory Service) |
| الأقسام المنفذة | القسم 7 (Domain Services) |
| الملفات المتأثرة | إنشاء واجهات لـ 10 خدمات نطاقية (PricingService, AccountingService, MedicalHistoryService, SampleTrackingService, OutsourcingService, ReceiptCalculationService, ResultValidationService, CultureSensitivityService, ReferralCommissionService, PriceListResolverService) |
| الاعتماديات | المرحلة 4 |
| التقدير الزمني | 8-12 ساعات |
| نقطة التحقق | Build ناجح + واجهة `IMedicalHistoryService` قادرة على تجميع التاريخ المرضي + واجهة `IPricingService` تحسب الإجمالي مع الخصم |

### المرحلة 6: تجميع الجذور وخصائص التنقل

**الوصف:** إضافة خصائص التنقل (Navigation Properties) للكيانات الأم لتفعيل التجميع (Aggregation) الكامل.

| البُعد | التفاصيل |
|--------|----------|
| الفجوات المغلقة | لا يوجد (تحسين معماري) |
| الأقسام المنفذة | القسم 2 (Relationships — Navigation Properties), القسم 4 (Aggregate Roots) |
| الملفات المتأثرة | `PatientVisit.cs`, `Receipt.cs`, `Test.cs`, `Culture.cs`, `PriceList.cs`, `User.cs`, `Account.cs` |
| الاعتماديات | المرحلة 5 |
| التقدير الزمني | 4-5 ساعات |
| نقطة التحقق | Build ناجح + `PatientVisit` يحتوي على `ICollection<VisitTest>` و `ICollection<Sample>` و `Receipt` يحتوي على `ICollection<ExtraServiceItem>` |

---

## 6. نقاط التحقق النهائية (Final Validation Criteria)

### معايير القبول النهائية لاستكمال طبقة الدومين

طبقة الدومين تُعتبر "مكتملة ومستقرة" عند تحقيق الشروط التالية:

**المعيار 1 — السلامة الهيكلية:** جميع الكيانات (35 كياناً) تستخدم الأنواع الصحيحة لخصائصها (Enums بدلاً من int، Value Objects بدلاً من Primitive Fields).

**المعيار 2 — السلامة السلوكية:** جميع الـ 20 Business Invariants المقترحة (INV-01 إلى INV-20) مُفعّلة ككود قابل للتنفيذ وليس مجرد تعليقات توثيقية.

**المعيار 3 — السلامة التنسيقية:** الأحداث النطاقية (Domain Events) مُرسلة عند كل تغيير جوهري في حالة الكيانات، وآلات الحالة (State Machines) تمنع الانتقالات غير المشروعة.

**المعيار 4 — السلامة الخدمية:** جميع الخدمات النطاقية (10 خدمات) موجودة كواجهات مع تطبيقات أولية.

**المعيار 5 — السلامة المعمارية:** خصائص التنقل (Navigation Properties) موجودة للكيانات الأم، والكيانات التابعة (Child Entities) مرتبطة بـ Collections.

**المعيار 6 — السلامة التقنية:** Build ناجح 100% + جميع اختبارات الدومين (إن وُجدت) تمر بنجاح + لا يوجد أي `NotImplementedException` في طبقة الدومين.

### المخاطر المتبقية بعد تنفيذ الخارطة

| الخطر | المستوى | الشرح |
|-------|---------|-------|
| ReceiptDto غير متوافق | متوسط | `ReceiptDto` في Application Layer يفتقر إلى `ChangeDue`, `RefundToPatient`, `ExtraServiceItems`. |
| Handlers NotImplemented | متوسط | عدة معالجات في Application Layer ترمي `NotImplementedException`. |
| PatientHistory View | منخفض | Read Model موجود لكن SQL View و Repository غير موجودين (يحتاج Infrastructure). |
| EF Core Configurations | منخفض | لا توجد تكوينات EF Core مخصصة (OnModelCreating) — يجب إعدادها في Infrastructure. |
| Navigation Properties | منخفض | إضافتها قد تتطلب تعديلات في EF Configurations للتعامل مع Cascade/Delete. |

### التوصيات للمراحل التالية

| الطبقة | التوصية |
|--------|---------|
| **Application Layer** | تفعيل AutoMapper بالكامل (إضافة الحقول الناقصة في `ReceiptDto`), إنهاء `NotImplementedException` في جميع الـ Handlers, ربط الـ Domain Services بـ Application Services. |
| **Infrastructure Layer** | إعداد EF Core Configurations (DbContext, OnModelCreating) مع Cascade Rules و Unique Constraints, إنشاء SQL View لـ PatientHistory, تنفيذ Repositories (DbContext-based), إعداد Event Bus لـ Domain Events. |
| **UI Layer (WPF)** | ربط الواجهات بالـ Application Services بدلاً من Entities مباشرة, تفعيل State Machines في الواجهة (أزرار مسح/إصدار/إغلاق), عرض Business Invariant violations كرسائل خطأ للمستخدم. |

---

> **إعداد:** Manus AI — تحليل استراتيجي بناءً على الكود الفعلي عند Commit `613838d` على فرع `niamod`
>
> **التاريخ:** 29 يوليو 2026
>
> **ملاحظة:** هذا التقرير في وضع تحليل وتخطيط فقط. لم يتم تعديل أي ملف أو تنفيذ أي أمر Git أو كتابة أي كود.
