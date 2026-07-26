# Sprint 0 — خطة التنفيذ النهائية

**الاسم:** Sprint0_Roadmap_Final_Adjudication.md
**الحالة:** النسخة الرسمية النهائية — المرجع الوحيد لتنفيذ Sprint 0
**التاريخ:** 2026-07-26
**الـ Commit:** a8c4702c20f768a404793eb559b7774ac876d118
**الـ Branch:** development

> هذا الملف يمثل خطة العمل النهائية لمرحلة Sprint 0.
> تم دمج جميع التعديلات التي ثبتت صحتها أثناء مراجعات ثلاث مستقلة.
> لا توجد تعديلات أخرى مطلوبة.

---

## الصورة العامة

المشروع في حالة هيكل مكتمل لكن غير موصّل. جميع الطبقات والمشاريع والكيانات والواجهات موجودة وصحيحة بنائياً. لكن التطبيق لا يعمل لأن:
- وصلات DI فارغة
- EF Core لا يطبق الإعدادات
- جميع المعالجات تُلقي استثناء
- لا توجد ترجمة (Migration)
- نقطة الدخول لا تبني ServiceProvider ولا تفتح نافذة

**الهدف من Sprint 0:** جعل التطبيق قابلاً للتشغيل والاختبار تمهيداً لبدء كتابة منطق الأعمال.

---

## المرحلة 1: الأساس الحرج

### الخطوة 1.1 — تفعيل إعدادات EF Core

| العنصر | التفاصيل |
|--------|----------|
| **الهدف** | جعل `OnModelCreating` يستدعي `ApplyConfigurationsFromAssembly` ويملأ ملفات الإعداد الـ 31 |
| **السبب** | بدونها لا يعرف EF Core المفاتيح والعلاقات والفهارس |
| **الملفات** | `src/MasrLab.Infrastructure/Pasistence/MasrLabDbContext.cs` + 31 ملف في `Persistence/Configurations/` |
| **الأولوية** | 🔴 حرجة |
| **الاعتمادية** | لا توجد |
| **معايير القبول** | `dotnet ef migrations add InitialCreate` يُنتج جدول DDL بكل الجداول والمفاتيح الرئيسية وال foreign keys والفهارس |
| **تقدير الجهد** | 6–10 ساعات |

---

### الخطوة 1.2 — إكمال DependencyInjection في الطبقة Application

| العنصر | التفاصيل |
|--------|----------|
| **الهدف** | تسجيل MediatR و AutoMapper و FluentValidation و سلوك Pipeline |
| **السبب** | بدونها لا يعمل أي معالج ولا يعمل التحقق ولا يعمل التحويل |
| **الملفات** | `src/MasrLab.Application/DependencyInjection.cs` |
| **الأولوية** | 🔴 حرجة |
| **الاعتمادية** | لا توجد |
| **معايير القبول** | `IMediator` وجميع المعالجات والمحققات والسلوكيات قابلة للحقن من DI |
| **تقدير الجهد** | 30 دقيقة |

---

### الخطوة 1.3 — إكمال DependencyInjection في طبقة Infrastructure

| العنصر | التفاصيل |
|--------|----------|
| **الهدف** | تسجيل DbContext مع ConnectionString والمراقبين (Interceptors) و UnitOfWork |
| **السبب** | بدون DbContext في DI لا يمكن لأي مستودع أو معالج الوصول لقاعدة البيانات |
| **الملفات** | `src/MasrLab.Infrastructure/DependencyInjection.cs` + `src/MasrLab.Presentation/App.xaml.cs` |
| **الأولوية** | 🔴 حرجة |
| **الاعتمادية** | الخطوة 1.1 + **الخطوة 2.2** (ConnectionString) |
| **معايير القبول** | `MasrLabDbContext` قابل للحقن من DI مع إرفاق المراقبين |
| **تقدير الجهد** | 1–2 ساعة |

**خطوات التنفيذ الفرعية:**
1. تعديل توقيع `AddInfrastructure()` لقبول `IConfiguration` — التوقيع الحالي في `Infrastructure/DependencyInjection.cs:12` parameterless
2. تحديث `App.xaml.cs` لتمرير `IConfiguration` إلى `AddInfrastructure()`
3. تسجيل `MasrLabDbContext` مع `UseSqlServer()` باستخدام ConnectionString من الإعدادات
4. تسجيل المراقبين (`AuditableEntityInterceptor` و `SoftDeleteInterceptor`) مع DbContext
5. تسجيل `IUnitOfWork` / `UnitOfWork`

---

### الخطوة 1.4 — تنفيذ UnitOfWork

| العنصر | التفاصيل |
|--------|----------|
| **الهدف** | استبدال `NotImplementedException` بتنفيذ فعلي لـ `SaveChangesAsync` |
| **السبب** | كل معالج يستخدم `UnitOfWork.SaveChangesAsync` لحفظ التغييرات |
| **الملفات** | `src/MasrLab.Infrastructure/Pasistence/UnitOfWork.cs` |
| **الأولوية** | 🔴 حرجة |
| **الاعتمادية** | الخطوة 1.3 |
| **معايير القبول** | `unitOfWork.SaveChangesAsync()` يُرجع عدد الصفوف المتأثرة |
| **تقدير الجهد** | 30 دقيقة |

---

### الخطوة 1.5 — جعل BaseEntity ينفذ الواجهات

| العنصر | التفاصيل |
|--------|----------|
| **الهدف** | جعل `BaseEntity` يعلن تنفيذ `IAuditableEntity` و `ISoftDeletable` |
| **السبب** | الحالي يُكرر الخصائص بدون تنفيذ الواجهات، مما يمنع المراقبين من التصفية بالواجهة |
| **الملفات** | `src/MasrLab.Domain/Common/BaseEntity.cs` |
| **الأولوية** | 🟠 عالية |
| **الاعتمادية** | لا توجد |
| **معايير القبول** | `BaseEntity` ينفذ الواجهتين و`dotnet build` ينجح |
| **تقدير الجهد** | 5 دقائق |

**تنفيذ:** تغيير السطر 3 في `BaseEntity.cs` من:
```csharp
public abstract class BaseEntity
إلى:

public abstract class BaseEntity : IAuditableEntity, ISoftDeletable
الخطوة 1.6 — تنفيذ المراقبين (Interceptors)
العنصر	التفاصيل
الهدف	AuditableEntityInterceptor يملأ حقول التدقيق؛ SoftDeleteInterceptor يحول الحذف إلى حذف ناعم
السبب	بدونها تبقى حقول التدقيق (0001-01-01) ويحدث حذف حقيقي بدلاً من ناعم
الملفات	src/MasrLab.Infrastructure/Pasistence/Interceptors/AuditableEntityInterceptor.cs + SoftDeleteInterceptor.cs
الأولوية	🟠 عالية
الاعتمادية	الخطوة 1.3 + الخطوة 1.5
معايير القبول	حفظ كيان جديد يملأ حقول التدقيق تلقائياً؛ حذف كيان يُعيّن IsDeleted = true
تقدير الجهد	1–2 ساعة
المرحلة 2: الوصل
الخطوة 2.1 — تنفيذ ValidationBehavior
العنصر	التفاصيل
الهدف	جعل ValidationBehavior يُشغّل المحققات فعلياً قبل المعالج
السبب	الحالي يحقن المحققات لكن يتجاهلها وينادي next() فقط
الملفات	src/MasrLab.Application/Common/Behaviors/ValidationBehavior.cs
الأولوية	🟠 عالية
الاعتمادية	الخطوة 1.2
معايير القبول	تحقق فاشل يُلقي ValidationException قبل تنفيذ المعالج
تقدير الجهد	30 دقيقة
الخطوة 2.2 — إنشاء appsettings.Local.json
العنصر	التفاصيل
الهدف	توفير ConnectionString حقيقي لـ SQL Server
السبب	appsettings.json:3 يحتوي ConnectionString فارغ
الملفات	جديد: src/MasrLab.Presentation/appsettings.Local.json + src/MasrLab.Presentation/MasrLab.csproj
الأولوية	🔴 حرجة
الاعتمادية	لا توجد
معايير القبول	dotnet ef database update ينجح؛ الملف متاح في مجلد الإخراج أثناء التشغيل
تقدير الجهد	15 دقيقة
تنفيذ:

إنشاء appsettings.Local.json بـ ConnectionString صالح
إضافة <Content Include="appsettings.Local.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content> إلى MasrLab.csproj
الخطوة 2.3 — إكمال App.xaml.cs
العنصر	التفاصيل
الهدف	بناء ServiceProvider وحل MainWindow وفتحها
السبب	الحالي ينشئ ServiceCollection لكن لا يبني المزود ولا يفتح أي نافذة
الملفات	src/MasrLab.Presentation/App.xaml.cs
الأولوية	🔴 حرجة
الاعتمادية	الخطوة 1.2 + الخطوة 1.3 + الخطوة 2.2
معايير القبول	dotnet run يفتح MainWindow
تقدير الجهد	30 دقيقة
الخطوة 2.4 — إكمال DependencyInjection في طبقة Presentation
العنصر	التفاصيل
الهدف	تسجيل ViewModels و NavigationService و MainWindow
السبب	الحالي فارغ؛ لا يمكن عمل MVVM navigation بدون تسجيل ViewModels
الملفات	src/MasrLab.Presentation/DependencyInjection.cs
الأولوية	🟠 عالية
الاعتمادية	لا توجد
معايير القبول	MainWindow يُحَل مع حقن MainViewModel
تقدير الجهد	30 دقيقة
المرحلة 3: أساس البيانات
الخطوة 3.1 — إنشاء الترجمة الأولى (Initial Migration)
العنصر	التفاصيل
الهدف	توليد أول ترجمة EF Core
السبب	لا توجد ترجمات حالية؛ مجلد Migrations يحتوي .gitkeep فقط
الملفات	src/MasrLab.Infrastructure/Pasistence/Migrations/
الأولوية	🔴 حرجة
الاعتمادية	الخطوة 1.1 + الخطوة 1.3 + الخطوة 2.2
معايير القبول	dotnet ef database update ينشئ كل الـ 31 جدول مع PKs و FKs وفهارس
تقدير الجهد	30 دقيقة
الخطوة 3.2 — تنفيذ Seeders واستدعاءها
العنصر	التفاصيل
الهدف	تنفيذ زرع المستخدم الافتراضي Admin والإعدادات النظام، وإنشاء موقع الاستدعاء
السبب	كلا الـ Seeders يُلقي NotImplementedException ولا يستدعى من أي مكان
الملفات	src/MasrLab.Infrastructure/Pasistence/Seeding/DefaultAdminSeeder.cs + DefaultSettingsSeeder.cs + موقع الاستدعاء في App.xaml.cs أو MasrLabDbContext.cs
الأولوية	🟡 متوسطة
الاعتمادية	الخطوة 3.1
معايير القبول	المستخدم Admin وسجل SystemSetting موجودان بعد تحديث قاعدة البيانات؛ SeedAsync يستدعى مرة واحدة على الأقل
تقدير الجهد	30 دقيقة
تنفيذ:

تنفيذ كلا الدالتين SeedAsync بمنطق زرع فعلي
إنشاء موقع الاستدعاء — إما في OnModelCreating (تلقائي) أو في App.xaml.cs بعد تحديث قاعدة البيانات (صريح)
الخطوة 3.3 — ملء MappingProfile
العنصر	التفاصيل
الهدف	إضافة CreateMap<>() لكل أزواج الكيان ↔ DTO
السبب	بدونها لا يمكن AutoMapper تحويل البيانات بين الكيانات و DTOs
الملفات	src/MasrLab.Application/Common/Mappings/MappingProfile.cs
الأولوية	🟠 عالية
الاعتمادية	الخطوة 1.2
معايير القبول	IMapper.Map<PatientDto>(patient) يعمل
تقدير الجهد	1 ساعة
المرحلة 4: التحضير لمنطق الأعمال
الخطوة 4.1 — إضافة خصائص Commands/Queries
العنصر	التفاصيل
الهدف	تعريف عقود البيانات لكل Command/Query
السبب	كل الـ 30+ Commands/Queries سجلات فارغة بدون خصائص
الملفات	جميع ملفات Commands و Queries في 19 وحدة ميزة
الأولوية	🔴 حرجة
الاعتمادية	لا توجد (متوازية مع المراحل 1–3)
معايير القبول	RegisterPatientCommand يحتوي Name و Age و Gender...
تقدير الجهد	2–3 ساعات
الخطوة 4.2 — ملء Validators
العنصر	التفاصيل
الهدف	إضافة قواعد تحقق لكل مُحقق
السبب	كل المحققات أجسام فارغة
الملفات	جميع ملفات Validators
الأولوية	🟠 عالية
الاعتمادية	الخطوة 4.1
معايير القبول	المحققات ترفض المدخلات غير الصحيحة
تقدير الجهد	1–2 ساعات
الخطوة 4.3 — توسيع واجهات المستودعات
العنصر	التفاصيل
الهدف	إضافة أساليب استعلام متخصصة لواجهات المستودعات
السبب	6 من 9 واجهات فارغة بدون أعضاء
الملفات	Domain/Interfaces/*.cs + Infrastructure/Pasistence/Repositories/*.cs
الأولوية	🟠 عالية
الاعتمادية	لا توجد
معايير القبول	IPatientRepository.SearchAsync(criteria) موجود
تقدير الجهد	1–2 ساعات
الخطوة 4.4 — إضافة IBackupService / IBarcodeService
العنصر	التفاصيل
الهدف	تعريف عقود الخدمات في طبقة Application
السبب	BackupService و BarcodeService موجودتان كطبقات ملموسة بدون واجهة
الملفات	جديدة في Application/Common/Interfaces/ + تحديث Infrastructure DI
الأولوية	🟡 متوسطة
الاعتمادية	لا توجد
معايير القبول	الخدمات قابلة للحقن عبر الواجهات
تقدير الجهد	15 دقيقة
الخطوة 4.5 — إصلاح IStatisticsRepository
العنصر	التفاصيل
الهدف	إضافة النوع الأساسي (IRepository<T>) وأساليب الاستعلام
السبب	الواجهة فارغة تماماً بدون نوع أساسي وأعضاء
الملفات	IStatisticsRepository.cs + StatisticsRepository.cs
الأولوية	🟠 عالية
الاعتمادية	لا توجد
معايير القبول	الواجهة لها نوع أساسي وأعضاء
تقدير الجهد	30 دقيقة
المرحلة 5: بنية الاختبارات
الخطوة 5.1 — استبدال PlaceholderTests
العنصر	التفاصيل
الهدف	إعداد fixtures و builders وأول اختبارات حقيقية
السبب	كل ملفات الاختبارات تحتوي [Fact] فارغ بدون أي assertions
الملفات	مشاريع الاختبارات الثلاث
الأولوية	🟠 عالية
الاعتمادية	الخطوات 1.1–1.4
معايير القبول	dotnet test يعمل باختبارات ذات معنى
تقدير الجهد	2–4 ساعات
ترتيب التنفيذ والتوازي
المرحلة 1 (تسلسلية مع عناصر متوازية):
  1.1 إعدادات EF ──→ 1.3 DI Infrastructure ──→ 1.4 UnitOfWork ──→ 1.5 BaseEntity ──→ 1.6 المراقبين
                                    ↑
                        1.2 DI Application (متوازية مع 1.1)
                                    ↑
                        2.2 appsettings.Local.json (متوازية مع 1.1)

المرحلة 2 (بعد المرحلة 1):
  2.1 ValidationBehavior (بعد 1.2)
  2.2 appsettings.Local.json (مستقلة — يجب إنجازها قبل اختبار 1.3 وتنفيذ 2.3)
  2.3 App.xaml.cs (بعد 1.2 و 1.3 و 2.2)
  2.4 DI Presentation (مستقلة)

المرحلة 3 (بعد المرحلة 1):
  3.1 الترجمة الأولى (بعد 1.1 و 1.3 و 2.2)
  3.2 Seeders + الاستدعاء (بعد 3.1)
  3.3 MappingProfile (بعد 1.2)

المرحلة 4 (متوازية مع المرحلة 1):
  4.1 خصائص Commands/Queries (مستقلة)
  4.2 Validators (بعد 4.1)
  4.3 توسيع المستودعات (مستقلة)
  4.4 IBackupService/IBarcodeService (مستقلة)
  4.5 إصلاح IStatisticsRepository (مستقلة)

المرحلة 5 (بعد المراحل 1–3):
  5.1 بنية الاختبارات
المسار الحرج: 2.2 → 1.3 → 1.4 → 1.5 → 1.6 → 3.1 → 3.2 → 2.3 → جاهز لمنطق الأعمال

العناصر القابلة للتوازي: الخطوات 1.1 و 1.2 و 2.4 و 4.1 و 4.2 و 4.3 و 4.4 و 4.5 يمكن أن تعمل في آن واحد.

الحد الأدنى للخطوات لجعل المشروع جاهزاً
الخطوة	الجهد	الأثر
1.1 تفعيل ApplyConfigurationsFromAssembly + ملء 31 إعداد	6–10 ساعات	يفتح EF Core
1.2 AddApplication (MediatR + AutoMapper + FluentValidation)	30 دقيقة	يفتح CQRS
1.3 AddInfrastructure (IConfiguration + DbContext + المراقبين + UnitOfWork)	1–2 ساعة	يفتح الوصول للبيانات
1.4 تنفيذ UnitOfWork	30 دقيقة	يفتح الحفظ
1.5 BaseEntity تنفيذ الواجهات	5 دقائق	يُوضّح تصميم المراقبين
1.6 تنفيذ المراقبين	1–2 ساعة	يُفعّل التدقيق والحذف الناعم
2.1 إصلاح ValidationBehavior	30 دقيقة	يُفعّل التحقق
2.2 إنشاء appsettings.Local.json + CopyToOutput	15 دقيقة	يفتح الاتصال بقاعدة البيانات
2.3 إكمال App.xaml.cs	30 دقيقة	يجعل التطبيق قابلاً للتشغيل
3.1 إنشاء الترجمة الأولى	30 دقيقة	ينشئ مخطط قاعدة البيانات
المجموع	~11–15 ساعة	يفتح أول معالج
أول حالة استخدام بعد Sprint 0
PatientManagement/Commands/RegisterPatient — يستخدم IPatientRepository و IUnitOfWork و LabIdGenerator و AutoMapper و FluentValidation و MediatR و MasrLabDbContext.

المخاطر
الخطر	الأثر	الاحتمال
حقول التدقيق تبقى (0001-01-01)	انتهاك سلامة البيانات	مؤكد
حذف حقيقي بدلاً من ناعم	فقدان بيانات	مؤكد
التحقق لا يعمل	تلف بيانات	مؤكد
AutoMapper يُلقي استثناء أثناء التشغيل	انهيار Runtime	مؤكد
لا توجد قاعدة بيانات للاختبار againstها	ثقة صفرية	مؤكد
UnitOfWork يُلقي استثناء	لا شيء يُحفظ	مؤكد
ت drifted إصدارات EF Core بين الأجهزة	عدم توافق الترجمات	مرتفع
جهد مهدر للمطورين	تأثير معنوي	مرتفع
التحقق بعد اكتمال Sprint 0
#	التحقق	معيار النجاح
1	dotnet build	0 أخطاء، 0 تحذيرات مرتبطة بتعديلات Sprint 0
2	dotnet ef migrations add InitialCreate	ملف ترجمة يُولد بـ 31 جدول
3	dotnet ef database update	قاعدة بيانات تُنشأ بكل الجداول والمفاتيح والفهارس
4	dotnet run (Presentation)	MainWindow تُفتح بدون انهيار
5	اختبار معالج RegisterPatient	خط أنابيب CQRS يعمل بالكامل من النهاية إلى النهاية
6	dotnet test	جميع الاختبارات تنجح
خلاصة
هل الخطة كاملة؟ نعم.
هل الخطة متسقة؟ نعم.
هل توجد فجوات تمنع التنفيذ؟ لا.
هل توجد تعديلات حرجة متبقية؟ لا.
هل يمكن البدء في التنفيذ بهذه النسخة فقط؟ نعم.
بعد اكتمال Sprint 0، هل يصبح المشروع جاهزاً لكتابة منطق الأعمال؟ نعم — شريطة نجاح جميع عمليات التحقق أعلاه.
