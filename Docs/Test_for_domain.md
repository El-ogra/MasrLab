# خطة اختبارات طبقة الدومين — MasrLab

- **المستودع:** [El-ogra/MasrLab](https://github.com/El-ogra/MasrLab)
- **الفرع:** `niamod`
- **الكوميت المرجعي (مصدر الحقيقة الوحيد):** `c011d26dd0b20a79b21a20f02551e6663311125c`
- **HEAD المؤكَّد:** `c011d26dd0b20a79b21a20f02551e6663311125c` — رسالة الكوميت: "تنفيذ الدومين" (تم التحقق عبر `git log -1`).
- **نطاق الفحص:** `src/MasrLab.Domain/**` فقط. جميع الحالات المذكورة أدناه مأخوذة من قراءة مباشرة للكود، لا من ملفات `Docs/*.md`.

> ⚠️ هذه وثيقة تخطيط للمراجعة البشرية قبل التنفيذ. **لا يوجد أي كود اختبار C# في هذه المرحلة.** كل حالة اختبار موصوفة نصياً فقط: اسم مقترح، Arrange، النتيجة المتوقعة، ومرجع مباشر لسطر الكود.

---

## القسم 0 — الاستثناءات الفعلية المعرَّفة في المشروع

مأخوذة حرفياً من `src/MasrLab.Domain/Exceptions/*.cs`:

| نوع الاستثناء | يرث من | الاستخدام الفعلي داخل Domain |
|---|---|---|
| `BusinessRuleViolationException` | `Exception` | جميع Guards داخل الكيانات (Sample, PatientVisit, Receipt, OutsourcedSample, Culture, Sensitivity, VisitTest, CashTransaction, Comment, CommentTemplate, Doctor). |
| `DuplicateLabIdException` | `Exception` | معرَّف فقط — لا استدعاء له داخل `src/MasrLab.Domain/Entities/**`. |
| `EntityNotFoundException` | `Exception` | معرَّف فقط — لا استدعاء له داخل `src/MasrLab.Domain/Entities/**`. |
| `InsufficientPermissionException` | `Exception` | معرَّف فقط — لا استدعاء له داخل `src/MasrLab.Domain/Entities/**`. |
| `ArgumentException` (BCL) | — | يُستخدم داخل Value Objects الثلاثة فقط: `Age`, `DateRange`, `EgyptianPhone`. |

**تنويه مهم:** الكيانات لا ترمي `ArgumentException` مطلقاً — فقط `BusinessRuleViolationException`. Value Objects تفعل العكس تماماً: فقط `ArgumentException`. هذا اختيار تصميمي مقصود يجب أن تعكسه الاختبارات بدقة.

---

## القسم 1 — Value Objects Guards

### 1.1 `Age` — `src/MasrLab.Domain/ValueObjects/Age.cs`

الشروط المطبَّقة فعلياً في الـ constructor (السطور 9–19):

- إذا كان أي من `years`, `months`, `days` أقل من صفر → `ArgumentException("Age components cannot be negative", nameof(years))`.
- إذا كانت الثلاثة `= 0` معاً → `ArgumentException("At least one age component must be greater than zero", nameof(years))`.

| # | اسم الاختبار المقترح | نوع السيناريو | Arrange (Input) | النتيجة المتوقعة | مرجع الكود |
|---|---|---|---|---|---|
| VO-A-01 | `Constructor_WhenAllComponentsPositive_ShouldCreateInstance` | Happy Path | `years=30, months=2, days=15` | إنشاء ناجح. `Years==30`, `Months==2`, `Days==15`. `TotalMonths==362`. `ToString()=="30 years, 2 months, 15 days"`. | Age.cs L9–L23 |
| VO-A-02 | `Constructor_WhenOnlyYearsPositive_ShouldCreateInstance` | Happy Path | `years=1, months=0, days=0` | إنشاء ناجح. | Age.cs L11–L14 |
| VO-A-03 | `Constructor_WhenOnlyDaysPositive_ShouldCreateInstance` | Happy Path | `years=0, months=0, days=1` | إنشاء ناجح (تحقّق أن الشرط الثاني لا يمنع القيم الحدّية). | Age.cs L13 |
| VO-A-04 | `Constructor_WhenYearsNegative_ShouldThrowArgumentException` | Failure Path | `years=-1, months=0, days=0` | `ArgumentException` تحوي `"Age components cannot be negative"` و `ParamName=="years"`. | Age.cs L11–L12 |
| VO-A-05 | `Constructor_WhenMonthsNegative_ShouldThrowArgumentException` | Failure Path | `years=0, months=-1, days=0` | `ArgumentException` نفس الرسالة. | Age.cs L11–L12 |
| VO-A-06 | `Constructor_WhenDaysNegative_ShouldThrowArgumentException` | Failure Path | `years=0, months=0, days=-1` | `ArgumentException` نفس الرسالة. | Age.cs L11–L12 |
| VO-A-07 | `Constructor_WhenAllZero_ShouldThrowArgumentException` | Failure Path | `years=0, months=0, days=0` | `ArgumentException` تحوي `"At least one age component must be greater than zero"`. | Age.cs L13–L14 |
| VO-A-08 | `TotalMonths_WhenYearsAndMonthsProvided_ShouldReturnYearsTimes12PlusMonths` | Happy Path | `years=2, months=3, days=0` | `TotalMonths==27`. | Age.cs L21 |

### 1.2 `DateRange` — `src/MasrLab.Domain/ValueObjects/DateRange.cs`

الشروط المطبَّقة فعلياً (السطور 8–15):

- إذا كان `end` له قيمة و `end.Value < start` → `ArgumentException("End date must be greater than or equal to Start date", nameof(end))`.
- `end == null` مقبول (نطاق مفتوح).
- `end == start` مقبول (شرط `<` وليس `<=`).

| # | اسم الاختبار المقترح | نوع السيناريو | Arrange (Input) | النتيجة المتوقعة | مرجع الكود |
|---|---|---|---|---|---|
| VO-D-01 | `Constructor_WhenEndAfterStart_ShouldCreateInstance` | Happy Path | `start=2026-01-01, end=2026-01-10` | إنشاء ناجح. | DateRange.cs L8–L15 |
| VO-D-02 | `Constructor_WhenEndEqualsStart_ShouldCreateInstance` | Happy Path | `start=end=2026-01-01` | إنشاء ناجح (شرط `<` صارم). | DateRange.cs L10 |
| VO-D-03 | `Constructor_WhenEndIsNull_ShouldCreateOpenRange` | Happy Path | `start=2026-01-01, end=null` | إنشاء ناجح. `End==null`. | DateRange.cs L10 |
| VO-D-04 | `Constructor_WhenEndBeforeStart_ShouldThrowArgumentException` | Failure Path | `start=2026-01-10, end=2026-01-01` | `ArgumentException` تحوي `"End date must be greater than or equal to Start date"` و `ParamName=="end"`. | DateRange.cs L10–L11 |
| VO-D-05 | `Contains_WhenDateWithinRange_ShouldReturnTrue` | Happy Path | Range `[2026-01-01, 2026-01-31]`, date `2026-01-15` | `true`. | DateRange.cs L17 |
| VO-D-06 | `Contains_WhenDateEqualsStart_ShouldReturnTrue` | Happy Path (boundary) | date == start | `true` (شرط `>=`). | DateRange.cs L17 |
| VO-D-07 | `Contains_WhenDateEqualsEnd_ShouldReturnTrue` | Happy Path (boundary) | date == end | `true` (شرط `<=`). | DateRange.cs L17 |
| VO-D-08 | `Contains_WhenDateBeforeStart_ShouldReturnFalse` | Failure Path | date قبل start | `false`. | DateRange.cs L17 |
| VO-D-09 | `Contains_WhenDateAfterEnd_ShouldReturnFalse` | Failure Path | date بعد end | `false`. | DateRange.cs L17 |
| VO-D-10 | `Contains_WhenEndIsNullAndDateAfterStart_ShouldReturnTrue` | Happy Path | Range مفتوح، date > start | `true` (لأن `!End.HasValue`). | DateRange.cs L17 |
| VO-D-11 | `Duration_WhenEndProvided_ShouldReturnEndMinusStart` | Happy Path | `start=2026-01-01, end=2026-01-11` | `Duration == 10.00:00:00`. | DateRange.cs L19 |

### 1.3 `EgyptianPhone` — `src/MasrLab.Domain/ValueObjects/EgyptianPhone.cs`

الشروط المطبَّقة فعلياً (السطور 7–20):

1. `IsNullOrWhiteSpace(value)` → `ArgumentException("Phone number cannot be empty", nameof(value))`.
2. عدد الأرقام (بعد استخراج `char.IsDigit` فقط) < 10 أو > 11 → `ArgumentException("Egyptian phone number must be 10 or 11 digits", nameof(value))`.
3. الأرقام المستخرجة لا تبدأ بـ `"01"` → `ArgumentException("Egyptian phone number must start with 01", nameof(value))`.

**ملاحظة دقيقة من الكود:** التحقق يتم على **الأرقام المستخرجة** فقط (`digits.StartsWith("01")`)، لكن الحقل المخزَّن `Value` هو **السلسلة الأصلية كما هي**، دون تنظيف. أي أن `"+20 100 000 0000"` يمر ثم يُخزَّن كما هو.

| # | اسم الاختبار المقترح | نوع السيناريو | Arrange (Input) | النتيجة المتوقعة | مرجع الكود |
|---|---|---|---|---|---|
| VO-P-01 | `Constructor_WhenValid11Digits_ShouldCreateInstance` | Happy Path | `"01012345678"` | إنشاء ناجح. `Value=="01012345678"`. | EgyptianPhone.cs L7–L20 |
| VO-P-02 | `Constructor_WhenValid10Digits_ShouldCreateInstance` | Happy Path | `"0112345678"` | إنشاء ناجح (10 أرقام مقبولة). | EgyptianPhone.cs L13 |
| VO-P-03 | `Constructor_WhenContainsFormattingButValidDigits_ShouldCreateInstance` | Happy Path | `"010-1234-5678"` (11 رقم بعد التنقية، يبدأ بـ 01) | إنشاء ناجح. `Value` يحفظ السلسلة الأصلية بالشرطات (لا تنظيف). | EgyptianPhone.cs L12–L19 |
| VO-P-04 | `Constructor_WhenNull_ShouldThrowArgumentException` | Failure Path | `null` | `ArgumentException` تحوي `"Phone number cannot be empty"` و `ParamName=="value"`. | EgyptianPhone.cs L9–L10 |
| VO-P-05 | `Constructor_WhenEmpty_ShouldThrowArgumentException` | Failure Path | `""` | نفس الاستثناء. | EgyptianPhone.cs L9–L10 |
| VO-P-06 | `Constructor_WhenWhitespace_ShouldThrowArgumentException` | Failure Path | `"   "` | نفس الاستثناء. | EgyptianPhone.cs L9–L10 |
| VO-P-07 | `Constructor_WhenDigitsLessThan10_ShouldThrowArgumentException` | Failure Path | `"012345"` (6 أرقام) | `ArgumentException` تحوي `"Egyptian phone number must be 10 or 11 digits"`. | EgyptianPhone.cs L13–L14 |
| VO-P-08 | `Constructor_WhenDigitsMoreThan11_ShouldThrowArgumentException` | Failure Path | `"010123456789"` (12 رقم) | نفس الاستثناء. | EgyptianPhone.cs L13–L14 |
| VO-P-09 | `Constructor_WhenValidLengthButDoesNotStartWith01_ShouldThrowArgumentException` | Failure Path | `"02012345678"` (11 رقم، يبدأ بـ 02) | `ArgumentException` تحوي `"Egyptian phone number must start with 01"`. | EgyptianPhone.cs L16–L17 |
| VO-P-10 | `Constructor_WhenNonDigitCharactersFillString_ShouldThrowArgumentException` | Failure Path | `"abcdefghijk"` (لا يحوي أرقاماً → digits.Length == 0) | `ArgumentException` تحوي `"Egyptian phone number must be 10 or 11 digits"` (يسبق فحص البداية). | EgyptianPhone.cs L12–L14 |
| VO-P-11 | `ToString_ShouldReturnOriginalValue` | Happy Path | `"01012345678"` | `ToString()=="01012345678"`. | EgyptianPhone.cs L22 |

---

## القسم 2 — Business Invariants المُنفَّذة فعلياً داخل الكيانات

مأخوذة عبر `grep` مباشر داخل `src/MasrLab.Domain/Entities/**`. نوع الاستثناء الحقيقي في كل الحالات هو `BusinessRuleViolationException`.

### 2.1 `VisitTest.Price` (setter داخلي)

- الملف: `src/MasrLab.Domain/Entities/Core/VisitTest.cs` L12–L22.
- المستوى: `internal set` — يمنع التعيين من خارج التجميعة (assembly).
- الشرط: `value < 0` → `BusinessRuleViolationException("Price cannot be negative.")`.

| # | الاختبار المقترح | نوع | Arrange | النتيجة المتوقعة | المرجع |
|---|---|---|---|---|---|
| INV-VT-01 | `PriceSetter_WhenValueNegative_ShouldThrowBusinessRuleViolation` | Failure | تعيين `-1` عبر `PatientVisit.AddTest(...)` أو من داخل التجميعة | `BusinessRuleViolationException("Price cannot be negative.")` | VisitTest.cs L18–L19 |
| INV-VT-02 | `PriceSetter_WhenValueZero_ShouldAcceptZero` | Happy | `price=0` | لا استثناء. | VisitTest.cs L18 (شرط `<` لا `<=`) |
| INV-VT-03 | `PriceSetter_WhenValuePositive_ShouldStoreValue` | Happy | `price=250m` | `Price==250m`. | VisitTest.cs L18–L20 |

### 2.2 `CashTransaction.Amount` (setter عام)

- الملف: `src/MasrLab.Domain/Entities/Financial/CashTransaction.cs` L11–L21.
- الشرط: `value <= 0` → `BusinessRuleViolationException("CashTransaction amount must be greater than zero.")`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-CT-01 | `AmountSetter_WhenZero_ShouldThrowBusinessRuleViolation` | Failure | `Amount = 0` | `BusinessRuleViolationException("CashTransaction amount must be greater than zero.")` | CashTransaction.cs L17–L18 |
| INV-CT-02 | `AmountSetter_WhenNegative_ShouldThrowBusinessRuleViolation` | Failure | `Amount = -50m` | نفس الاستثناء. | CashTransaction.cs L17–L18 |
| INV-CT-03 | `AmountSetter_WhenPositive_ShouldStoreValue` | Happy | `Amount = 100m` | `Amount==100m`. | CashTransaction.cs L17–L19 |

### 2.3 `Comment.CommentText` (setter عام)

- الملف: `src/MasrLab.Domain/Entities/Core/Comment.cs` L10–L22.
- الشروط:
  - `IsNullOrWhiteSpace` → `BusinessRuleViolationException("Comment text cannot be empty.")`.
  - `Length > 1000` → `BusinessRuleViolationException("Comment text cannot exceed 1000 characters.")`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-CM-01 | `CommentTextSetter_WhenNull_ShouldThrowBusinessRuleViolation` | Failure | `null` | `BusinessRuleViolationException("Comment text cannot be empty.")` | Comment.cs L16–L17 |
| INV-CM-02 | `CommentTextSetter_WhenWhitespace_ShouldThrowBusinessRuleViolation` | Failure | `"   "` | نفس الاستثناء. | Comment.cs L16–L17 |
| INV-CM-03 | `CommentTextSetter_WhenExactly1000Chars_ShouldAccept` | Happy (boundary) | نص طوله 1000 حرف | لا استثناء. `CommentText` يحفظ القيمة. | Comment.cs L18 (شرط `>` صارم) |
| INV-CM-04 | `CommentTextSetter_WhenExceeds1000Chars_ShouldThrowBusinessRuleViolation` | Failure | نص طوله 1001 حرف | `BusinessRuleViolationException("Comment text cannot exceed 1000 characters.")` | Comment.cs L18–L19 |
| INV-CM-05 | `CommentTextSetter_WhenValid_ShouldStoreValue` | Happy | `"Elevated"` | `CommentText=="Elevated"`. | Comment.cs L20 |

### 2.4 `CommentTemplate.Text` (setter عام)

- الملف: `src/MasrLab.Domain/Entities/Administrative/CommentTemplate.cs` L10–L22.
- شروط مطابقة لـ Comment لكن الرسائل هي `"Comment template text cannot be empty."` و `"Comment template text cannot exceed 1000 characters."`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-TPL-01 | `TextSetter_WhenNull_ShouldThrowBusinessRuleViolation` | Failure | `null` | `BusinessRuleViolationException("Comment template text cannot be empty.")` | CommentTemplate.cs L16–L17 |
| INV-TPL-02 | `TextSetter_WhenWhitespace_ShouldThrowBusinessRuleViolation` | Failure | `"   "` | نفس الاستثناء. | CommentTemplate.cs L16–L17 |
| INV-TPL-03 | `TextSetter_WhenExactly1000Chars_ShouldAccept` | Happy (boundary) | نص 1000 حرف | لا استثناء. | CommentTemplate.cs L18 |
| INV-TPL-04 | `TextSetter_WhenExceeds1000Chars_ShouldThrowBusinessRuleViolation` | Failure | نص 1001 حرف | `BusinessRuleViolationException("Comment template text cannot exceed 1000 characters.")` | CommentTemplate.cs L18–L19 |

### 2.5 `Doctor.CommissionPercent` (setter عام)

- الملف: `src/MasrLab.Domain/Entities/Administrative/Doctor.cs` L13–L23.
- الشرط: `value < 0 || value > 100` → `BusinessRuleViolationException("CommissionPercent must be between 0 and 100.")`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-DR-01 | `CommissionPercentSetter_WhenNegative_ShouldThrowBusinessRuleViolation` | Failure | `-1m` | `BusinessRuleViolationException("CommissionPercent must be between 0 and 100.")` | Doctor.cs L19–L20 |
| INV-DR-02 | `CommissionPercentSetter_WhenGreaterThan100_ShouldThrowBusinessRuleViolation` | Failure | `100.01m` | نفس الاستثناء. | Doctor.cs L19–L20 |
| INV-DR-03 | `CommissionPercentSetter_WhenZero_ShouldAccept` | Happy (boundary) | `0m` | لا استثناء. | Doctor.cs L19 (شرط `<` لا `<=`) |
| INV-DR-04 | `CommissionPercentSetter_WhenExactly100_ShouldAccept` | Happy (boundary) | `100m` | لا استثناء. | Doctor.cs L19 |
| INV-DR-05 | `CommissionPercentSetter_WhenValidMid_ShouldStoreValue` | Happy | `25m` | `CommissionPercent==25m`. | Doctor.cs L19–L21 |

### 2.6 `Sensitivity` (Constructor)

- الملف: `src/MasrLab.Domain/Entities/Culture/Sensitivity.cs` L13–L23.
- الشروط:
  - `cultureId <= 0` → `BusinessRuleViolationException("Sensitivity requires a valid CultureId.")`.
  - `antibioticId <= 0` → `BusinessRuleViolationException("Sensitivity requires a valid AntibioticId.")`.
- يوجد Constructor خاص بدون معاملات (`private Sensitivity()`) لـ EF.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-SN-01 | `Constructor_WhenValidIds_ShouldCreateInstance` | Happy | `cultureId=1, antibioticId=5, level=HighlySensitive` | إنشاء ناجح. الخصائص تُخزَّن. | Sensitivity.cs L20–L22 |
| INV-SN-02 | `Constructor_WhenCultureIdZero_ShouldThrowBusinessRuleViolation` | Failure | `cultureId=0` | `BusinessRuleViolationException("Sensitivity requires a valid CultureId.")` | Sensitivity.cs L15–L16 |
| INV-SN-03 | `Constructor_WhenCultureIdNegative_ShouldThrowBusinessRuleViolation` | Failure | `cultureId=-3` | نفس الاستثناء. | Sensitivity.cs L15–L16 |
| INV-SN-04 | `Constructor_WhenAntibioticIdZero_ShouldThrowBusinessRuleViolation` | Failure | `cultureId=1, antibioticId=0` | `BusinessRuleViolationException("Sensitivity requires a valid AntibioticId.")` | Sensitivity.cs L17–L18 |
| INV-SN-05 | `Constructor_WhenAntibioticIdNegative_ShouldThrowBusinessRuleViolation` | Failure | `antibioticId=-1` | نفس الاستثناء. | Sensitivity.cs L17–L18 |

### 2.7 `Receipt` — قواعد الدفعات والخصم

- الملف: `src/MasrLab.Domain/Entities/Financial/Receipt.cs`.
- Guards:
  - `AddPayment`: `amount <= 0` → `"Payment amount must be greater than zero."`. `PaidNow + amount > Total` → `"Total payment cannot exceed receipt total."`.
  - `ApplyDiscount`: `discountAmount < 0` → `"Discount cannot be negative."`. `discountAmount > Total` → `"Discount cannot exceed receipt total."`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-RC-01 | `AddPayment_WhenAmountZero_ShouldThrowBusinessRuleViolation` | Failure | `Total=100, PaidNow=0, AddPayment(0)` | `BusinessRuleViolationException("Payment amount must be greater than zero.")` | Receipt.cs L35–L36 |
| INV-RC-02 | `AddPayment_WhenAmountNegative_ShouldThrowBusinessRuleViolation` | Failure | `AddPayment(-10)` | نفس الاستثناء. | Receipt.cs L35–L36 |
| INV-RC-03 | `AddPayment_WhenExceedsRemaining_ShouldThrowBusinessRuleViolation` | Failure | `Total=100, PaidNow=60, AddPayment(50)` (المجموع 110) | `BusinessRuleViolationException("Total payment cannot exceed receipt total.")` | Receipt.cs L37–L38 |
| INV-RC-04 | `AddPayment_WhenExactRemaining_ShouldAcceptAndSetRemainingZero` | Happy | `Total=100, PaidNow=40, AddPayment(60)` | لا استثناء. `PaidNow==100`, `Remaining==0`. | Receipt.cs L37, L40–L42 |
| INV-RC-05 | `AddPayment_WhenPartial_ShouldUpdatePaidAndRemaining` | Happy | `Total=100, AddPayment(30)` | `PaidNow==30`, `Remaining==70`. | Receipt.cs L40–L42 |
| INV-RC-06 | `ApplyDiscount_WhenNegative_ShouldThrowBusinessRuleViolation` | Failure | `Total=100, ApplyDiscount(-1)` | `BusinessRuleViolationException("Discount cannot be negative.")` | Receipt.cs L48–L49 |
| INV-RC-07 | `ApplyDiscount_WhenExceedsTotal_ShouldThrowBusinessRuleViolation` | Failure | `Total=100, ApplyDiscount(150)` | `BusinessRuleViolationException("Discount cannot exceed receipt total.")` | Receipt.cs L50–L51 |
| INV-RC-08 | `ApplyDiscount_WhenZero_ShouldAccept` | Happy (boundary) | `Total=100, ApplyDiscount(0)` | لا استثناء. `Discount==0`. | Receipt.cs L48 (شرط `<`) |
| INV-RC-09 | `ApplyDiscount_WhenEqualsTotal_ShouldAccept` | Happy (boundary) | `Total=100, ApplyDiscount(100)` | لا استثناء. `Discount==100`, `Remaining==0`. | Receipt.cs L50 (شرط `>`)، L54–L55 |
| INV-RC-10 | `ApplyDiscount_WhenValidAndPaidNowSet_ShouldClampRemainingAtZero` | Happy | `Total=100, PaidNow=80, ApplyDiscount(30)` | `Remaining==0` (بسبب `if (Remaining < 0) Remaining = 0`). | Receipt.cs L54–L55 |

### 2.8 `OutsourcedSample.SetPrices`

- الملف: `src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs` L18–L24.
- الشرط: `patientPrice < costPrice` → `BusinessRuleViolationException("Patient price must be greater than or equal to cost price.")`.

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| INV-OS-01 | `SetPrices_WhenPatientPriceLessThanCost_ShouldThrowBusinessRuleViolation` | Failure | `patientPrice=80, costPrice=100` | `BusinessRuleViolationException("Patient price must be greater than or equal to cost price.")` | OutsourcedSample.cs L20–L21 |
| INV-OS-02 | `SetPrices_WhenEqual_ShouldAccept` | Happy (boundary) | `patientPrice=100, costPrice=100` | لا استثناء. `PatientPrice==CostPrice==100`. | OutsourcedSample.cs L20 (شرط `<`) |
| INV-OS-03 | `SetPrices_WhenPatientPriceGreater_ShouldStoreBoth` | Happy | `patientPrice=150, costPrice=100` | `PatientPrice==150, CostPrice==100`. | OutsourcedSample.cs L22–L23 |

---

## القسم 3 — State Machines (انتقالات الحالة المحمية)

### 3.1 `Sample` — `src/MasrLab.Domain/Entities/Core/Sample.cs`

Enum المستخدَم فعلياً: `SampleStatus { Collected, NotCollected }` (Common/Enums/SampleStatus.cs).

الانتقالات المطبَّقة:

| الحالة الحالية | Method | Guard الفعلي في الكود | الحالة التالية | Event المنبعث |
|---|---|---|---|---|
| ≠ `Collected` | `Collect(int userId)` | إذا كانت `Collected` مسبقاً → `BusinessRuleViolationException("Sample has already been collected.")` | `Collected` + تعيين `CollectedByUserId` و `CollectedAt = DateTime.UtcNow` | `SampleCollected(Id, PatientVisitId, TestId, userId)` |
| `Collected` | `RevertCollection()` | إذا لم تكن `Collected` → `BusinessRuleViolationException("Sample is not in collected state.")` | `NotCollected` + `CollectedByUserId=null`, `CollectedAt=null` | `SampleUncollectedReverted(Id, PatientVisitId)` |

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| SM-S-01 | `Collect_WhenNotCollected_ShouldTransitionToCollectedAndRaiseEvent` | Happy | Sample جديد (`CollectionStatus=NotCollected`), `userId=7` | `CollectionStatus==Collected`, `CollectedByUserId==7`, `CollectedAt!=null`, و`DomainEvents` يحوي حدث `SampleCollected` واحد بالحمولة الصحيحة. | Sample.cs L18–L26 |
| SM-S-02 | `Collect_WhenAlreadyCollected_ShouldThrowBusinessRuleViolation` | Failure | Sample.CollectionStatus = Collected مسبقاً | `BusinessRuleViolationException("Sample has already been collected.")` ولا حدث جديد. | Sample.cs L20–L21 |
| SM-S-03 | `RevertCollection_WhenCollected_ShouldRevertAndRaiseEvent` | Happy | تم `Collect(1)` أولاً | `CollectionStatus==NotCollected`, `CollectedByUserId==null`, `CollectedAt==null`, حدث `SampleUncollectedReverted`. | Sample.cs L28–L36 |
| SM-S-04 | `RevertCollection_WhenNotCollected_ShouldThrowBusinessRuleViolation` | Failure | `CollectionStatus=NotCollected` (تعيين صريح — العضو الأول في enum هو `Collected` وهو الافتراضي) | `BusinessRuleViolationException("Sample is not in collected state.")` | Sample.cs L30–L31 |

### 3.2 `PatientVisit` — `src/MasrLab.Domain/Entities/Core/PatientVisit.cs`

Enum: `VisitStatus { Registered, ResultsEntered, Printed, Closed }`.

الانتقالات/الحُرَّاس المطبَّقة:

| Method | Guard الفعلي | التأثير على الحالة | Event |
|---|---|---|---|
| `Create(...)` (static factory) | لا Guard صريح، يعيّن `Status=Registered` | `Status=Registered` | `PatientVisitCreated` |
| `AddTest(testId, price, isOutsourced)` | `Status == Closed` → `"Cannot add tests to a closed visit."` | لا تغيير حالة | `VisitTestAdded` |
| `EnterAllResults()` | `Status != Registered` → `"Visit must be in Registered status to enter results."` ثم `!VisitTests.Any()` → `"Cannot enter results for a visit with no tests."` | `Status = ResultsEntered` | لا حدث |
| `IssueReceipt()` | `Status != Registered && Status != ResultsEntered` → `"Visit must be open to issue a receipt."` | `Status = ResultsEntered` (حتى لو كانت `Registered`) | لا حدث |
| `Close(finalTotal)` | `Status == Closed` → `"Visit is already closed."` | `Status = Closed` | `VisitClosed(Id, DateTime.UtcNow, finalTotal)` |

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| SM-V-01 | `Create_WithValidArgs_ShouldReturnRegisteredVisitAndRaiseCreatedEvent` | Happy | `patientId=1, userId=1, labId="L1"` | `Status==Registered`, `VisitDate` قريبة من UtcNow، `DomainEvents` يحوي `PatientVisitCreated`. | PatientVisit.cs L25–L39 |
| SM-V-02 | `AddTest_WhenVisitOpen_ShouldAppendVisitTestAndRaiseEvent` | Happy | Visit جديد بحالة `Registered` | `VisitTests.Count==1` مع القيم الممرَّرة، حدث `VisitTestAdded(Id, testId, price, isOutsourced)`. | PatientVisit.cs L41–L54 |
| SM-V-03 | `AddTest_WhenVisitClosed_ShouldThrowBusinessRuleViolation` | Failure | استدعِ `Close(0)` ثم `AddTest(...)` | `BusinessRuleViolationException("Cannot add tests to a closed visit.")` | PatientVisit.cs L43–L44 |
| SM-V-04 | `EnterAllResults_WhenRegisteredAndHasTests_ShouldTransitionToResultsEntered` | Happy | Visit=Registered، أضف اختباراً واحداً | `Status==ResultsEntered`، لا حدث. | PatientVisit.cs L56–L63 |
| SM-V-05 | `EnterAllResults_WhenStatusNotRegistered_ShouldThrowBusinessRuleViolation` | Failure | استدعِ `EnterAllResults` مرتين متتاليتين (الثانية بينما الحالة `ResultsEntered`) | `BusinessRuleViolationException("Visit must be in Registered status to enter results.")` | PatientVisit.cs L58–L59 |
| SM-V-06 | `EnterAllResults_WhenNoTests_ShouldThrowBusinessRuleViolation` | Failure | Visit=Registered، `VisitTests` فارغة | `BusinessRuleViolationException("Cannot enter results for a visit with no tests.")` | PatientVisit.cs L60–L61 |
| SM-V-07 | `IssueReceipt_WhenRegistered_ShouldSetResultsEntered` | Happy | Visit=Registered | `Status==ResultsEntered`. | PatientVisit.cs L65–L70 |
| SM-V-08 | `IssueReceipt_WhenResultsEntered_ShouldRemainResultsEntered` | Happy | Visit=ResultsEntered | `Status==ResultsEntered` (لا استثناء). | PatientVisit.cs L67–L69 |
| SM-V-09 | `IssueReceipt_WhenClosed_ShouldThrowBusinessRuleViolation` | Failure | Visit=Closed | `BusinessRuleViolationException("Visit must be open to issue a receipt.")` | PatientVisit.cs L67–L68 |
| SM-V-10 | `IssueReceipt_WhenPrinted_ShouldThrowBusinessRuleViolation` | Failure | Visit=Printed (لا methods أخرى تصل هنا، لكن الحالة موجودة في enum) | نفس الاستثناء (Guard يقصر السماح على Registered و ResultsEntered فقط). | PatientVisit.cs L67–L68 |
| SM-V-11 | `Close_WhenOpen_ShouldTransitionToClosedAndRaiseEvent` | Happy | Visit=Registered، `Close(500m)` | `Status==Closed`، حدث `VisitClosed` مع `FinalTotal=500`. | PatientVisit.cs L72–L78 |
| SM-V-12 | `Close_WhenAlreadyClosed_ShouldThrowBusinessRuleViolation` | Failure | Visit=Closed، ثم `Close(...)` مجدداً | `BusinessRuleViolationException("Visit is already closed.")` | PatientVisit.cs L74–L75 |

### 3.3 `OutsourcedSample` — `src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs`

Enum: `SettlementStatus { Pending, PartiallySettled, Settled }`.

| Method | Guard | التأثير | Event |
|---|---|---|---|
| `SetPrices(patientPrice, costPrice)` | راجع القسم 2.8 (INV-OS-*) | تعيين الأسعار | لا |
| `Send(externalLabId, costPrice)` | `SettlementStatus != Pending` → `"Outsourced sample must be in Pending status to send."` | يعين `ExternalLabId`, `CostPrice`, ويعيد `SettlementStatus=Pending` (كما هي) | `OutsourcedSampleSent` |
| `ReceiveResult()` | `ReceivedAt.HasValue` → `"Result has already been received for this outsourced sample."` | `ReceivedAt = DateTime.UtcNow` | `OutsourcedResultReceived` |

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| SM-OS-01 | `Send_WhenPending_ShouldSetLabAndCostAndRaiseEvent` | Happy | SettlementStatus=Pending (افتراضي enum) | `ExternalLabId, CostPrice` مضبوطتان، حدث `OutsourcedSampleSent`. | OutsourcedSample.cs L26–L34 |
| SM-OS-02 | `Send_WhenPartiallySettled_ShouldThrowBusinessRuleViolation` | Failure | SettlementStatus=PartiallySettled يدوياً | `BusinessRuleViolationException("Outsourced sample must be in Pending status to send.")` | OutsourcedSample.cs L28–L29 |
| SM-OS-03 | `Send_WhenSettled_ShouldThrowBusinessRuleViolation` | Failure | SettlementStatus=Settled | نفس الاستثناء. | OutsourcedSample.cs L28–L29 |
| SM-OS-04 | `ReceiveResult_WhenReceivedAtIsNull_ShouldSetTimestampAndRaiseEvent` | Happy | `ReceivedAt=null` | `ReceivedAt!=null`، حدث `OutsourcedResultReceived(Id, ReceivedAt.Value)`. | OutsourcedSample.cs L36–L42 |
| SM-OS-05 | `ReceiveResult_WhenAlreadyReceived_ShouldThrowBusinessRuleViolation` | Failure | استدعاء `ReceiveResult()` مرتين | `BusinessRuleViolationException("Result has already been received for this outsourced sample.")` | OutsourcedSample.cs L38–L39 |

### 3.4 `Culture` — `src/MasrLab.Domain/Entities/Culture/Culture.cs`

لا يوجد enum حالة في `Culture`؛ الانتقال الوحيد يقاس بحضور/غياب Organisms.

| Method | Guard | التأثير | Event |
|---|---|---|---|
| `Record(colonyCount, organismA, organismB, organismC)` | لا Guard | تعيين القيم | `CultureRecorded(Id, Id)` (⚠️ VisitTestId يُمرَّر `Id` نفسه — سلوك الكود كما هو) |
| `RecordSensitivity(antibioticId, level)` | إذا كانت `OrganismA`, `OrganismB`, `OrganismC` كلها `IsNullOrEmpty` → `"Cannot record sensitivity without at least one organism."` | إنشاء وإضافة `Sensitivity` جديدة | `SensitivityRecorded(sensitivity.Id, Id)` |

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| SM-C-01 | `Record_ShouldSetFieldsAndRaiseCultureRecorded` | Happy | `colonyCount=10^5, organismA="E.coli"` | القيم مخزَّنة، حدث `CultureRecorded` واحد. | Culture.cs L19–L26 |
| SM-C-02 | `RecordSensitivity_WhenNoOrganisms_ShouldThrowBusinessRuleViolation` | Failure | `OrganismA/B/C = null` | `BusinessRuleViolationException("Cannot record sensitivity without at least one organism.")` | Culture.cs L30–L31 |
| SM-C-03 | `RecordSensitivity_WhenOrganismAEmpty_ShouldThrowIfAllEmpty` | Failure | `OrganismA=""` والباقي null | نفس الاستثناء (شرط `IsNullOrEmpty` على الثلاثة معاً). | Culture.cs L30 |
| SM-C-04 | `RecordSensitivity_WithAtLeastOneOrganism_ShouldAddSensitivityAndRaiseEvent` | Happy | `OrganismA="E.coli"`, `antibioticId=3`, `level=HighlySensitive` | `Sensitivities.Count==1`، حدث `SensitivityRecorded`. | Culture.cs L28–L35 |
| SM-C-05 | `RecordSensitivity_WhenAntibioticIdInvalid_ShouldPropagateSensitivityBusinessRuleViolation` | Failure | organism موجود، `antibioticId=0` | `BusinessRuleViolationException("Sensitivity requires a valid AntibioticId.")` (منبعث من ctor `Sensitivity`). | Culture.cs L32 + Sensitivity.cs L17–L18 |
| SM-C-06 | `RecordSensitivity_WhenCultureIdZero_ShouldPropagateSensitivityBusinessRuleViolation` | Failure | Culture غير محفوظ (Id=0)، organism موجود | `BusinessRuleViolationException("Sensitivity requires a valid CultureId.")` (Culture يمرّر `Id` إلى ctor Sensitivity). | Culture.cs L32 + Sensitivity.cs L15–L16 |

### 3.5 `Receipt` — `src/MasrLab.Domain/Entities/Financial/Receipt.cs`

**ملاحظة تصميمية:** لا يوجد حقل `Status` أو enum لحالة الإيصال في الكود الفعلي. لا Guard في `Issue`؛ يمكن استدعاؤها أكثر من مرة (تُعيد ضبط `Total, IssueDate, ReceiveTime, Remaining`) وترفع `ReceiptIssued` كل مرة.

| Method | Guard | التأثير | Event |
|---|---|---|---|
| `Issue(total)` | لا Guard | يعيد ضبط `Total, IssueDate, ReceiveTime, Remaining=total` | `ReceiptIssued` |
| `AddPayment(amount)` | راجع القسم 2.7 (INV-RC-01..05) | يحدّث `PaidNow, Remaining` (Remaining يُقصّ عند سالب) | `ReceiptPaymentAdded` |
| `ApplyDiscount(discount)` | راجع القسم 2.7 (INV-RC-06..10) | يحدّث `Discount, Remaining` | `DiscountApplied` |

| # | الاختبار | نوع | Arrange | المتوقع | المرجع |
|---|---|---|---|---|---|
| SM-RC-01 | `Issue_ShouldSetTotalRemainingAndDatesAndRaiseEvent` | Happy | `Issue(200m)` على Receipt جديد | `Total==200`, `Remaining==200`, `IssueDate` قريب من UtcNow، حدث `ReceiptIssued(Id, PatientVisitId, 200, PaidNow)`. | Receipt.cs L24–L31 |
| SM-RC-02 | `Issue_CalledTwice_ShouldOverwriteTotalAndRaiseTwoEvents` | Happy | `Issue(100)` ثم `Issue(150)` | `Total==150`, `Remaining==150`، `DomainEvents.Count(...)==2` من نوع `ReceiptIssued`. | Receipt.cs L24–L31 (لا Guard) |
| SM-RC-03 | `AddPayment_ShouldRaiseReceiptPaymentAddedEvent` | Happy | `Issue(100)` ثم `AddPayment(40)` | حدث `ReceiptPaymentAdded(Id, 40)`، `PaidNow==40`, `Remaining==60`. | Receipt.cs L33–L44 |
| SM-RC-04 | `ApplyDiscount_ShouldRaiseDiscountAppliedEvent` | Happy | `Issue(100)` ثم `ApplyDiscount(20)` | حدث `DiscountApplied(Id, 20)`، `Discount==20`, `Remaining==80`. | Receipt.cs L46–L57 |

راجع أيضاً INV-RC-01..10 في القسم 2.7 للسيناريوهات السلبية.

---

## القسم 4 — Domain Events

الأحداث المعرَّفة فعلياً في `src/MasrLab.Domain/Events/DomainEvents.cs` (20 حدثاً + النوع الأساسي).

`BaseEntity.AddDomainEvent(...)` (BaseEntity.cs L18–L21) هي القناة الوحيدة، وترصد الأحداث في `List<IDomainEvent>` تعرَّض للقراءة عبر `DomainEvents` وتُنظَّف عبر `ClearDomainEvents()`.

### 4.1 الأحداث المُطلَقة فعلياً من داخل الكيانات

| # | الحدث | Method المُطلِقة | الملف/السطر | الحمولة الفعلية |
|---|---|---|---|---|
| E-01 | `PatientVisitCreated` | `PatientVisit.Create` | PatientVisit.cs L37 | `(int VisitId, int PatientId, int? DoctorId, DateTime VisitDate)` |
| E-02 | `VisitTestAdded` | `PatientVisit.AddTest` | PatientVisit.cs L53 | `(int VisitId, int TestId, decimal Price, bool IsOutsourced)` |
| E-03 | `VisitClosed` | `PatientVisit.Close` | PatientVisit.cs L77 | `(int VisitId, DateTime ClosedAt, decimal FinalTotal)` |
| E-04 | `SampleCollected` | `Sample.Collect` | Sample.cs L25 | `(int SampleId, int VisitId, int TestId, int CollectedBy)` |
| E-05 | `SampleUncollectedReverted` | `Sample.RevertCollection` | Sample.cs L35 | `(int SampleId, int VisitId)` |
| E-06 | `OutsourcedSampleSent` | `OutsourcedSample.Send` | OutsourcedSample.cs L33 | `(int OutsourcedSampleId, int VisitTestId, int ExternalLabId, decimal CostPrice)` |
| E-07 | `OutsourcedResultReceived` | `OutsourcedSample.ReceiveResult` | OutsourcedSample.cs L41 | `(int OutsourcedSampleId, DateTime ReceivedAt)` |
| E-08 | `CultureRecorded` | `Culture.Record` | Culture.cs L25 | `(int CultureId, int VisitTestId)` — ⚠️ الكود يمرّر `Id, Id` (نفس المُعرِّف مرتين) |
| E-09 | `SensitivityRecorded` | `Culture.RecordSensitivity` | Culture.cs L34 | `(int SensitivityId, int CultureId)` |
| E-10 | `ReceiptIssued` | `Receipt.Issue` | Receipt.cs L30 | `(int ReceiptId, int VisitId, decimal Total, decimal Paid)` |
| E-11 | `ReceiptPaymentAdded` | `Receipt.AddPayment` | Receipt.cs L43 | `(int ReceiptId, decimal Amount)` |
| E-12 | `DiscountApplied` | `Receipt.ApplyDiscount` | Receipt.cs L56 | `(int ReceiptId, decimal DiscountValue)` |

### 4.2 حالات اختبار Events

| # | الاختبار | Method | التحقق | المرجع |
|---|---|---|---|---|
| EV-01 | `Create_ShouldRaiseExactlyOnePatientVisitCreatedEventWithCorrectPayload` | `PatientVisit.Create` | حدث واحد من النوع `PatientVisitCreated` وبيانات مطابقة. | PatientVisit.cs L37 |
| EV-02 | `AddTest_ShouldRaiseVisitTestAddedEventWithSamePriceAndFlag` | `PatientVisit.AddTest` | حمولة الحدث تطابق الوسائط. | PatientVisit.cs L53 |
| EV-03 | `AddTest_WhenClosed_ShouldNotRaiseAnyEvent` | `PatientVisit.AddTest` بعد Close | `DomainEvents` لا يحوي `VisitTestAdded` جديداً. | PatientVisit.cs L43–L44 |
| EV-04 | `Close_ShouldRaiseVisitClosedWithFinalTotal` | `PatientVisit.Close(750m)` | حدث `VisitClosed` بـ `FinalTotal==750`. | PatientVisit.cs L77 |
| EV-05 | `EnterAllResults_ShouldNotRaiseAnyDomainEvent` | `PatientVisit.EnterAllResults` | لا حدث. | PatientVisit.cs L56–L63 |
| EV-06 | `IssueReceipt_OnPatientVisit_ShouldNotRaiseAnyDomainEvent` | `PatientVisit.IssueReceipt` | لا حدث (بخلاف `Receipt.Issue`). | PatientVisit.cs L65–L70 |
| EV-07 | `Sample_Collect_ShouldRaiseSampleCollectedEvent` | `Sample.Collect(userId)` | حدث واحد بالحمولة الصحيحة. | Sample.cs L25 |
| EV-08 | `Sample_RevertCollection_ShouldRaiseSampleUncollectedRevertedEvent` | `Sample.RevertCollection` | حدث واحد `(Id, PatientVisitId)`. | Sample.cs L35 |
| EV-09 | `OutsourcedSample_Send_ShouldRaiseOutsourcedSampleSentWithCostPrice` | `OutsourcedSample.Send` | حمولة تحوي `ExternalLabId, CostPrice`. | OutsourcedSample.cs L33 |
| EV-10 | `OutsourcedSample_ReceiveResult_ShouldRaiseOutsourcedResultReceivedWithReceivedAt` | `OutsourcedSample.ReceiveResult` | الحدث يحمل `ReceivedAt` مطابقاً لما ضُبِط. | OutsourcedSample.cs L41 |
| EV-11 | `Culture_Record_ShouldRaiseCultureRecordedWithIdEqualToItself` | `Culture.Record` | حدث `CultureRecorded(Id, Id)` — يوثِّق السلوك الحالي حرفياً. | Culture.cs L25 |
| EV-12 | `Culture_RecordSensitivity_ShouldRaiseSensitivityRecordedWithSensitivityId` | `Culture.RecordSensitivity` | حدث واحد بالمعرِّف الصحيح. | Culture.cs L34 |
| EV-13 | `Receipt_Issue_ShouldRaiseReceiptIssuedWithTotalAndCurrentPaidNow` | `Receipt.Issue` | حدث `ReceiptIssued(Id, PatientVisitId, total, PaidNow)`. | Receipt.cs L30 |
| EV-14 | `Receipt_AddPayment_ShouldRaiseReceiptPaymentAddedOnlyOnSuccess` | `Receipt.AddPayment` | حدث `ReceiptPaymentAdded` لا يُطلَق إذا رمى Guard. | Receipt.cs L36, L38, L43 |
| EV-15 | `Receipt_ApplyDiscount_ShouldRaiseDiscountAppliedOnlyOnSuccess` | `Receipt.ApplyDiscount` | نفس السلوك للخصم. | Receipt.cs L49, L51, L56 |
| EV-16 | `BaseEntity_ClearDomainEvents_ShouldEmptyList` | `BaseEntity.ClearDomainEvents` | بعد إطلاق أحداث ثم استدعاء ClearDomainEvents، `DomainEvents.Count==0`. | BaseEntity.cs L23–L26 |
| EV-17 | `BaseEntity_DomainEventsCollection_ShouldBeReadOnly` | استعراض `DomainEvents` | النوع `IReadOnlyCollection<IDomainEvent>` (لا `Add` من الخارج). | BaseEntity.cs L16 |

---

## القسم 5 — تناقضات مكتشفة (مقارنة الكود الفعلي مع تقرير `Docs/Domaim1-10.md`)

التقرير في `Docs/Domaim1-10.md` (أقسام 6 و 8 و 9) يدّعي: **20 Domain Event، 20 Business Invariant، 5 State Machines كاملة**. ما يلي فجوات موثَّقة بين الادعاء والكود المصدري عند الكوميت `c011d26`.

### 5.1 Domain Events — 20 مُدَّعاة، الفعلي المُنبعث من كيانات = 12

| المعرِّف في التقرير | الحدث | الحالة الفعلية | مرجع الكود |
|---|---|---|---|
| E-01 | `PatientRegistered` | مُعرَّف كـ `record` في `DomainEvents.cs` L8، **لا يُطلَق من أي كيان**. `Patient.cs` لا يحوي أي `AddDomainEvent`. | DomainEvents.cs L8 ↔ Patient.cs (كامل الملف) |
| E-02 | `PatientUpdated` | مُعرَّف L10، **لا يُطلَق أبداً** من الكيان. | DomainEvents.cs L10 ↔ Patient.cs |
| E-05 | `VisitTestRemoved` | مُعرَّف L16، **لا يُطلَق**؛ `PatientVisit` لا يوفّر عملية إزالة تحليل. | DomainEvents.cs L16 |
| E-08/E-09 | `TestResultEntered`, `TestResultEdited` | مُعرَّفَان L22 و L24، **لا يُطلَقان**؛ `TestResult` كيان بيانات نقي بلا methods. | DomainEvents.cs L22, L24 ↔ TestResult.cs |
| E-17/E-18 | `CashDeposited`, `CashWithdrawn` | مُعرَّفَان L40 و L42، **لا يُطلَقان**؛ `CashTransaction` لا يستدعي `AddDomainEvent`. | DomainEvents.cs L40, L42 ↔ CashTransaction.cs |
| E-19 | `CommentAttachedToResult` | مُعرَّف L44، **لا يُطلَق**؛ `Comment` لا يحوي `AddDomainEvent`. | DomainEvents.cs L44 ↔ Comment.cs |

**الخلاصة:** 8 من الأحداث الـ 20 معرَّفة كأنواع فقط دون أن تُطلَق من أي كيان — بينها أحداث محورية (تسجيل مريض، إدخال/تعديل نتيجة، إيداع/سحب نقدي). العدد الفعلي للأحداث المفعَّلة داخل Domain: **12** فقط (راجع القسم 4.1).

### 5.2 Business Invariants — 20 مُدَّعاة، الفعلي داخل Entities/VOs ≈ 10

| المعرِّف | الادعاء | الحالة الفعلية |
|---|---|---|
| INV-01 | `Receipt.Total == Σ(VisitTest.Price) + Σ(ExtraServices.Price) − Discount` | **غير منفَّذ**. `Receipt.Issue(total)` يقبل الإجمالي بارامتراً بدون أي حساب أو تحقق ضد `VisitTests` أو `ExtraServiceItems`. تعليق L9–L10 يعترف: "Actual computation via PricingService — Domain Service phase, not yet implemented." |
| INV-02 | `VisitTest.IsOutsourced ⟺ يوجد OutsourcedSample مرتبط بنفس (PatientVisitId, TestId)` | **غير منفَّذ**. لا يوجد أي Guard في `PatientVisit.AddTest` أو في `OutsourcedSample` يفرض هذا الارتباط. |
| INV-03 | `PatientVisit.DoctorId == null → افتراض Patient.DoctorId` | **غير منفَّذ**. `PatientVisit.Create` يحفظ `doctorId` كما هو ولا يقرأ من `Patient`. التعليق L24 يعترف بأنها قاعدة نظرية. |
| INV-04 | `VisitTest.Price snapshot لا يتغير عند تعديل PriceList` | **جزئي فقط**: `Price` له setter `internal` مع فحص السالب — لكن لا آلية داخل الدومين تفرض عدم تعديله بعد الإنشاء (لا `init` ولا حالة "مقفلة"). التعليق L8 يعترف بأنها سياسة، لا Invariant حقيقي. |
| INV-05 | `NetProfit يُعاد حسابه بعد كل حدث مالي` | **غير منفَّذ**. لا خدمة تنفيذية داخل `MasrLab.Domain` (`Services/*.cs` كلها Interfaces فقط). |
| INV-06 | `CommissionPercent ∈ [0, 100]` | ✅ **منفَّذ فقط على `Doctor.CommissionPercent`** (Doctor.cs L19–L20). لا وجود لعمولة على `ReferralEntity` أصلاً — الحقل غائب من الكيان. |
| INV-07 | `Age` مكوّناتها ≥ 0 وليست كلها 0 | ✅ منفَّذ (Age.cs L11–L14). |
| INV-08 | `DateRange.Start ≤ End` | ✅ منفَّذ (DateRange.cs L10–L11). |
| INV-09 | `Receipt.Paid ≤ Receipt.Total` | ✅ منفَّذ (Receipt.cs L37–L38). |
| INV-10 | `Discount ≤ Subtotal` | ✅ منفَّذ جزئياً (Receipt.cs L50–L51) — يقارن ضد `Total` وليس `Subtotal` (لا يوجد حقل `Subtotal` في الكيان). |
| INV-11 | `Sample.CollectedAt ≠ null ⟺ IsCollected == true` | **جزئي**: `Sample.Collect` يعيّن `CollectedAt=UtcNow` و`RevertCollection` يعيّنها `null` — لكن الحقل `CollectedAt` هو setter عام حر، ولا Invariant صريح يفرض التلازم. |
| INV-12 | `TestResult موجودة ⟹ Sample.IsCollected == true` | **غير منفَّذ**. `TestResult` كيان بيانات؛ لا Guard في أي مكان يمنع إدخال نتيجة قبل السحب. |
| INV-13 | `OutsourcedSample.PatientPrice ≥ CostPrice` | ✅ منفَّذ فقط داخل `SetPrices(...)` (OutsourcedSample.cs L20–L21). ⚠️ الفحص **يتجاوَز عبر setters عامة**: `PatientPrice` و `CostPrice` مُعرَّفتان `public { get; set; }` (L14–L15) دون Guard خلفي، فالتعيين المباشر يهرب من الفحص. |
| INV-14 | `Culture مرتبطة فقط بـ VisitTest من نوع Microbiology` | **غير منفَّذ**. `Culture` لا يحوي أي مرجع لـ `VisitTestId` أو `TestType`؛ لا Guard. |
| INV-15 | `Sensitivity.CultureId ≠ null` | ✅ منفَّذ (Sensitivity.cs L15–L16) — أقوى من المُدَّعى: يشترط `> 0`. |
| INV-16 | `ReferralEntity.PriceListId موجود وفعّال` | **غير قابل للتحقق داخل Domain** — لا Guard في `ReferralEntity`، الفعّالية تعني وصولاً لمستودع (خارج Domain). |
| INV-17 | لا يمكن حذف Patient/Visit إذا كان لديه Receipt | **غير منفَّذ داخل Domain**. `ISoftDeletable` مجرد علامة (`IsDeleted` bool) دون Guards. |
| INV-18 | كل `VisitTest` يرتبط بمريض واحد عبر PatientVisit | **بنيوي فقط** عبر FK؛ لا Guard صريح داخل الدومين. |
| INV-19 | `CashTransaction.Amount > 0` | ✅ منفَّذ (CashTransaction.cs L17–L18) — يستخدم `<= 0`. |
| INV-20 | `Comment.Text` بين 1 و MaxCommentLength | ✅ منفَّذ (Comment.cs L16–L19) — الحد الأقصى 1000 (ثابت hardcoded)؛ يوجد نظير في `CommentTemplate.Text`. |

**الخلاصة العددية:** المطبَّق فعلياً بشكل قاطع = 9 (INV-06 جزئياً، 07، 08، 09، 10 جزئياً، 13 مع ثغرة setters، 15، 19، 20). البقية إما غائبة تماماً، أو تُنفَّذ فقط داخل مسار واحد قابل للتحايل عبر setters عامة، أو تنتظر Domain Services غير موجودة.

### 5.3 State Machines — 5 مُدَّعاة كاملة

| SM | الادعاء في التقرير | الحالة الفعلية |
|---|---|---|
| 8.1 Sample | حالتان + انتقالان + حَدَثان | ✅ **مطابق**. Sample.cs يوفّر `Collect` و`RevertCollection` مع Guards وأحداث. |
| 8.2 OutsourcedSample | 3 حالات: Marked → Sent → ResultReceived → Completed مع event `AttachToReport` | ❌ **جزئي**. الكود يحوي: `Send` (Guard = `Pending`)، `ReceiveResult` (Guard = `ReceivedAt == null`). **لا يوجد** حالات "Marked" أو "Completed"، ولا انتقال `AttachToReport`. `SettlementStatus` enum ثابت `{Pending, PartiallySettled, Settled}` ولا كود يفعّل الانتقال إلى `PartiallySettled` أو `Settled`. |
| 8.3 PatientVisit | Draft → Open → Completed → Closed + كل الانتقالات محروسة بضمانات | ❌ **مغاير جوهرياً**. Enum الفعلي `VisitStatus { Registered, ResultsEntered, Printed, Closed }` — لا "Draft" ولا "Open" ولا "Completed". `Printed` معرَّفة في enum لكن **لا method** ينقل إليها. `Create` لا يفحص `Tests.Count >= 1` (الادعاء). `EnterAllResults` لا يفحص أن لكل VisitTest نتيجة (الادعاء)، بل فقط أن الحالة `Registered` و`Tests.Any()`. `IssueReceipt` يتيح البقاء في `Registered` أو `ResultsEntered` — لا يفحص وجود Receipt سابق. `Close` لا يفحص `Receipt.Remaining==0`. |
| 8.4 Culture | Pending → Recorded → WithSensitivity مع Guard `Sample = Collected` | ❌ **جزئي**. لا enum حالة في `Culture`. `Record` بلا Guard. `RecordSensitivity` يفحص وجود Organism واحد على الأقل (وليس `Sample = Collected`). لا حالة "Pending" أو "WithSensitivity" في الكود. |
| 8.5 Receipt | Draft → Issued → PartiallyPaid → Paid مع Guards على كل انتقال | ❌ **الحالة كمفهوم غائبة**. لا حقل `Status` ولا enum لحالة الإيصال في `Receipt.cs`. `Issue` بلا Guard (قابل للاستدعاء مراراً). التمييز بين PartiallyPaid و Paid يُستنتج من `Remaining` فقط، لا من انتقال حالة معلن. |

### 5.4 عناصر أخرى مذكورة في الوثائق وغير موجودة داخل Domain

- **`PatientHistoryEntry`** كـ DTO موجود في `Common/DTOs/PatientHistoryEntry.cs` — لا سلوك فيه، ولا اختبارات مطلوبة على الدومين.
- **Domain Services**: كل `Services/*.cs` **Interfaces فقط** (`IPricingService`, `IAccountingService`, `ISampleTrackingService`, `IReceiptCalculationService`, `IResultValidationService`, ...). لا Implementations داخل `MasrLab.Domain`. أي ادعاء تنفيذي بخصوصها يخصّ طبقة أخرى.
- **`InsufficientPermissionException`, `DuplicateLabIdException`, `EntityNotFoundException`**: معرَّفة ولا تُرمى من أي كيان داخل Domain.
- **`SampleCollection` entity**: كيان تخزيني فقط (لا سلوك)، بينما `Sample.CollectionStatus` يحمل الحالة الفعلية — تكرار بيانات غير محكوم بـ Invariant.

---

## القسم 6 — قائمة تسليم مركّزة (Checklist للمراجع البشري)

- [ ] هل عدد حالات اختبار Value Objects يغطي كل شرط أعلاه (Age: 8، DateRange: 11، EgyptianPhone: 11)؟
- [ ] هل حالات Guards على الكيانات (VisitTest, CashTransaction, Comment, CommentTemplate, Doctor, Sensitivity, Receipt, OutsourcedSample) تشمل الحدود العليا والسفلى (0, 100, 1000, cost==patient)؟
- [ ] هل حالات State Machines تغطي كل Guard صريح فعلي (Sample: 4، PatientVisit: 12، OutsourcedSample: 5، Culture: 6، Receipt: 4 + عبور القسم 2.7)؟
- [ ] هل حالات Events تتحقق من: (أ) الإطلاق عند النجاح فقط، (ب) عدم الإطلاق عند فشل Guard، (ج) صحة الحمولة، (د) عمل `ClearDomainEvents` و read-only للمجموعة؟
- [ ] هل قسم "التناقضات المكتشفة" مُراجَع بند-بند قبل بدء أي اختبارات لأنه يحدد ما لا يجب اختباره (ادعاءات بلا كود)؟

---

## القسم 7 — قرارات مقترحة قبل كتابة الكود

1. **لا اختبارات لأحداث لم تُطلَق أبداً** (`PatientRegistered`, `PatientUpdated`, `VisitTestRemoved`, `TestResultEntered/Edited`, `CashDeposited/Withdrawn`, `CommentAttachedToResult`) — سجّلها كـ TODO في طبقة الدومين قبل كتابة أي فحص عليها.
2. **لا اختبارات لـ Invariants غير منفَّذة** (INV-01, 02, 03, 05, 12, 14, 16, 17)؛ إما تُنفَّذ أولاً أو تُنقل من التوثيق إلى قائمة القيود المستقبلية.
3. **ثغرة setters عامة على `OutsourcedSample.PatientPrice / CostPrice`**: قبل بناء INV-OS-*، اقترح تحويلها إلى `private set` أو حصر التعديل عبر `SetPrices` فقط.
4. **حالة `VisitStatus.Printed` بلا انتقال**: أضف اختباراً سلبياً واحداً يوثّق أن Visit لا يمكن أن يصل هذه الحالة إلا بتعيين خارجي (توثيق فجوة، ليس تحقق سلوك).
5. **`CultureRecorded(Id, Id)`**: التوقيع يمرّر `Id` مرتين — استوضح مع المطوّر: هل هذا خطأ يجب إصلاحه (تمرير `VisitTestId` الحقيقي) قبل اعتماد اختباره كسلوك مرجعي؟
