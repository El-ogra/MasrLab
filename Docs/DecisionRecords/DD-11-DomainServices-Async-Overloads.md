# DD-11 — Overloads غير متزامنة لخدمات الدومين (Domain Services Async Overloads)

- **الحالة:** مقبول (سُلِّم في المرحلة 3، commit `a5025e4`)
- **التاريخ:** 2026-08-04

## السياق (Context)

- أربع واجهات خدمات في طبقة Domain تُعلن أعضاءً **متزامنة** تحتاج قراءة من قاعدة البيانات عبر Repositories.
- تنفيذاتها تستخدم لذلك shims حاجبة: `.GetAwaiter().GetResult()` تظهر 7 مرات عبر `CultureSensitivityService` و`PriceListResolverService` و`ResultValidationService` و`ReferralCommissionService`.
- خارطة الطريق الأصلية §3.2 طالبت بقرار موحَّد (غير مكسور للتوافق) قبل تنفيذ المرحلة 4.

## القرار (Decision)

إضافة **overloads غير متزامنة** إلى الواجهات الأربع مع إبقاء الأعضاء المتزامنة كما هي (توافق رجعي):

| الواجهة | العضو المتزامن | الـ overload غير المتزامن |
|---|---|---|
| `ICultureSensitivityService` | `RecordSensitivity(int, int, int)` | `RecordSensitivityAsync(int, int, int, CancellationToken)` |
| `IPriceListResolverService` | `ResolvePrice(int, int?, int?)` | `ResolvePriceAsync(int, int?, int?, CancellationToken)` |
| `IResultValidationService` | `ValidateResult(int, string, string?, int)` | `ValidateResultAsync(int, string, string?, int, CancellationToken)` |
| `IResultValidationService` | `IsResultInRange(int, string, out string?)` | `IsResultInRangeAsync(int, string, CancellationToken) → Task<(bool IsInRange, string? Comment)>` |
| `IReferralCommissionService` | `CalculateCommission(int?, int?, decimal)` | `CalculateCommissionAsync(int?, int?, decimal, CancellationToken)` |

**استثناءان مقصودان (مؤكَّدان في الواجهات الحية):**

1. `ICultureSensitivityService.GetSensitivitySummary` — **لا** overload غير متزامن: قراءة نقيّة لكائن محمَّل أصلاً (تم تسجيلها كتفاوت مقصود).
2. `IResultValidationService.IsResultInRangeAsync` — تُرجع `Task<(bool IsInRange, string? Comment)>` **وليس** معامل `out`؛ لأن `out` لا يجوز في الطرق `async`، والـ tuple هو البديل الاصطلاحي.

## البدائل المطروحة (Alternatives Considered)

- **تحويل الأعضاء المتزامنة إلى غير متزامنة فقط:** مرفوض — تغيير مكسور في عقود Domain.
- **الاكتفاء بتنفيذ متزامن عبر shim حاجب:** مرفوض — هذه هي الحالة السابقة للقرار؛ V2 (C8) تفرض على الـ Handlers استدعاء الـ `*Async` overloads.
- **الإبقاء على معامل `out` في الصيغة غير المتزامنة:** مرفوض — غير قابل للتعريف داخل طرق `async`.

## النتائج (Consequences)

- الأعضاء المتزامنة تبقى لكنها traps معروفة (`IsResultInRange` المتزامن يتجاهل الجنس/العمر — C8؛ `RecordSensitivity` المتزامن shim فوق async).
- توصيل المرحلة 7 يفرض استدعاء الـ `*Async` overloads حصراً.
- إهمال الأعضاء المتزامنة (إضافة `[Obsolete]`/حذف) مؤجَّل إلى المرحلة 10 وفق القرار المفتوح O-5 في V2.

## الروابط (Links)

- `src/MasrLab.Domain/Services/ICultureSensitivityService.cs`
- `src/MasrLab.Domain/Services/IPriceListResolverService.cs`
- `src/MasrLab.Domain/Services/IResultValidationService.cs`
- `src/MasrLab.Domain/Services/IReferralCommissionService.cs`
- التطبيقات: `src/MasrLab.Application/Services/{CultureSensitivityService, PriceListResolverService, ResultValidationService, ReferralCommissionService}.cs`
