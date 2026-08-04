# DD-10 — عقد IPrintService

- **الحالة:** مقبول (سُلِّم العقد في المرحلة 3، commit `a5025e4`)
- **التاريخ:** 2026-08-04

## السياق (Context)

- `IPrintService` في `src/MasrLab.Application/Common/Interfaces/` كانت **واجهة فارغة بلا أعضاء**، و`Infrastructure/Services/PrintService.cs` تطبيقاً فارغاً.
- طبقة العرض لديها مسار تقارير/طباعة (`Printing/Reports/*`) يحتاج عقداً للطباعة، وHandlers إنشاء التقارير يحتاج مسار Render يُرجع بايتات للمعاينة.
- العقد المقترح يعكس ما تفعله أنابيب التقارير في Presentation.

## القرار (Decision)

اعتماد العقد التالي في `MasrLab.Application`:

```csharp
public interface IPrintService
{
    Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default);
    Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default);
}
```

## البدائل المطروحة (Alternatives Considered)

- **ميثودات مخصّصة لكل نوع تقرير (strongly-typed payload):** مرفوض في هذه المرحلة — كان سينشئ مجموعة overloads قبل وجود DTOs التقارير (المرحلة 6)؛ `object payload` يُبقي العقد محايداً تجاه نوع التقرير.
- **أعضاء متزامنة `Print`/`Render`:** مرفوض — طباعة I/O وتوليد التقارير عمليات قابلة للحجب؛ الالتفاف غير المتزامن هو المعيار.
- **`RenderAsync` يُرجع `Stream`:** مرفوض — `byte[]` أبسط لمسار المعاينة/الطباعة في هذا الريبو.

## النتائج (Consequences)

- `object payload` يؤجّل تحقق النوع إلى التنفيذ؛ DTOs التقارير تُضاف في المرحلة 6 وتُمرَّر كما هي.
- Handlers التقارير/الطباعة في المرحلة 7 تستهلك هذا العقد.
- التطبيق الفعلي يُكمَّل في `Infrastructure/Services/PrintService.cs` (حالياً `NotImplementedException`) ضمن المرحلة 7/10.

## الروابط (Links)

- `src/MasrLab.Application/Common/Interfaces/IPrintService.cs`
- `src/MasrLab.Infrastructure/Services/PrintService.cs`
