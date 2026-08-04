# DD-08 — القرار المعماري للطبقات وتموضع LabIdGenerator

- **الحالة:** مقبول (معاد تأليفه في المرحلة 4.5 بعد التحقق من أنه لم يُكتب على القرص في أي فرع/commit سابق)
- **التاريخ:** 2026-08-04
- **المصادر المستندة إليها (أدلة):** `src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs` (تعليق `summary`) + `Docs/Application-Layer-Implementation-Roadmap.md` §4 (سطر 450) و §2.2 (سطر 226) و §7.3 (سطر 1053)

## السياق (Context)

- خارطة الطريق الأصلية ذكرت `Docs/DecisionRecords/` على أنه يحوي `DD-08 (موجود)`، لكن البحث الكامل في تاريخ الـ git (`git log --all --full-history`) أثبت أن **لا ملف ولا مجلد باسم DD-08 أو DecisionRecords وُجد في أي commit على أي فرع** — أي أن القرار صِيغ كقاعدة معمارية مطبَّقة في الكود دون أن يُوثَّق فعلياً.
- التعليق الحالي في `LabIdGenerator.cs` يذكر القرار صراحة: «موضعه في طبقة Application بقرار DD-08، لأنه يعتمد على IPatientRepository (تحقق من التفرّد مقابل المخزَّن) وليس دالة صرفة — فلا يصلح لطبقة Domain، ولا يجوز وضعه في Presentation/Helpers».
- خارطة الطريق الأصلية §4 تذكر القرار كإطار معماري شامل: «العقود في Domain، التنفيذ في Application، الوصول لقاعدة البيانات عبر Repository Interfaces حصراً».

## القرار (Decision)

1. **قاعدة الطبقات (Layering rule):**
   - **العقود (Contracts) في طبقة Domain:** واجهات الخدمات مثل `ICultureSensitivityService` وغيرها تُعرَّف في `MasrLab.Domain/Services/`.
   - **التنفيذ (Implementations) في طبقة Application:** التطبيقات الملموسة تُكتب في `MasrLab.Application/Services/`.
   - **الوصول لقاعدة البيانات حصراً عبر Repository Interfaces:** لا تصل طبقة Application مباشرة إلى `DbContext`؛ كل وصول يتم عبر الواجهات في `MasrLab.Domain/Interfaces/` (مثل `IPatientRepository`, `IRepository<T>`, `IUnitOfWork`).
2. **تموضع `LabIdGenerator` في طبقة Application (وليس Domain ولا Presentation):**
   - لأنه يعتمد على `IPatientRepository` للتحقق من تفرّد الرقم مقابل المخزَّن، فهو **ليس دالة صرفة** — لذلك لا يصلح لطبقة Domain.
   - ولا يجوز وضعه في `Presentation/Helpers` لأن طبقة العرض لا تملك وصولاً مباشراً للمخزَّن.
   - الاستقرار النهائي: `MasrLab.Application/Common/Helpers/LabIdGenerator.cs` مع `GenerateAsync(CancellationToken)`.
   - (إلغاء مجلد `Helpers` من طبقة العرض كان تنفيذاً لهذا القرار؛ راجع تاريخ `feature/structure-alignment` في سلسلة ترقيم مستقلة غير متعلقة بهذا القرار.)

## البدائل المطروحة (Alternatives Considered)

- **وضع `LabIdGenerator` في Domain كدالة صرفة:** مرفوض — التوليد يتطلب التحقق من التفرّد مقابل قاعدة البيانات، فلا يتحقق كدالة نقيّة.
- **وضع `LabIdGenerator` في Presentation/Helpers:** مرفوض — يتجاوز حدود الطبقات ويصل طبقة العرض للمخزَّن.
- **تنفيذ خدمات Domain داخل Domain نفسها:** مرفوض — الانفصال السليم هو العقود في Domain والتنفيذ في Application، مع منع أي وصول مباشر لقاعدة البيانات من الطبقة العليا.

## النتائج (Consequences)

- طبقة Application تملك التطبيقات الملموسة للخدمات وتعتمد على واجهات Repositories فقط؛ هذا هو الأساس الذي بُني عليه تنفيذ الخدمات العشر في `MasrLab.Application/Services/` (المرحلة 4).
- `LabIdGenerator.GenerateAsync` يبقى في Application؛ منطق توليده الفعلي يُنجز في مرحلة Business Logic (Phase 7) — حالياً `throw new NotImplementedException()`.
- أي تطبيق مستقبلي داخل Infrastructure يُسجَّل عبر DI في الطبقة المناسبة دون كسر قاعدة الطبقات.

## الروابط (Links)

- `src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs`
- `src/MasrLab.Domain/Services/` (واجهات الخدمات العشر)
- `src/MasrLab.Application/Services/` (التطبيقات الملموسة)
- `src/MasrLab.Domain/Interfaces/` (واجهات Repositories)
- `Docs/Application-Layer-Implementation-Roadmap.md` §4، §2.2
