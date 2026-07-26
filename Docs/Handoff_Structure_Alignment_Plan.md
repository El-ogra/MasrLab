# خطة التنفيذ لمواءمة هيكل مشروع MasrLab مع القرارات التصميمية المعتمدة
## Handoff — Structure Alignment Execution Plan

> **نوع الوثيقة:** خطة عمل تنفيذية (Execution Handoff Plan) — موجَّهة لوكيل برمجي تنفيذي (سحابي أو محلي) أو لمطوّر بشري.
> **لم يُنفَّذ أي تعديل على الكود أثناء إعداد هذه الخطة.** الوثيقة تصف ما **يجب** تنفيذه، ولا تُوثّق شيئاً منفَّذاً بالفعل.

---

## 0. بطاقة الهوية (Document Identity Card)

| البند | القيمة |
|---|---|
| **المستودع** | `https://github.com/El-ogra/MasrLab.git` |
| **الفرع** | `development` |
| **Commit الأساس (Baseline)** | `e4aab3c34b255571d66c85289d6fd39da6ea8297` |
| **رسالة الـ Commit** | "إختبار الفرع الجديد" |
| **حالة التحقق من الـ Commit** | ✅ تم استنساخ المستودع والتحقق من أن `e4aab3c` هو أحدث Commit على `development` وقت إعداد هذه الخطة |
| **مصدر القرارات** | القرارات النهائية المعتمدة من صاحب المشروع: `DD-01` → `DD-10` |
| **مصدر قائمة الانحرافات** | تقرير `Examination_analysis_and_evaluation_of_the_projec_structure.md` — **قائمة الانحرافات D-01…D-24 ومساراتها فقط**، دون أي توصية أو رأي وارد فيه |
| **المرجع المعماري** | `Docs/implementation_plan.md` (924 سطراً) |
| **المرجع الوصفي** | `Docs/MasrLab_Specifications_and_Audit.md` (1245 سطراً) — خاصة الجزء الثالث "قرارات سد الفجوات" |
| **نطاق الخطة** | الهيكل المعماري (Project Structure) فقط — **لا منطق أعمال (Business Logic)** |
| **عدد الأجزاء (Parts)** | 9 أجزاء (Part 0 → Part 8) |
| **عدد الانحرافات المعالَجة** | 18 انحرافاً (بعد إلغاء 6 بقرار DD-05) |

---

## 1. تنبيه إلزامي لأي وكيل تنفيذي يستخدم هذه الخطة

اقرأ هذا القسم بالكامل قبل تنفيذ أي سطر:

1. **مصدر الحقيقة للقرارات هو قسم 2 من هذه الوثيقة (DD-01…DD-10) فقط.** أي رأي أو توصية أو "قرار مقترح" في تقرير التحليل السابق (`Examination_analysis_and_evaluation_of_the_projec_structure.md`) **ملغى ولا يُستشهد به إطلاقاً**. ذلك التقرير يُستخدَم فقط كفهرس لمسارات الانحرافات.
2. **ممنوع منعاً باتاً** إعادة فتح أي قرار تصميمي، أو اقتراح بديل له، أو تنفيذ "تحسين" غير مذكور صراحة في هذه الخطة.
3. **ممنوع** تنفيذ أي عملية دمج/تجميع لمجلدات `ViewModels/` أو `Views/` (`MasterData/`, `Administration/`, `Financial/`, `Settings/`) تحت أي ظرف — القرار DD-05 يلغي ذلك نهائياً.
4. **بوابة الجودة الإلزامية (Quality Gate)** بعد كل جزء وقبل الانتقال للجزء التالي:
   ```bash
   dotnet build   # من جذر الحل — نجاح دون أي خطأ ودون أي تحذير جديد
   dotnet test    # من جذر الحل — نجاح جميع الاختبارات دون استثناء
   ```
   عند فشل أي منهما: **يُمنع الانتقال للجزء التالي**، ويجب الإصلاح داخل نفس الجزء.
5. **كل جزء = Commit واحد مستقل** قابل لـ `git revert` دون كسر الأجزاء اللاحقة.
6. **DD-10 إلزامي:** أي جزء يغيّر الهيكل الفعلي يجب أن يُحدِّث `Docs/implementation_plan.md` في **نفس الـ Commit**، بحيث لا يوجد لحظة واحدة يكون فيها ملف الخطة مخالفاً للواقع.
7. **أرقام الأسطر** المذكورة في هذه الوثيقة صالحة عند Commit `e4aab3c` فقط. بعد أول تعديل تتزحزح الأرقام — لذلك **طبّق التعديلات بمطابقة النص (Content Anchor) لا برقم السطر**.

---

## 2. القرارات المعتمدة (مرجع سريع — غير قابلة للنقاش)

| # | القرار | الخلاصة التنفيذية | الجزء المنفِّذ |
|---|---|---|---|
| **DD-01** | اسم مشروع Presentation | الإبقاء على `MasrLab.csproj` — **لا إعادة تسمية**؛ توثيق فقط | Part 1 |
| **DD-02** | `SampleDto.cs` | الإبقاء على الملف + إضافته رسمياً للخطة | Part 1 |
| **DD-03** | `PatientHistoryView` | اعتماد الصيغتين: `.sql` (View جديد كـ Placeholder) + `.cs` (keyless entity) | Part 4 |
| **DD-04** | تسمية Converter | الإبقاء على `BooleanToVisibilityConverter.cs` + تصحيح الخطة | Part 1 |
| **DD-05** | تنظيم ViewModels/Views | **Feature-per-Module** كما هو فعلياً — **إلغاء D-13, D-14, D-15, D-17, D-18, D-19 نهائياً** + تحديث الخطة | Part 1 |
| **DD-06** | مجلد Migrations | `Persistence/Migrations/.gitkeep` فقط — **بلا توليد Migration** | Part 2 |
| **DD-07** | ملفات Styles | 4 ملفات Skeleton فارغة (ResourceDictionary بلا محتوى) | Part 3 |
| **DD-08** | LabIdGenerator / AgeCalculator | `LabIdGenerator` → `Application/Common/Helpers/` · `AgeCalculator` → `Domain/Common/` · **ممنوع** إنشاؤهما في `Presentation/Helpers/` | Part 6 و Part 7 |
| **DD-09** | `appsettings.json` | يبقى في Presentation + توثيقه رسمياً | Part 1 |
| **DD-10** | سياسة تحديث الخطة | تحديث `implementation_plan.md` فوراً مع كل تغيير | كل الأجزاء |

---

## 3. مصفوفة الانحرافات → الأجزاء (Deviation Traceability Matrix)

| الانحراف | الوصف المختصر | القرار الحاكم | المصير | الجزء |
|---|---|---|---|---|
| **D-01** | `MasrLab.csproj` بدل `MasrLab.Presentation.csproj` | DD-01 | توثيق فقط (بلا تغيير كود) | Part 1 |
| **D-02** | `SampleDto.cs` غير مذكور في الخطة | DD-02 | توثيق فقط | Part 1 |
| **D-03** | `PatientHistoryView.cs` بدل `.sql` | DD-03 | إنشاء `.sql` + إبقاء `.cs` | **Part 4** |
| **D-04** | `Persistence/Migrations/` مفقود | DD-06 | إنشاء مجلد + `.gitkeep` | **Part 2** |
| **D-05** | `ButtonStyles.xaml` مفقود | DD-07 | إنشاء Skeleton | **Part 3** |
| **D-06** | `TextBoxStyles.xaml` مفقود | DD-07 | إنشاء Skeleton | **Part 3** |
| **D-07** | `DataGridStyles.xaml` مفقود | DD-07 | إنشاء Skeleton | **Part 3** |
| **D-08** | `Colors.xaml` مفقود | DD-07 | إنشاء Skeleton | **Part 3** |
| **D-09** | `Resources/Icons/` مفقود | — | إنشاء مجلد + `.gitkeep` | **Part 2** |
| **D-10** | `Resources/Images/` مفقود | — | إنشاء مجلد + `.gitkeep` | **Part 2** |
| **D-11** | `Resources/Fonts/` مفقود | — | إنشاء مجلد + `.gitkeep` | **Part 2** |
| **D-12** | `BooleanToVisibilityConverter` vs `BoolToVisibilityConverter` | DD-04 | توثيق فقط (تصحيح الخطة) | Part 1 |
| **D-13** | `ViewModels/MasterData/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-14** | `ViewModels/Administration/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-15** | `ViewModels/Financial/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-16** | `ViewModels/Settings/` vs `SystemSettings/` | DD-05 | توثيق فقط (الخطة تتبع الواقع) | Part 1 |
| **D-17** | `Views/MasterData/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-18** | `Views/Administration/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-19** | `Views/Financial/` | DD-05 | ❌ **ملغى — Not a Deviation** | Part 1 (شطب) |
| **D-20** | `Views/Settings/` vs `SystemSettings/` | DD-05 | توثيق فقط (الخطة تتبع الواقع) | Part 1 |
| **D-21** | `Behaviors/RtlBehavior.cs` مفقود | — | إنشاء الملف | **Part 5** |
| **D-22** | `LabIdGenerator` مفقود | DD-08 | إنشاء في **Application** | **Part 7** |
| **D-23** | `AgeCalculator` مفقود | DD-08 | إنشاء في **Domain** | **Part 6** |
| **D-24** | `appsettings.json` غير موثّق | DD-09 | توثيق فقط | Part 1 |

**الإحصاء:** 24 انحرافاً مسجّلاً − 6 ملغاة (DD-05) = **18 انحرافاً صالحاً**، منها **8 تتطلب تغييراً في الملفات** و**10 تُعالَج بتحديث الوثيقة فقط**.

---

## 4. ترتيب التنفيذ ومنحنى الخطورة

| الجزء | العنوان | طبيعة التغيير | الخطورة | أثر على الـ Build |
|---|---|---|---|---|
| **Part 0** | التحقق من الأساس والتقاط خط الأساس | لا تغيير (قراءة فقط) | ⚪ صفر | لا |
| **Part 1** | مواءمة الوثيقة فقط (10 انحرافات) | Markdown فقط | 🟢 منخفضة جداً | لا |
| **Part 2** | المجلدات الهيكلية الفارغة (D-04, D-09…D-11) | ملفات `.gitkeep` | 🟢 منخفضة | لا |
| **Part 3** | هياكل Styles الأربعة (D-05…D-08) | XAML + `App.xaml` | 🟡 منخفضة–متوسطة | **نعم** (Page compile) |
| **Part 4** | `PatientHistoryView.sql` (D-03) | ملف SQL غير مُصرَّف | 🟢 منخفضة | لا |
| **Part 5** | `RtlBehavior.cs` (D-21) | C# في Presentation | 🟡 متوسطة | **نعم** |
| **Part 6** | `AgeCalculator` في Domain (D-23) | C# في Domain | 🟠 متوسطة | **نعم** (طبقة أساس) |
| **Part 7** | `LabIdGenerator` في Application (D-22) | C# في Application + تبعية Repository | 🔴 الأعلى | **نعم** |
| **Part 8** | التدقيق النهائي وإغلاق الملف | تحقق + Markdown | 🟢 منخفضة | لا |

**مبرر الترتيب:** يبدأ بما لا يمسّ الكود إطلاقاً (وثيقة)، ثم بما لا يدخل في التصريف (`.gitkeep`, `.sql`)، ثم بأصول XAML، ثم بكود C# صعوداً من الطبقة الأعلى (Presentation) نحو الطبقات التي تعتمد عليها بقية الطبقات (Domain ثم Application مع تبعية Repository) — فأي كسر في الطبقات الدنيا يُكتشف متأخراً وبأقل عدد ممكن من التغييرات المتراكمة قبله.

---

# Part 0 — التحقق من الأساس والتقاط خط الأساس (Baseline Capture)

> **الغرض:** ضمان أن التنفيذ يبدأ من نفس الحالة التي بُنيت عليها هذه الخطة، والتقاط قائمة التحذيرات الحالية لأن كل الأجزاء التالية مقيَّدة بشرط **"دون أي تحذير جديد"** — وهذا الشرط لا معنى له بدون خط أساس موثّق.
> **الخطورة:** ⚪ صفر — لا تعديل على أي ملف.

## 0.1 الملفات المتأثرة
لا شيء. (يُسمَح بإنشاء ملف مؤقت خارج المستودع فقط: `baseline_warnings.txt`، **ولا يُضاف إلى Git**.)

## 0.2 خطوات التنفيذ

1. استنساخ المستودع والانتقال للفرع والـ Commit المحدد:
   ```bash
   git clone https://github.com/El-ogra/MasrLab.git
   cd MasrLab
   git checkout development
   git log -1 --format='%H %s'
   ```
2. **التحقق الإلزامي:** يجب أن يكون الناتج بالضبط:
   ```
   e4aab3c34b255571d66c85289d6fd39da6ea8297  إختبار الفرع الجديد
   ```
   ⛔ إن اختلف الـ Hash: **توقّف فوراً** ولا تنفّذ أي جزء، وأبلغ صاحب المشروع بأن المستودع تغيّر عن الأساس الذي بُنيت عليه الخطة.
3. التأكد من نظافة شجرة العمل: `git status --porcelain` يجب أن يكون فارغاً.
4. التقاط خط أساس البناء والتحذيرات:
   ```bash
   dotnet restore
   dotnet build   2>&1 | tee ../baseline_warnings.txt
   dotnet test    2>&1 | tee ../baseline_tests.txt
   ```
5. استخراج التحذيرات وعدّها وحفظها كمرجع:
   ```bash
   grep -E "warning [A-Z]+[0-9]+" ../baseline_warnings.txt | sort -u
   ```
6. تسجيل الأرقام التالية في مذكرة العمل (ستُقارَن بعد كل جزء):
   - عدد الأخطاء (يجب أن يكون **0**).
   - العدد والقائمة الفريدة لأكواد التحذيرات (`CSxxxx` / `MSBxxxx` / `NETSDKxxxx`).
   - عدد الاختبارات الناجحة/الفاشلة/المتخطّاة في المشاريع الثلاثة: `MasrLab.Domain.Tests`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

> **ملاحظة بيئية مهمة:** مشروع العرض يستهدف `net8.0-windows` مع `UseWPF=true`، أي أن `dotnet build` للحل بأكمله **يتطلب بيئة Windows** (أو Windows Runner في CI). إن كانت بيئة التنفيذ Linux/macOS فلن يُبنى مشروع WPF. في تلك الحالة **يُمنع** تنفيذ Parts 3 و5 (المتعلقة بـ Presentation) قبل تأمين بيئة Windows، لأن بوابة الجودة لن تكون قابلة للتحقق. لا تُسقِط البوابة ولا تستبدلها ببناء جزئي.

## 0.3 تحديث `implementation_plan.md`
لا يوجد — هذا الجزء لا يغيّر الواقع.

## 0.4 معايير التحقق (Acceptance Criteria)
- [ ] الـ Commit المُتحقَّق منه هو `e4aab3c34b255571d66c85289d6fd39da6ea8297`.
- [ ] `git status --porcelain` فارغ.
- [ ] `dotnet build` نجح بـ **0 Errors**.
- [ ] `dotnet test` نجح لجميع مشاريع الاختبار الثلاثة.
- [ ] قائمة التحذيرات الأساسية موثّقة نصياً ومحفوظة **خارج** المستودع.

## 0.5 الـ Commit
لا يوجد Commit لهذا الجزء (لا تغييرات). يُنشأ فرع عمل فقط:
```bash
git checkout -b feature/structure-alignment
```

---

# Part 1 — مواءمة الوثيقة مع الواقع (Documentation-Only Alignment)

> **الغرض:** جعل `Docs/implementation_plan.md` مطابقاً للهيكل الفعلي في كل بند لا يتطلب تغيير كود، وشطب الانحرافات الملغاة بقرار DD-05.
> **يعالج:** D-01, D-02, D-12, D-13, D-14, D-15, D-16, D-17, D-18, D-19, D-20, D-24 (12 بنداً، منها 6 شطب).
> **الخطورة:** 🟢 منخفضة جداً — لا يُمسّ أي ملف كود، ولا أثر على البناء.
> **لماذا أولاً؟** لأنه يوثّق القرار المعماري الأهم (DD-05) قبل أي عمل، فيصبح ملف الخطة حاجزاً يمنع أي وكيل لاحق من تنفيذ الدمج الملغى بالخطأ.

## 1.1 الملفات المتأثرة
- `Docs/implementation_plan.md` — **الملف الوحيد.**

⛔ **ممنوع في هذا الجزء:** لمس أي ملف داخل `src/` أو `tests/`.

## 1.2 خطوات التنفيذ التفصيلية

### الخطوة 1-A — توثيق اسم مشروع Presentation (D-01 / DD-01)
في قسم "هيكل الحل (Solution Structure)" — السطر ~65:

**النص الحالي:**
```
│   └── MasrLab.Presentation/                    ← طبقة العرض (Presentation Layer — WPF)
```
**النص البديل:**
```
│   └── MasrLab.Presentation/                    ← طبقة العرض (Presentation Layer — WPF)
│       └── MasrLab.csproj                       ← اسم ملف المشروع واسم الـ Assembly الناتج = MasrLab
│                                                   (مُعتمَد بقرار DD-01 — اسم المجلد فقط هو MasrLab.Presentation)
```

### الخطوة 1-B — توثيق `SampleDto.cs` (D-02 / DD-02)
في شجرة `MasrLab.Application` → `Common/DTOs/` — السطر ~197:

**النص الحالي:**
```
│   │   ├── StatisticsDto.cs
```
**النص البديل:**
```
│   │   ├── SampleDto.cs                         ← DTO للعينة (Entity 9: Sample) — مُعتمَد بقرار DD-02
│   │   ├── StatisticsDto.cs
```

### الخطوة 1-C — تصحيح اسم الـ Converter (D-12 / DD-04)
في شجرة `MasrLab.Presentation` → `Resources/Converters/` — السطر ~469:

**النص الحالي:**
```
│   │   ├── BoolToVisibilityConverter.cs
```
**النص البديل:**
```
│   │   ├── BooleanToVisibilityConverter.cs      ← الاسم المعتمد بقرار DD-04 (بلا اختصار)
```

### الخطوة 1-D — استبدال تجميعات ViewModels بالتنظيم الفعلي (D-13, D-14, D-15, D-16 / DD-05)
في شجرة `MasrLab.Presentation` → `ViewModels/` — الأسطر ~517–537.

**الكتلة الحالية بالكامل (تُحذَف):**
```
│   ├── MasterData/                              ← Modules 10–14 (البيانات الرئيسية)
│   │   ├── TestsMasterDataViewModel.cs          ← Module 10
│   │   ├── PriceListsViewModel.cs               ← Module 11
│   │   ├── FixedCommentsViewModel.cs            ← Module 12
│   │   ├── TestGroupsViewModel.cs               ← Module 13
│   │   └── DoctorsReferralsViewModel.cs         ← Module 14
│   │
│   ├── Administration/                          ← Modules 15–16 (الإدارة)
│   │   ├── UsersPermissionsViewModel.cs         ← Module 15
│   │   └── AttendanceAuditViewModel.cs          ← Module 16
│   │
│   ├── Financial/                               ← Modules 17–19 (المالية)
│   │   ├── PeriodDrawerViewModel.cs             ← Module 17
│   │   ├── DoctorReferralDrawerViewModel.cs     ← Module 18
│   │   └── AccountTypeDrawerViewModel.cs        ← Module 19
│   │
│   ├── Statistics/                              ← Module 20
│   │   └── StatisticsViewModel.cs
│   │
│   └── Settings/                                ← Module 21
│       └── SystemSettingsViewModel.cs
```

**الكتلة البديلة (تُطابق الواقع حرفياً):**
```
│   ├── TestsMasterData/                         ← Module 10
│   │   └── TestsMasterDataViewModel.cs
│   │
│   ├── PriceLists/                              ← Module 11
│   │   └── PriceListsViewModel.cs
│   │
│   ├── FixedComments/                           ← Module 12
│   │   └── FixedCommentsViewModel.cs
│   │
│   ├── TestGroups/                              ← Module 13
│   │   └── TestGroupsViewModel.cs
│   │
│   ├── DoctorsAndReferrals/                     ← Module 14
│   │   └── DoctorsReferralsViewModel.cs
│   │
│   ├── UsersAndPermissions/                     ← Module 15
│   │   └── UsersPermissionsViewModel.cs
│   │
│   ├── AttendanceAndAudit/                      ← Module 16
│   │   └── AttendanceAuditViewModel.cs
│   │
│   ├── Accounting/                              ← Modules 17–19 (الأدراج المالية)
│   │   ├── PeriodDrawerViewModel.cs             ← Module 17
│   │   ├── DoctorReferralDrawerViewModel.cs     ← Module 18
│   │   └── AccountTypeDrawerViewModel.cs        ← Module 19
│   │
│   ├── Statistics/                              ← Module 20
│   │   └── StatisticsViewModel.cs
│   │
│   └── SystemSettings/                          ← Module 21
│       └── SystemSettingsViewModel.cs
```

### الخطوة 1-E — استبدال تجميعات Views بالتنظيم الفعلي (D-17, D-18, D-19, D-20 / DD-05)
في شجرة `MasrLab.Presentation` → `Views/` — الأسطر ~568–584.

**الكتلة الحالية بالكامل (تُحذَف):**
```
│   ├── MasterData/
│   │   ├── TestsMasterDataView.xaml
│   │   ├── PriceListsView.xaml
│   │   ├── FixedCommentsView.xaml
│   │   ├── TestGroupsView.xaml
│   │   └── DoctorsReferralsView.xaml
│   ├── Administration/
│   │   ├── UsersPermissionsView.xaml
│   │   └── AttendanceAuditView.xaml
│   ├── Financial/
│   │   ├── PeriodDrawerView.xaml
│   │   ├── DoctorReferralDrawerView.xaml
│   │   └── AccountTypeDrawerView.xaml
│   ├── Statistics/
│   │   └── StatisticsView.xaml
│   └── Settings/
│       └── SystemSettingsView.xaml
```

**الكتلة البديلة:**
```
│   ├── TestsMasterData/
│   │   └── TestsMasterDataView.xaml
│   ├── PriceLists/
│   │   └── PriceListsView.xaml
│   ├── FixedComments/
│   │   └── FixedCommentsView.xaml
│   ├── TestGroups/
│   │   └── TestGroupsView.xaml
│   ├── DoctorsAndReferrals/
│   │   └── DoctorsReferralsView.xaml
│   ├── UsersAndPermissions/
│   │   └── UsersPermissionsView.xaml
│   ├── AttendanceAndAudit/
│   │   └── AttendanceAuditView.xaml
│   ├── Accounting/
│   │   ├── PeriodDrawerView.xaml
│   │   ├── DoctorReferralDrawerView.xaml
│   │   └── AccountTypeDrawerView.xaml
│   ├── Statistics/
│   │   └── StatisticsView.xaml
│   └── SystemSettings/
│       └── SystemSettingsView.xaml
```

### الخطوة 1-F — توثيق `appsettings.json` (D-24 / DD-09)
في شجرة `MasrLab.Presentation`، مباشرة قبل السطر الأخير `└── DependencyInjection.cs` (~618):

**النص الحالي:**
```
└── DependencyInjection.cs                       ← تسجيل خدمات العرض في DI
```
**النص البديل:**
```
├── appsettings.json                             ← ملف التهيئة (ConnectionStrings + LabSettings)
│                                                   — يبقى في طبقة العرض بقرار DD-09 (نقطة الدخول تقرأ التهيئة)
│
└── DependencyInjection.cs                       ← تسجيل خدمات العرض في DI
```

### الخطوة 1-G — تثبيت المبدأ المعماري لـ DD-05 نصياً (إلزامي)
في فقرة **"تبرير التصنيف"** أسفل شجرة `MasrLab.Presentation` (~السطر 622):

**النص الحالي:**
```
- `ViewModels/` و `Views/` يتبعان نفس الهيكل التنظيمي المقسّم حسب الموديولات لتسهيل التنقل.
```
**النص البديل:**
```
- `ViewModels/` و `Views/` يتبعان سياسة **Feature-per-Module** الصارمة: مجلد مستقل لكل موديول،
  بتسمية مطابقة حرفياً لتسميات `MasrLab.Application/Features/`، **بلا أي تجميع موضوعي**
  (لا `MasterData/` ولا `Administration/` ولا `Financial/` ولا `Settings/`) — مُعتمَد بقرار **DD-05**.
- **المبرر المعماري لـ DD-05:** النافذة الرئيسية تعمل بمنطق تصفّح على مستويين مستقلَّين:
  (1) الضغط على أيقونة في الشريط العلوي يُظهر قائمة أزرار في المنطقة المركزية فقط (تنقّل بسيط لا يستدعي دمج ViewModels)،
  (2) الضغط على أي زر يفتح **نافذة مستقلة تماماً** خاصة بذلك الموديول وحده، مع إخفاء كامل للنافذة الرئيسية والشريط العلوي.
  وبما أنه لا توجد شاشة تجميعية ولا تبويبات مشتركة تجمع عدة موديولات في View واحد،
  فإن استقلال كل موديول بـ View/ViewModel خاص هو الانعكاس الصحيح الوحيد لهذا السلوك.
- ⛔ **قاعدة مُلزِمة لأي وكيل أو مطوّر لاحق:** أي اقتراح بدمج مجلدات ViewModels/Views موضوعياً
  **مرفوض مسبقاً** ولا يُعاد طرحه.
```

### الخطوة 1-H — ملاحظة توثيقية عن عدد المجلدات (توضيح رقمي، ليس قراراً)
> عند صياغة DD-05 وردت الإشارة إلى "11 مجلداً". التطابق الرقمي مع الواقع كالتالي، ويجب توثيقه في نفس الفقرة لمنع أي التباس لاحق:
> - التجميعات الأربع في الخطة القديمة (`MasterData`, `Administration`, `Financial`, `Settings`) كانت تضم **11 ViewModel** بالضبط (5 + 2 + 3 + 1).
> - هذه الـ 11 موزّعة فعلياً على **9 مجلدات مستقلة**.
> - إجمالي مجلدات الموديولات فعلياً = **19 مجلداً** في `ViewModels/` و**19 مجلداً** في `Views/`، مطابقة لـ **19 مجلداً** في `Application/Features/`.
>
> **هذه ملاحظة تحقق رقمي فقط — مبدأ DD-05 (Feature-per-Module بلا دمج) يُطبَّق كما هو ولا يتغير.**

يُضاف السطر التالي في فقرة التبرير:
```
- التطابق العددي: 19 مجلد موديول في `ViewModels/` = 19 مجلد موديول في `Views/` = 19 مجلداً في `Application/Features/`.
```

## 1.3 تحديث `implementation_plan.md`
**هذا الجزء بالكامل هو تحديث لملف الخطة** — لا يوجد تحديث إضافي منفصل.

## 1.4 معايير التحقق
- [ ] لا وجود لأي من السلاسل التالية في `Docs/implementation_plan.md` داخل شجرتي `ViewModels/` و`Views/`:
  ```bash
  grep -n "MasterData/\|Administration/\|Financial/\|BoolToVisibilityConverter" Docs/implementation_plan.md
  ```
  (النتائج المسموح بها فقط: `Financial/` داخل شجرتي **Domain/Entities** و**Infrastructure/Configurations** — وهما صحيحان ولا يُمسّان.)
- [ ] `SampleDto.cs` و`appsettings.json` و`MasrLab.csproj` مذكورة في الخطة.
- [ ] `BooleanToVisibilityConverter.cs` هو الاسم الوحيد الوارد.
- [ ] نص DD-05 ومبرره المعماري موجود حرفياً في فقرة التبرير.
- [ ] `git diff --stat` يُظهر **ملفاً واحداً فقط** معدّلاً: `Docs/implementation_plan.md`.
- [ ] ✅ **بوابة الجودة:** `dotnet build` بلا أخطاء وبلا تحذيرات جديدة + `dotnet test` ناجح بالكامل.

## 1.5 الـ Commit
```bash
git add Docs/implementation_plan.md
git commit -m "docs(plan): مواءمة implementation_plan مع الواقع وفق DD-01/02/04/05/09 وإلغاء D-13..D-19"
```
**Rollback:** `git revert <hash>` — لا أثر على أي كود.

---

# Part 2 — المجلدات الهيكلية الفارغة (D-04, D-09, D-10, D-11)

> **الغرض:** استكمال المجلدات المنصوص عليها في الخطة والمفقودة فعلياً، بإنشائها فارغة مع `.gitkeep` (Git لا يتتبّع المجلدات الفارغة).
> **الخطورة:** 🟢 منخفضة — لا ملف مُصرَّف، ولا تعديل على أي `.csproj`.

## 2.1 الملفات المتأثرة (إنشاء فقط)
| # | المسار الكامل | الانحراف |
|---|---|---|
| 1 | `src/MasrLab.Infrastructure/Persistence/Migrations/.gitkeep` | D-04 (DD-06) |
| 2 | `src/MasrLab.Presentation/Resources/Icons/.gitkeep` | D-09 |
| 3 | `src/MasrLab.Presentation/Resources/Images/.gitkeep` | D-10 |
| 4 | `src/MasrLab.Presentation/Resources/Fonts/.gitkeep` | D-11 |

بالإضافة إلى: `Docs/implementation_plan.md` (تحديث مصاحب).

## 2.2 خطوات التنفيذ التفصيلية

1. إنشاء المجلدات الأربعة وملفات `.gitkeep` (محتواها **فارغ تماماً**، بلا BOM):
   ```bash
   mkdir -p src/MasrLab.Infrastructure/Persistence/Migrations
   mkdir -p src/MasrLab.Presentation/Resources/Icons
   mkdir -p src/MasrLab.Presentation/Resources/Images
   mkdir -p src/MasrLab.Presentation/Resources/Fonts

   : > src/MasrLab.Infrastructure/Persistence/Migrations/.gitkeep
   : > src/MasrLab.Presentation/Resources/Icons/.gitkeep
   : > src/MasrLab.Presentation/Resources/Images/.gitkeep
   : > src/MasrLab.Presentation/Resources/Fonts/.gitkeep
   ```
2. ⛔ **ممنوع في هذا الجزء:**
   - تنفيذ `dotnet ef migrations add` أو أي أمر EF Core يولّد Migration (نصّ DD-06 صراحة على عدم توليد أي Migration الآن).
   - إضافة أي أيقونة أو صورة أو خط فعلي (اختيار الأصول يخص مرحلة تصميم الواجهة).
   - تعديل `MasrLab.csproj` أو `MasrLab.Infrastructure.csproj` — لا حاجة إليه: مشاريع SDK-style تتعامل مع هذه المجلدات دون تسجيل، وملفات `.gitkeep` بلا امتداد لا تدخل التصريف.
3. التأكد أن `.gitignore` لا يستبعد هذه الملفات:
   ```bash
   git check-ignore -v src/MasrLab.Presentation/Resources/Icons/.gitkeep
   ```
   يجب ألا يُرجِع أي ناتج. (`.gitignore` الحالي يستبعد `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `packages/`, `TestResults/`, `*.log`, `appsettings.Local.json` — ولا شيء منها ينطبق.)

## 2.3 تحديث `implementation_plan.md` المصاحب (DD-10)

**(أ) شجرة Infrastructure — السطر ~420:**
```
│   ├── Migrations/                              ← EF Core Migrations
```
→
```
│   ├── Migrations/                              ← EF Core Migrations (مجلد محجوز — يحتوي .gitkeep فقط)
│   │                                               لم يُولَّد أي Migration بعد — مؤجَّل بقرار DD-06
│   │                                               حتى اكتمال جميع Fluent API Configurations
```

**(ب) شجرة Presentation — الأسطر ~463، 464، 471:**
```
│   ├── Icons/                                   ← أيقونات الشاشة الرئيسية (9 أيقونات — القسم 7.14)
│   ├── Images/                                  ← صور (الشعار، رأس التقرير)
```
→
```
│   ├── Icons/                                   ← أيقونات الشاشة الرئيسية (9 أيقونات — القسم 7.14)
│   │                                               مجلد محجوز حالياً (.gitkeep) — الأصول تُضاف في مرحلة الواجهة
│   ├── Images/                                  ← صور (الشعار، رأس التقرير)
│   │                                               مجلد محجوز حالياً (.gitkeep)
```
و:
```
│   └── Fonts/                                   ← خطوط عربية
```
→
```
│   └── Fonts/                                   ← خطوط عربية — مجلد محجوز حالياً (.gitkeep)
```

## 2.4 معايير التحقق
- [ ] المجلدات الأربعة موجودة وكل منها يحوي `.gitkeep` فقط:
  ```bash
  find src -name ".gitkeep" | sort
  ```
  الناتج المتوقّع: 4 مسارات بالضبط (المذكورة في 2.1).
- [ ] كل ملف `.gitkeep` حجمه **0 بايت**.
- [ ] لا يوجد أي ملف `*.Designer.cs` أو `*_Migration.cs` أو `MasrLabDbContextModelSnapshot.cs` داخل `Migrations/`:
  ```bash
  ls -A src/MasrLab.Infrastructure/Persistence/Migrations/
  ```
  الناتج: `.gitkeep` فقط.
- [ ] `git status --porcelain` يُظهر 4 ملفات جديدة + `Docs/implementation_plan.md` معدَّلاً — لا غير.
- [ ] ✅ **بوابة الجودة:** `dotnet build` (0 أخطاء، 0 تحذيرات جديدة) + `dotnet test` ناجح.

## 2.5 الـ Commit
```bash
git add src Docs/implementation_plan.md
git commit -m "chore(structure): إنشاء مجلدات Migrations/Icons/Images/Fonts بـ .gitkeep وفق DD-06 (D-04, D-09, D-10, D-11)"
```
**Rollback:** `git revert <hash>` — حذف 4 ملفات فارغة، بلا أثر على البناء.

---

# Part 3 — هياكل ملفات Styles الأربعة (D-05, D-06, D-07, D-08)

> **الغرض:** إنشاء ملفات الأنماط المنصوص عليها في الخطة كـ **Skeleton فارغ** (ResourceDictionary بلا أي محتوى)، تنفيذاً حرفياً لـ DD-07.
> **الخطورة:** 🟡 منخفضة–متوسطة — أول جزء يؤثر فعلياً على التصريف (ملفات XAML تُصرَّف كـ `Page`، وتحميلها في `App.xaml` يُتحقَّق منه وقت التشغيل أيضاً).

## 3.1 الملفات المتأثرة
**إنشاء:**
| # | المسار الكامل | الانحراف |
|---|---|---|
| 1 | `src/MasrLab.Presentation/Resources/Styles/Colors.xaml` | D-08 |
| 2 | `src/MasrLab.Presentation/Resources/Styles/ButtonStyles.xaml` | D-05 |
| 3 | `src/MasrLab.Presentation/Resources/Styles/TextBoxStyles.xaml` | D-06 |
| 4 | `src/MasrLab.Presentation/Resources/Styles/DataGridStyles.xaml` | D-07 |

**تعديل:**
- `src/MasrLab.Presentation/App.xaml` (تسجيل القواميس في `MergedDictionaries`)
- `Docs/implementation_plan.md`

**مرجع الحالة الفعلية:** `Resources/Styles/GlobalStyles.xaml` موجود بالفعل وهو نفسه `ResourceDictionary` فارغ، ومُسجَّل حالياً وحيداً في `App.xaml`. الملفات الأربعة الجديدة تتبع **نفس النمط حرفياً**.

## 3.2 خطوات التنفيذ التفصيلية

1. إنشاء الملفات الأربعة بمحتوى موحّد ومطابق لأسلوب `GlobalStyles.xaml` الحالي (بلا BOM، بلا أي Style/Color بداخلها):

**`Colors.xaml`:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- ألوان النظام (متطلب 20 — قابلة للتخصيص). هيكل فارغ بقرار DD-07 — يُملأ في مرحلة تصميم الواجهة. -->
</ResourceDictionary>
```
**`ButtonStyles.xaml`:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- أنماط الأزرار القياسية (إضافة/حفظ/تعديل/طباعة). هيكل فارغ بقرار DD-07. -->
</ResourceDictionary>
```
**`TextBoxStyles.xaml`:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- أنماط حقول الإدخال. هيكل فارغ بقرار DD-07. -->
</ResourceDictionary>
```
**`DataGridStyles.xaml`:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- أنماط جداول العرض. هيكل فارغ بقرار DD-07. -->
</ResourceDictionary>
```

> **ملاحظة تنفيذية:** التعليق `<!-- ... -->` مسموح ولا يُعدّ "محتوى" بالمعنى المقصود في DD-07 (لا ألوان ولا أنماط). إن فضّل المنفّذ ملفات بلا تعليق إطلاقاً فذلك مقبول أيضاً — النتيجة الملزِمة هي: **`ResourceDictionary` بلا أي `Style` أو `Color` أو `Brush`.**

2. تحديث `src/MasrLab.Presentation/App.xaml` — **الترتيب مهم**: `Colors.xaml` أولاً (لأن بقية القواميس ستستهلك ألوانه مستقبلاً)، ثم `GlobalStyles.xaml`، ثم الأنماط المتخصصة:

**الحالي:**
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/GlobalStyles.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```
**البديل:**
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/Colors.xaml"/>
            <ResourceDictionary Source="Resources/Styles/GlobalStyles.xaml"/>
            <ResourceDictionary Source="Resources/Styles/ButtonStyles.xaml"/>
            <ResourceDictionary Source="Resources/Styles/TextBoxStyles.xaml"/>
            <ResourceDictionary Source="Resources/Styles/DataGridStyles.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```
⚠️ **لا تُغيّر** سطر `x:Class="MasrLab.Presentation.App"` ولا الـ `xmlns` القائمة.

3. ⛔ **ممنوع:** إضافة أي `<Style>`، `<SolidColorBrush>`، `<Color>`، `<FontFamily>` أو `TargetType` داخل الملفات الأربعة. أي لون افتراضي مُقحَم = مخالفة صريحة لـ DD-07.
4. ⛔ **ممنوع:** تسجيل الملفات يدوياً في `MasrLab.csproj` — مشروع WPF بنمط SDK يلتقط `**/*.xaml` تلقائياً كـ `Page`. التسجيل اليدوي يسبب خطأ التكرار `MSB3105 / duplicate items`.

## 3.3 تحديث `implementation_plan.md` المصاحب (DD-10)
في شجرة Presentation → `Resources/Styles/` (~الأسطر 457–462):
```
│   ├── Styles/                                  ← أنماط CSS/XAML العامة
│   │   ├── GlobalStyles.xaml                    ← الأنماط العامة (RTL, Fonts)
│   │   ├── ButtonStyles.xaml                    ← أنماط الأزرار القياسية (إضافة/حفظ/تعديل/طباعة)
│   │   ├── TextBoxStyles.xaml
│   │   ├── DataGridStyles.xaml
│   │   └── Colors.xaml                          ← ألوان النظام (قابلة للتخصيص — متطلب 20)
```
→
```
│   ├── Styles/                                  ← أنماط XAML العامة
│   │   │                                           جميعها ResourceDictionary فارغة (Skeleton) بقرار DD-07،
│   │   │                                           ومُسجَّلة في App.xaml بترتيب: Colors → Global → Button → TextBox → DataGrid
│   │   ├── Colors.xaml                          ← ألوان النظام (قابلة للتخصيص — متطلب 20)
│   │   ├── GlobalStyles.xaml                    ← الأنماط العامة (RTL, Fonts)
│   │   ├── ButtonStyles.xaml                    ← أنماط الأزرار القياسية (إضافة/حفظ/تعديل/طباعة)
│   │   ├── TextBoxStyles.xaml                   ← أنماط حقول الإدخال
│   │   └── DataGridStyles.xaml                  ← أنماط جداول العرض
```

## 3.4 معايير التحقق
- [ ] وجود 5 ملفات في `Resources/Styles/`:
  ```bash
  ls src/MasrLab.Presentation/Resources/Styles/
  # Colors.xaml ButtonStyles.xaml DataGridStyles.xaml GlobalStyles.xaml TextBoxStyles.xaml
  ```
- [ ] لا يحتوي أي من الملفات الأربعة الجديدة على `<Style` أو `<Color` أو `<SolidColorBrush`:
  ```bash
  grep -l "<Style\|<Color\|<SolidColorBrush" src/MasrLab.Presentation/Resources/Styles/*.xaml
  ```
  الناتج المتوقع: **فارغ**.
- [ ] `App.xaml` يحوي 5 مُدخلات `MergedDictionaries` بالترتيب المحدد.
- [ ] ✅ **بوابة الجودة (على بيئة Windows):** `dotnet build` — 0 أخطاء، **0 تحذيرات جديدة** مقارنة بخط أساس Part 0 (انتبه خصوصاً لتحذيرات XAML مثل `MC3074` أو `XLS0414` — أي منها = فشل الجزء). ثم `dotnet test` ناجح.
- [ ] (تحقق إضافي مُستحسن) تشغيل التطبيق مرة واحدة والتأكد من عدم رمي `XamlParseException` عند الإقلاع بسبب مسار قاموس خاطئ.

## 3.5 الـ Commit
```bash
git add src/MasrLab.Presentation Docs/implementation_plan.md
git commit -m "feat(styles): إضافة هياكل Colors/Button/TextBox/DataGrid وتسجيلها في App.xaml وفق DD-07 (D-05..D-08)"
```
**Rollback:** `git revert <hash>` — يعيد `App.xaml` لقاموس واحد ويحذف الملفات الأربعة.

---

# Part 4 — تعريف الـ SQL View لـ PatientHistory (D-03)

> **الغرض:** تنفيذ DD-03 بإضافة طبقة SQL بجانب طبقة C# القائمة، لأن المواصفات (`Docs/MasrLab_Specifications_and_Audit.md` — Entity 3، الجزء الثالث "قرارات سد الفجوات") تنص صراحة أن `PatientHistory` **كيان مُشتق (Derived Entity / SQL View) وليس جدولاً مخزَّناً**، وله 16 حقلاً ناتجاً عن استعلام، منها `ComparisonFlag`.
> **الخطورة:** 🟢 منخفضة — ملف `.sql` لا يدخل التصريف ولا يُنفَّذ على أي قاعدة بيانات في هذا الجزء.

## 4.1 الملفات المتأثرة
**إنشاء:**
- `src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.sql`

**إبقاء بلا تعديل (إلزامي):**
- `src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.cs` — **يبقى كما هو تماماً.**

**تعديل:**
- `Docs/implementation_plan.md`

## 4.2 خطوات التنفيذ التفصيلية

1. إنشاء `PatientHistoryView.sql` كـ **Placeholder موثّق** يُصرّح بالحقول الستة عشر المطلوبة من المواصفات دون كتابة منطق الاستعلام الفعلي (المشروع لم يدخل مرحلة الـ Business Logic بعد):

```sql
/* ============================================================================
   MasrLab — vw_PatientHistory
   ----------------------------------------------------------------------------
   Entity 3 (PatientHistory) — كيان مُشتق (Derived Entity / SQL View)
   المرجع: Docs/MasrLab_Specifications_and_Audit.md — القسم 2.1 / Entity 3
           + الجزء الثالث (قرارات سد الفجوات — الفجوة رقم 1)
   القرار الحاكم: DD-03 (اعتماد طبقتي SQL + C# معاً)

   ⚠️ PLACEHOLDER — لم يُكتب منطق الاستعلام بعد.
   يُملأ لاحقاً في مرحلة Business Logic بحيث يجمع نتائج TestResult
   عبر المسار: TestResult → VisitTest → PatientVisit → Patient
   لنفس المريض (PatientId / LabId) ونفس TestId، مرتبةً تصاعدياً بـ VisitDate،
   ويعرض القيمة السابقة والحالية جنباً إلى جنب للمقارنة.

   الحقول الستة عشر المطلوبة (View Columns):
     01. PatientId              INT            — من Patient
     02. LabId                  NVARCHAR       — من Patient
     03. TestId                 INT            — من Test عبر VisitTest
     04. TestName               NVARCHAR       — Test.Name
     05. TestReportName         NVARCHAR       — Test.ReportName
     06. PreviousValue          NVARCHAR       — TestResult.Value (السابقة)
     07. PreviousUnit           NVARCHAR       — TestResult.Unit (السابقة)
     08. PreviousReferenceRange NVARCHAR       — TestResult.ReferenceRange (السابقة)
     09. PreviousStatus         NVARCHAR       — High/Low/Normal (السابقة)
     10. PreviousVisitDate      DATE           — PatientVisit.VisitDate (السابقة)
     11. CurrentValue           NVARCHAR       — TestResult.Value (الحالية)
     12. CurrentUnit            NVARCHAR       — TestResult.Unit (الحالية)
     13. CurrentReferenceRange  NVARCHAR       — TestResult.ReferenceRange (الحالية)
     14. CurrentStatus          NVARCHAR       — High/Low/Normal (الحالية)
     15. CurrentVisitDate       DATE           — تاريخ الزيارة الحالية
     16. ComparisonFlag         BIT            — هل يوجد فرق بين القيمتين

   ملاحظات إلزامية:
     - هذا الـ View لا يحمل أعمدة تدقيق (CreatedAt/UpdatedAt/IsDeleted)
       لأنه ليس جدولاً مخزَّناً — راجع القسم 7.3.2 (الاستثناء).
     - ComparisonFlag يتطلب استعلاماً/حساباً من طبقة التطبيق،
       وهو سبب اعتماد الطبقتين معاً (SQL View + keyless entity في EF Core).
     - الطبقة المقابلة في C#:
       src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.cs
       (keyless entity — تُستهلك عبر EF Core بعد ربطها بـ ToView).
   ============================================================================ */

-- TODO (مرحلة Business Logic): استبدال هذا الـ Placeholder بتعريف الـ View الفعلي.
-- CREATE OR ALTER VIEW dbo.vw_PatientHistory AS
-- SELECT ... ;
```

2. ⛔ **ممنوع في هذا الجزء:**
   - تعديل `PatientHistoryView.cs` (لا إضافة حقول ولا تغيير الحقول الستة الحالية) — توسيعه إلى الحقول الـ16 وربطه بـ `ToView(...)`/`HasNoKey()` عمل يخص مرحلة الـ Business Logic ونمذجة EF، وخارج نطاق هذه الخطة الهيكلية.
   - تعديل `MasrLabDbContext.cs` أو إضافة `DbSet<PatientHistoryView>` أو استدعاء `HasNoKey()`.
   - تنفيذ الـ SQL على أي قاعدة بيانات.
   - إضافة `<EmbeddedResource>` أو `<None>` للملف في `MasrLab.Infrastructure.csproj` — لا حاجة له الآن: ملفات `.sql` تُلتقط ضمناً كـ `None` ولا تدخل التصريف. (تضمينه كمورد مُدمج يخص مرحلة تشغيل الـ View آلياً لاحقاً.)

## 4.3 تحديث `implementation_plan.md` المصاحب (DD-10)
في شجرة Infrastructure → `Persistence/Views/` (~الأسطر 411–412):
```
│   ├── Views/                                   ← SQL Views
│   │   └── PatientHistoryView.sql               ← Entity 3 — SQL View للتاريخ المرضي
```
→
```
│   ├── Views/                                   ← الكيان المشتق PatientHistory (Entity 3) — طبقتان بقرار DD-03
│   │   ├── PatientHistoryView.sql               ← تعريف الـ SQL View (Placeholder — 16 حقلاً حسب المواصفات)
│   │   └── PatientHistoryView.cs                ← Keyless Entity لاستهلاك الـ View عبر EF Core
```
وفي فقرة **"تبرير التصنيف"** أسفل شجرة Infrastructure:
```
- `Views/` يحتوي SQL View للكيان المشتق PatientHistory (Entity 3).
```
→
```
- `Views/` يحتوي طبقتي الكيان المشتق PatientHistory (Entity 3) معاً بقرار **DD-03**:
  ملف `.sql` يعرّف الـ View على مستوى قاعدة البيانات، وملف `.cs` كـ keyless entity تستهلكه EF Core.
  المبرر: المواصفات (الجزء الثالث — قرارات سد الفجوات) تنص أن PatientHistory كيان مُشتق غير مخزَّن،
  وحقل `ComparisonFlag` يحتاج استعلاماً/حساباً من طبقة التطبيق — ما يستلزم الطبقتين معاً.
```

## 4.4 معايير التحقق
- [ ] الملفان موجودان معاً:
  ```bash
  ls src/MasrLab.Infrastructure/Persistence/Views/
  # PatientHistoryView.cs  PatientHistoryView.sql
  ```
- [ ] `git diff` **لا يُظهر أي تغيير** على `PatientHistoryView.cs`.
- [ ] ملف `.sql` يذكر الحقول الستة عشر بأسمائها كما في المواصفات (تحقق يدوي بالمقارنة مع سطر Entity 3 في ملف المواصفات).
- [ ] لا وجود لأي `CREATE VIEW` غير مُعلَّق (الملف Placeholder فقط):
  ```bash
  grep -n "^\s*CREATE" src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.sql
  ```
  الناتج المتوقع: **فارغ**.
- [ ] لا تعديل على `MasrLabDbContext.cs` ولا على `MasrLab.Infrastructure.csproj`.
- [ ] ✅ **بوابة الجودة:** `dotnet build` + `dotnet test` ناجحان بلا تحذيرات جديدة.

## 4.5 الـ Commit
```bash
git add src/MasrLab.Infrastructure Docs/implementation_plan.md
git commit -m "feat(persistence): إضافة PatientHistoryView.sql كطبقة SQL بجانب الـ keyless entity وفق DD-03 (D-03)"
```

---

# Part 5 — سلوك RTL في طبقة العرض (D-21)

> **الغرض:** إنشاء `Behaviors/RtlBehavior.cs` المنصوص عليه في الخطة والمفقود فعلياً، دعماً لمتطلب 1 (واجهة عربية RTL بالكامل).
> **الخطورة:** 🟡 متوسطة — أول ملف C# جديد في الحل، داخل مشروع WPF.

## 5.1 الملفات المتأثرة
**إنشاء:**
- `src/MasrLab.Presentation/Behaviors/RtlBehavior.cs`

**تعديل:**
- `Docs/implementation_plan.md`

## 5.2 خطوات التنفيذ التفصيلية

1. إنشاء المجلد `src/MasrLab.Presentation/Behaviors/` ثم الملف بالمحتوى التالي.
   **قيد إلزامي:** التنفيذ عبر **Attached Property** من WPF القياسي فقط — ⛔ **ممنوع** إضافة حزمة `Microsoft.Xaml.Behaviors.Wpf` أو أي حزمة NuGet جديدة في هذا الجزء (إضافة حزمة = تغيير تبعيات خارج نطاق مواءمة الهيكل، وقد يجلب تحذيرات جديدة).

```csharp
using System.Windows;

namespace MasrLab.Presentation.Behaviors;

/// <summary>
/// سلوك RTL (متطلب 1): يضبط اتجاه التدفق من اليمين لليسار على أي عنصر واجهة.
/// يُطبَّق تصريحياً في XAML عبر خاصية مرفقة:
///     &lt;Window behaviors:RtlBehavior.IsEnabled="True" /&gt;
/// هيكل أساسي — منطق الواجهة التفصيلي يُستكمل في مرحلة تصميم الشاشات.
/// </summary>
public static class RtlBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(RtlBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.FlowDirection = e.NewValue is true
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }
}
```

2. ⛔ **ممنوع:** تطبيق السلوك على `MainWindow.xaml` أو أي View قائم في هذا الجزء — الهدف هنا استكمال الهيكل فقط، وربطه بالشاشات يخص مرحلة الواجهة.
3. ⛔ **ممنوع:** إنشاء أي ملف آخر داخل `Behaviors/`.

## 5.3 تحديث `implementation_plan.md` المصاحب (DD-10)
الكتلة الحالية (~الأسطر 611–612) صحيحة أصلاً وتبقى، مع توضيح آلية التنفيذ:
```
├── Behaviors/                                   ← سلوكيات XAML
│   └── RtlBehavior.cs                           ← سلوك RTL (متطلب 1)
```
→
```
├── Behaviors/                                   ← سلوكيات XAML
│   └── RtlBehavior.cs                           ← سلوك RTL (متطلب 1) — Attached Property بـ WPF القياسي
│                                                   (بلا اعتماد على Microsoft.Xaml.Behaviors)
```

## 5.4 معايير التحقق
- [ ] الملف موجود في المسار الصحيح تماماً: `src/MasrLab.Presentation/Behaviors/RtlBehavior.cs`.
- [ ] الـ namespace هو `MasrLab.Presentation.Behaviors` (متسق مع بقية المشروع الذي يستخدم بادئة `MasrLab.Presentation.*` رغم أن اسم الـ Assembly هو `MasrLab`).
- [ ] لم تُضَف أي `PackageReference` جديدة:
  ```bash
  git diff -- src/MasrLab.Presentation/MasrLab.csproj
  ```
  الناتج المتوقع: **فارغ**.
- [ ] ✅ **بوابة الجودة:** `dotnet build` — 0 أخطاء و**0 تحذيرات جديدة** (انتبه لتحذيرات nullable `CS86xx` لأن `Nullable=enable` مفعّل على مستوى الحل عبر `Directory.Build.props`) + `dotnet test` ناجح.

## 5.5 الـ Commit
```bash
git add src/MasrLab.Presentation/Behaviors Docs/implementation_plan.md
git commit -m "feat(presentation): إضافة RtlBehavior كـ Attached Property لدعم متطلب RTL (D-21)"
```

---

# Part 6 — نقل `AgeCalculator` إلى طبقة Domain (D-23 / DD-08)

> **الغرض:** إنشاء `AgeCalculator` كدالة صرفة (Pure Function) بلا تبعيات داخل `MasrLab.Domain/Common/`، تنفيذاً للشق الثاني من DD-08.
> **الخطورة:** 🟠 متوسطة — أول تعديل في الطبقة الأساسية التي تعتمد عليها **كل** الطبقات الأخرى، فأي كسر فيها يوقف الحل بأكمله.

## 6.1 الملفات المتأثرة
**إنشاء:**
- `src/MasrLab.Domain/Common/AgeCalculator.cs`

**اختياري مُستحسن (إنشاء):**
- `tests/MasrLab.Domain.Tests/Common/AgeCalculatorTests.cs`

**تعديل:**
- `Docs/implementation_plan.md`

⛔ **ممنوع قطعياً:** إنشاء `src/MasrLab.Presentation/Helpers/AgeCalculator.cs` — نصّ DD-08 صراحة على ألا يُنشأ في `Presentation/Helpers/` إطلاقاً.
⛔ **ممنوع:** تعديل `Patient.cs` أو إضافة خاصية `Age` محسوبة إليه في هذا الجزء (تعديل الكيانات منطق أعمال، خارج النطاق).

## 6.2 خطوات التنفيذ التفصيلية

1. إنشاء الملف بمحتوى دالة صرفة، متسقاً مع حقول `Patient` الفعلية (`AgeYears`, `AgeMonths`, `AgeDays`) ومع التعداد القائم `MasrLab.Domain.Common.Enums.AgeUnit`:

```csharp
namespace MasrLab.Domain.Common;

/// <summary>
/// حساب السن (سنوات/أشهر/أيام) — دالة صرفة (Pure Function) بلا أي تبعيات خارجية.
/// موضعها في طبقة Domain بقرار DD-08، لأنها منطق نطاق خالص يخدم
/// Patient.AgeYears / AgeMonths / AgeDays ومطابقة القيم المرجعية (Entity 6: ReferenceValue.AgeUnit).
/// </summary>
public static class AgeCalculator
{
    /// <summary>
    /// يحسب السن كثلاثية (سنوات، أشهر، أيام) بين تاريخ الميلاد وتاريخ مرجعي.
    /// </summary>
    /// <param name="birthDate">تاريخ الميلاد.</param>
    /// <param name="asOf">التاريخ المرجعي (عادةً تاريخ الزيارة).</param>
    /// <exception cref="ArgumentOutOfRangeException">إذا كان تاريخ الميلاد لاحقاً للتاريخ المرجعي.</exception>
    public static (int Years, int Months, int Days) Calculate(DateTime birthDate, DateTime asOf)
    {
        var from = birthDate.Date;
        var to = asOf.Date;

        if (from > to)
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                "تاريخ الميلاد لا يمكن أن يكون لاحقاً للتاريخ المرجعي.");
        }

        var years = to.Year - from.Year;
        var months = to.Month - from.Month;
        var days = to.Day - from.Day;

        if (days < 0)
        {
            months--;
            days += DateTime.DaysInMonth(
                to.Month == 1 ? to.Year - 1 : to.Year,
                to.Month == 1 ? 12 : to.Month - 1);
        }

        if (months < 0)
        {
            years--;
            months += 12;
        }

        return (years, months, days);
    }
}
```

2. **(اختياري مُستحسن)** إضافة اختبارات وحدة للتحقق من صحة الدالة، مع الالتزام بأسلوب مشروع الاختبار القائم (xUnit، `Using Include="Xunit"` مُعرَّف في الـ csproj فيقلّ الحاجة لـ `using`):

```csharp
using MasrLab.Domain.Common;

namespace MasrLab.Domain.Tests.Common;

public class AgeCalculatorTests
{
    [Fact]
    public void Calculate_ExactYears_ReturnsYearsOnly()
    {
        var result = AgeCalculator.Calculate(new DateTime(2000, 1, 15), new DateTime(2020, 1, 15));
        Assert.Equal((20, 0, 0), result);
    }

    [Fact]
    public void Calculate_WithDayBorrow_NormalizesMonthsAndDays()
    {
        var result = AgeCalculator.Calculate(new DateTime(2020, 1, 31), new DateTime(2020, 3, 1));
        Assert.Equal(0, result.Years);
        Assert.Equal(1, result.Months);
    }

    [Fact]
    public void Calculate_BirthDateAfterReference_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AgeCalculator.Calculate(new DateTime(2025, 1, 1), new DateTime(2024, 1, 1)));
    }
}
```

> **ملاحظة:** إن أضفت الاختبارات فهي جزء من نفس الـ Commit، ويجب أن تنجح جميعها. إن اخترت عدم إضافتها فلا يتأثر أي معيار آخر — لكن الإضافة مُفضَّلة لأنها ترفع قيمة بوابة `dotnet test` التي تعمل حالياً على اختبارات Placeholder فارغة فقط.

3. ⛔ **ممنوع:** إضافة أي `PackageReference` أو `ProjectReference` لمشروع Domain (يجب أن يبقى بلا أي تبعية — وهذا جوهر كونه الطبقة الأكثر استقلالية).

## 6.3 تحديث `implementation_plan.md` المصاحب (DD-10)

**(أ) شجرة Domain → `Common/` (~الأسطر 89–91):**
```
│   ├── IAuditableEntity.cs                      ← واجهة لأعمدة التدقيق
│   ├── ISoftDeletable.cs                        ← واجهة للحذف المنطقي
│   └── Enums/                                   ← التعدادات المشتركة
```
→
```
│   ├── IAuditableEntity.cs                      ← واجهة لأعمدة التدقيق
│   ├── ISoftDeletable.cs                        ← واجهة للحذف المنطقي
│   ├── AgeCalculator.cs                         ← حساب السن (سنوات/أشهر/أيام) — دالة صرفة بلا تبعيات
│   │                                               نُقل إلى Domain بقرار DD-08 (كان مقترحاً في Presentation/Helpers)
│   └── Enums/                                   ← التعدادات المشتركة
```

**(ب) شجرة Presentation — حذف نصف كتلة `Helpers/`** (~الأسطر 614–616). الكتلة الحالية:
```
├── Helpers/                                     ← مساعدات
│   ├── LabIdGenerator.cs                        ← توليد Lab ID الفريد
│   └── AgeCalculator.cs                         ← حساب السن (سنوات/أشهر/أيام)
```
→ في هذا الجزء تُصبح:
```
├── Helpers/                                     ← مساعدات
│   └── LabIdGenerator.cs                        ← توليد Lab ID الفريد (يُنقل إلى Application في Part 7 — DD-08)
```
> الكتلة كلها ستُحذف نهائياً في Part 7. هذا التدرّج مقصود ليبقى ملف الخطة مطابقاً للواقع **بعد كل Commit على حدة** كما يفرض DD-10.

**(ج) إضافة سطر في فقرة "تبرير التصنيف الفرعي" لطبقة Domain:**
```
- `Common/AgeCalculator.cs` دالة صرفة (Pure Function) بلا تبعيات، موضعها Domain بقرار **DD-08**
  لأنها منطق نطاق يخدم Patient.Age* والقيم المرجعية (ReferenceValue.AgeUnit) ولا علاقة لها بالعرض.
```

## 6.4 معايير التحقق
- [ ] الملف موجود في `src/MasrLab.Domain/Common/AgeCalculator.cs` والـ namespace `MasrLab.Domain.Common`.
- [ ] **لا وجود** لأي مجلد `Helpers` داخل Presentation حتى الآن:
  ```bash
  ls src/MasrLab.Presentation/ | grep -i helpers
  ```
  الناتج المتوقع: **فارغ**.
- [ ] `MasrLab.Domain.csproj` بلا أي `ItemGroup` تبعيات:
  ```bash
  git diff -- src/MasrLab.Domain/MasrLab.Domain.csproj   # فارغ
  ```
- [ ] ✅ **بوابة الجودة:** `dotnet build` (0 أخطاء / 0 تحذيرات جديدة) + `dotnet test` ناجح — وإن أُضيفت اختبارات `AgeCalculatorTests` فيجب أن تظهر ضمن الناجحة وأن يرتفع إجمالي عدد الاختبارات مقارنة بخط أساس Part 0.

## 6.5 الـ Commit
```bash
git add src/MasrLab.Domain tests/MasrLab.Domain.Tests Docs/implementation_plan.md
git commit -m "feat(domain): إضافة AgeCalculator كدالة صرفة في Domain/Common وفق DD-08 (D-23)"
```

---

# Part 7 — نقل `LabIdGenerator` إلى طبقة Application (D-22 / DD-08)

> **الغرض:** إنشاء `LabIdGenerator` في `MasrLab.Application/Common/Helpers/` معتمداً على `IPatientRepository`، تنفيذاً للشق الأول من DD-08، وحذف مجلد `Helpers/` من شجرة Presentation في الخطة نهائياً.
> **الخطورة:** 🔴 الأعلى في هذه الخطة — لأنه الوحيد الذي يُدخِل **تبعية بين مكوّنات** (Application → Domain.Interfaces) ويلامس حدود مسؤولية طبقة التطبيق. لذلك جاء أخيراً بين أجزاء الكود.

## 7.1 الملفات المتأثرة
**إنشاء:**
- `src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs`

**تعديل:**
- `Docs/implementation_plan.md`

**بلا تعديل (إلزامي):**
- `src/MasrLab.Domain/Interfaces/IPatientRepository.cs` — يبقى كما هو (`IRepository<Patient>` بلا أعضاء إضافية).
- `src/MasrLab.Application/DependencyInjection.cs` — لا تسجيل الآن (انظر 7.2 بند 3).
- `src/MasrLab.Application/Features/PatientManagement/Queries/GenerateLabId/*` — يبقى كما هو.

⛔ **ممنوع قطعياً:** إنشاء `src/MasrLab.Presentation/Helpers/LabIdGenerator.cs`.

## 7.2 خطوات التنفيذ التفصيلية

1. إنشاء المجلد `src/MasrLab.Application/Common/Helpers/` ثم الملف:

```csharp
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Common.Helpers;

/// <summary>
/// توليد Lab ID الفريد للمريض.
/// موضعه في طبقة Application بقرار DD-08، لأنه يعتمد على IPatientRepository
/// (تحقق من التفرّد مقابل المخزَّن) وليس دالة صرفة — فلا يصلح لطبقة Domain،
/// ولا يجوز وضعه في Presentation/Helpers.
/// هيكل أساسي: منطق التوليد الفعلي يُكتب في مرحلة Business Logic،
/// اتساقاً مع أسلوب Handlers الحالية في المشروع.
/// </summary>
public class LabIdGenerator
{
    private readonly IPatientRepository _patientRepository;

    public LabIdGenerator(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository
            ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    /// <summary>
    /// يولّد Lab ID فريداً غير مستخدم مسبقاً.
    /// </summary>
    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        _ = _patientRepository;
        throw new NotImplementedException();
    }
}
```

> **مبرر السطر `_ = _patientRepository;`:** يمنع أي تحذير محلّل مستقبلي عن حقل مُسنَد وغير مقروء، ويوثّق التبعية المقصودة. يُحذف عند كتابة المنطق الفعلي.
> **مبرر `NotImplementedException`:** متسق تماماً مع الأسلوب القائم في المستودع (مثل `GenerateLabIdQueryHandler`)، ويؤكد أن هذا الجزء **هيكلي فقط**.

2. ⛔ **ممنوع:** كتابة أي منطق توليد فعلي (تسلسل، تاريخ، بادئة، تحقق تفرّد) — هذا منطق أعمال خارج نطاق الخطة.
3. ⛔ **ممنوع:** تسجيل `LabIdGenerator` في `DependencyInjection.cs`. السبب: `AddApplication` حالياً فارغ تماماً (لا MediatR ولا AutoMapper ولا FluentValidation مُسجَّلة بعد)، وتسجيل هذا الصنف وحده يُنشئ حالة DI غير متسقة ومضلِّلة. تسجيله يتم دفعة واحدة ضمن مهمة "تهيئة DI لطبقة التطبيق" المستقلة.
4. ⛔ **ممنوع:** إضافة أي عضو إلى `IPatientRepository` (مثل `ExistsByLabIdAsync`) — تعديل العقود يخص مرحلة الـ Business Logic.
5. حذف نهائي لكتلة `Helpers/` من شجرة Presentation في ملف الخطة (تفصيلها في 7.3).

## 7.3 تحديث `implementation_plan.md` المصاحب (DD-10)

**(أ) شجرة Application → `Common/`** — تحويل `Behaviors/` من آخر عنصر وإضافة `Helpers/` بعده (~الأسطر 201–205):
```
│   ├── Mappings/                                ← تحويلات Entity ↔ DTO
│   │   └── MappingProfile.cs
│   └── Behaviors/                               ← سلوكيات عامة (Validation, Logging)
│       ├── ValidationBehavior.cs
│       └── AuditBehavior.cs                     ← تسجيل تلقائي في AuditLog (القسم 7.2)
```
→
```
│   ├── Mappings/                                ← تحويلات Entity ↔ DTO
│   │   └── MappingProfile.cs
│   ├── Behaviors/                               ← سلوكيات عامة (Validation, Logging)
│   │   ├── ValidationBehavior.cs
│   │   └── AuditBehavior.cs                     ← تسجيل تلقائي في AuditLog (القسم 7.2)
│   └── Helpers/                                 ← مساعدات طبقة التطبيق
│       └── LabIdGenerator.cs                    ← توليد Lab ID الفريد (يعتمد على IPatientRepository)
│                                                   نُقل إلى Application بقرار DD-08
```

**(ب) شجرة Presentation — حذف كتلة `Helpers/` نهائياً** (الحالة بعد Part 6):
```
├── Helpers/                                     ← مساعدات
│   └── LabIdGenerator.cs                        ← توليد Lab ID الفريد (يُنقل إلى Application في Part 7 — DD-08)
│
```
→ **تُحذف الكتلة بالكامل** (بما فيها سطر الفاصل `│`)، فتصبح `Behaviors/` متبوعةً مباشرة بـ `appsettings.json` ثم `DependencyInjection.cs`.

**(ج) إضافة سطر في فقرة "تبرير التصنيف" لطبقة Presentation:**
```
- ⚠️ لا يوجد مجلد `Helpers/` في طبقة العرض بقرار **DD-08**:
  `LabIdGenerator` → `MasrLab.Application/Common/Helpers/` (يعتمد على IPatientRepository)،
  و`AgeCalculator` → `MasrLab.Domain/Common/` (دالة صرفة بلا تبعيات).
  طبقة العرض لا تستضيف منطق نطاق أو تطبيق.
```

**(د) إضافة سطر في فقرة "تبرير التصنيف" لطبقة Application:**
```
- `Common/Helpers/` يستضيف المساعدات التي تحتاج تبعيات على عقود المستودعات (Repository Contracts)
  ولا تصلح كدوال صرفة في Domain — أولها `LabIdGenerator` (قرار **DD-08**).
```

## 7.4 معايير التحقق
- [ ] الملف موجود في `src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs` بـ namespace `MasrLab.Application.Common.Helpers`.
- [ ] **لا وجود** لأي مجلد `Helpers` في Presentation، لا في الشجرة الفعلية ولا في ملف الخطة:
  ```bash
  ls src/MasrLab.Presentation/ | grep -i helpers                 # فارغ
  grep -n "Presentation" -A 200 Docs/implementation_plan.md | grep -n "Helpers/"   # لا نتيجة داخل شجرة Presentation
  ```
- [ ] `git diff` لا يُظهر أي تغيير على: `IPatientRepository.cs`، `DependencyInjection.cs` (الثلاثة)، `GenerateLabIdQuery*`.
- [ ] لم تُضَف أي حزمة إلى `MasrLab.Application.csproj`.
- [ ] لم تنعكس أي تبعية اتجاهية خاطئة: Application لا يزال يُراجع `MasrLab.Domain` فقط.
- [ ] ✅ **بوابة الجودة:** `dotnet build` (0 أخطاء / 0 تحذيرات جديدة) + `dotnet test` ناجح بالكامل.

## 7.5 الـ Commit
```bash
git add src/MasrLab.Application Docs/implementation_plan.md
git commit -m "feat(application): إضافة LabIdGenerator في Common/Helpers وإلغاء Helpers من طبقة العرض وفق DD-08 (D-22)"
```
**Rollback:** `git revert <hash>` — يحذف الملف ويعيد كتلة `Helpers/` في الوثيقة. لا يؤثر على Parts 1–6.

---

# Part 8 — التدقيق النهائي وإغلاق المواءمة (Final Audit & Closure)

> **الغرض:** إثبات أن الهيكل الفعلي وملف `implementation_plan.md` أصبحا متطابقين تماماً، وأن الانحرافات الثمانية عشر الصالحة أُغلقت، والستة الملغاة لا أثر لها.
> **الخطورة:** 🟢 منخفضة — تحقق + تحديث وثيقة.

## 8.1 الملفات المتأثرة
- `Docs/implementation_plan.md` (إضافة سجل مواءمة في نهايته)

## 8.2 خطوات التنفيذ التفصيلية

1. تشغيل جرد الهيكل الفعلي ومقارنته يدوياً بأشجار `implementation_plan.md` الأربع (Domain / Application / Infrastructure / Presentation):
   ```bash
   find src -path "*/bin" -prune -o -path "*/obj" -prune -o -type f -print | sort
   find src -path "*/bin" -prune -o -path "*/obj" -prune -o -type d -print | sort
   ```
2. تنفيذ فحوص الاتساق السريعة:
   ```bash
   # (أ) لا أثر للتجميعات الملغاة داخل شجرتي ViewModels/Views
   grep -n "ViewModels/MasterData\|ViewModels/Administration\|ViewModels/Financial\|Views/MasterData\|Views/Administration\|Views/Financial" Docs/implementation_plan.md   # فارغ
   # (ب) الاسم الصحيح للمحوّل
   grep -c "BooleanToVisibilityConverter" Docs/implementation_plan.md      # ≥ 1
   grep -c "BoolToVisibilityConverter\b" Docs/implementation_plan.md       # 0 (عدا الاسم الكامل)
   # (ج) لا Helpers في طبقة العرض
   ls src/MasrLab.Presentation | grep -i helpers                            # فارغ
   # (د) الملفات المُنشأة الثمانية موجودة
   ls src/MasrLab.Infrastructure/Persistence/Migrations/.gitkeep \
      src/MasrLab.Presentation/Resources/Icons/.gitkeep \
      src/MasrLab.Presentation/Resources/Images/.gitkeep \
      src/MasrLab.Presentation/Resources/Fonts/.gitkeep \
      src/MasrLab.Presentation/Resources/Styles/Colors.xaml \
      src/MasrLab.Presentation/Behaviors/RtlBehavior.cs \
      src/MasrLab.Domain/Common/AgeCalculator.cs \
      src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs \
      src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.sql
   ```
3. إضافة **سجل المواءمة** في نهاية `Docs/implementation_plan.md`:

```markdown
---

## سجل مواءمة الهيكل (Structure Alignment Log)

| البند | القيمة |
|---|---|
| Commit الأساس | `e4aab3c34b255571d66c85289d6fd39da6ea8297` (development) |
| المرجع التنفيذي | `Handoff_Structure_Alignment_Plan.md` (Parts 0–8) |
| القرارات المطبَّقة | DD-01 … DD-10 |
| انحرافات عولجت بتغيير ملفات | D-03, D-04, D-05, D-06, D-07, D-08, D-09, D-10, D-11, D-21, D-22, D-23 |
| انحرافات عولجت بتوثيق فقط | D-01, D-02, D-12, D-16, D-20, D-24 |
| انحرافات مُلغاة (Not a Deviation) | D-13, D-14, D-15, D-17, D-18, D-19 — بقرار **DD-05** |

**قاعدة دائمة (DD-10):** هذا الملف هو **مصدر الحقيقة الوحيد** لهيكل المشروع. أي تغيير في الهيكل الفعلي
يجب أن يُصاحبه تحديث لهذا الملف في **نفس الـ Commit**. ويُمنع على أي وكيل برمجي أو مطوّر اقتراح خطة
أو قرار تصميمي يخالف ما ورد هنا دون قرار صريح جديد من صاحب المشروع.
```

4. تشغيل بوابة الجودة النهائية ومقارنة النتائج بخط أساس Part 0.

## 8.3 معايير التحقق (Definition of Done للخطة بأكملها)
- [ ] جميع فحوص 8.2 تُرجِع النتائج المتوقعة.
- [ ] الملفات التسعة المُنشأة موجودة (4 `.gitkeep` + 4 `.xaml` + `.sql` + 3 ملفات `.cs`… إجمالي المُنشأ = 12 ملفاً: 4 gitkeep، 4 styles، 1 sql، 3 cs).
- [ ] `Docs/implementation_plan.md` يصف الهيكل الفعلي **بلا فارق واحد**.
- [ ] عدد الـ Commits على فرع العمل = **7** (Parts 1–8، حيث Part 0 بلا Commit)، وكل واحد قابل للـ revert منفرداً.
- [ ] ✅ `dotnet build`: **0 Errors**، وقائمة التحذيرات مطابقة تماماً لخط أساس Part 0 (لا كود تحذير جديد).
- [ ] ✅ `dotnet test`: جميع الاختبارات ناجحة (وعدد الاختبارات ≥ خط الأساس).

## 8.4 الـ Commit
```bash
git add Docs/implementation_plan.md
git commit -m "docs(plan): إضافة سجل مواءمة الهيكل وإغلاق الانحرافات الصالحة الثمانية عشر"
```

---

## 9. ملخّص الملفات المُنشأة والمعدَّلة عبر الخطة كاملة

### ملفات تُنشأ (12)
| الجزء | المسار |
|---|---|
| 2 | `src/MasrLab.Infrastructure/Persistence/Migrations/.gitkeep` |
| 2 | `src/MasrLab.Presentation/Resources/Icons/.gitkeep` |
| 2 | `src/MasrLab.Presentation/Resources/Images/.gitkeep` |
| 2 | `src/MasrLab.Presentation/Resources/Fonts/.gitkeep` |
| 3 | `src/MasrLab.Presentation/Resources/Styles/Colors.xaml` |
| 3 | `src/MasrLab.Presentation/Resources/Styles/ButtonStyles.xaml` |
| 3 | `src/MasrLab.Presentation/Resources/Styles/TextBoxStyles.xaml` |
| 3 | `src/MasrLab.Presentation/Resources/Styles/DataGridStyles.xaml` |
| 4 | `src/MasrLab.Infrastructure/Persistence/Views/PatientHistoryView.sql` |
| 5 | `src/MasrLab.Presentation/Behaviors/RtlBehavior.cs` |
| 6 | `src/MasrLab.Domain/Common/AgeCalculator.cs` |
| 7 | `src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs` |
| 6 (اختياري) | `tests/MasrLab.Domain.Tests/Common/AgeCalculatorTests.cs` |

### ملفات تُعدَّل (2)
| الجزء | المسار |
|---|---|
| 3 | `src/MasrLab.Presentation/App.xaml` |
| 1–8 | `Docs/implementation_plan.md` |

### ملفات يُمنع لمسها في هذه الخطة
`MasrLab.sln` · جميع ملفات `*.csproj` · `Directory.Build.props` · `global.json` · `MasrLabDbContext.cs` · `PatientHistoryView.cs` · `IPatientRepository.cs` · جميع ملفات `DependencyInjection.cs` · جميع ملفات `Features/**` · جميع `Entities/**` · جميع `Configurations/**` · جميع `ViewModels/**` و`Views/**` القائمة · `appsettings.json` · `.gitignore`

---

## 10. سياسة الفرع والدمج والتراجع

- **فرع العمل:** `feature/structure-alignment` متفرّع من `development` عند `e4aab3c`.
- **نمط الـ Commit:** Conventional Commits (`docs:` / `chore:` / `feat:`) مع ذكر رمز الانحراف والقرار الحاكم في الرسالة.
- **التراجع الجزئي:** كل جزء `git revert` مستقل. الترتيب مصمَّم بحيث لا يعتمد أي جزء على سابقه في الكود، باستثناء علاقة توثيقية واحدة: Part 7 يحذف كتلة `Helpers/` التي عدّلها Part 6 في الوثيقة — لذلك عند التراجع عن Part 6 وحده، راجع فقرة `Helpers/` في الوثيقة يدوياً.
- **الدمج في `development`:** بعد نجاح Part 8 فقط، ويفضَّل `--no-ff` للحفاظ على تسلسل الأجزاء في التاريخ.
- **معيار قبول الدمج:** بناء نظيف على بيئة Windows + جميع الاختبارات خضراء + تطابق تام بين الشجرة الفعلية وملف الخطة.

---

## 11. ما هو خارج نطاق هذه الخطة صراحةً (Out of Scope)

هذه البنود **ليست** انحرافات ولا تُنفَّذ هنا، وتُذكر لمنع أي توسّع غير مأذون:

1. توليد أي EF Core Migration، أو تنفيذ `PatientHistoryView.sql` على قاعدة بيانات.
2. توسيع `PatientHistoryView.cs` إلى 16 حقلاً، أو ربطه بـ `ToView()` / `HasNoKey()` في `MasrLabDbContext`.
3. كتابة منطق `LabIdGenerator.GenerateAsync` أو أي منطق أعمال آخر.
4. تهيئة الـ DI (MediatR، AutoMapper، FluentValidation، DbContext، Interceptors) في ملفات `DependencyInjection.cs` الفارغة.
5. ملء ملفات Styles بألوان/أنماط، أو إضافة أيقونات وصور وخطوط فعلية.
6. أي إعادة تسمية لمشروع أو مجلد أو ملف (DD-01 و DD-04 و DD-05 تمنعها جميعاً).
7. استبدال اختبارات `PlaceholderTests.cs` أو إعادة هيكلة مشاريع الاختبار.
8. أي تعديل على `Docs/MasrLab_Specifications_and_Audit.md` — ملف المواصفات مرجع للقراءة فقط في هذه الخطة.

---

*نهاية الخطة — Parts 0 → 8.*
