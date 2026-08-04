# DD-09 — عقد IAuthenticationService

- **الحالة:** مقبول (سُلِّم العقد في المرحلة 3، commit `a5025e4`)
- **التاريخ:** 2026-08-04

## السياق (Context)

- `IAuthenticationService` في `src/MasrLab.Application/Common/Interfaces/` كانت **واجهة فارغة تماماً بلا أعضاء**.
- `src/MasrLab.Infrastructure/Services/AuthenticationService.cs` تدّعي تنفيذها لكن البناء كان ينجح فقط لأن الواجهة فارغة (خطر تسمية Naming collision مُوثَّق في خارطة الطريق §2.2).
- Handlers الحضور والانصراف (`RecordLoginCommandHandler`, `RecordLogoutCommandHandler`, `RecordBreakCommandHandler`) تحتاج عقداً ملموساً للتعامل مع تسجيل الدخول/الخروج.
- العقد المقترح استُخلص من `LoginViewModel` ومن هيكل `Infrastructure/Services/AuthenticationService`.

## القرار (Decision)

اعتماد العقد التالي في `MasrLab.Application` (واجهة العقد المرجعية الوحيدة):

```csharp
public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);
}
```

نوع الإرجاع هو الـ record الموجود مسبقاً:

```csharp
public record AuthResult(int UserId, string Username, IReadOnlyList<string> Permissions);
```

## البدائل المطروحة (Alternatives Considered)

- **إبقاء الواجهة فارغة والاعتماد على فئة ملموسة داخل الـ Handlers:** مرفوض — يهدم التجريد الذي يعتمد عليه الفصل بين الطبقات ويُبقي التعارض التسمي قائماً.
- **أعضاء متزامنة `Login`/`Logout`:** مرفوض — المصادقة عملية I/O، واصطلاح الريبو غير متزامن (async-first).
- **إرجاع كيان Domain أو `ClaimsPrincipal`:** مرفوض — يُسرّب اهتمامات البنية التحتية/المصادقة إلى Application؛ `AuthResult` هو النموذج المحايد المقصود.

## النتائج (Consequences)

- Handlers المرحلة 7 تستدعي `LoginAsync`/`LogoutAsync` مباشرة.
- التطبيق الفعلي يُكمَّل في `Infrastructure/Services/AuthenticationService.cs` (حالياً `NotImplementedException`) ضمن المرحلة 7/10.
- حُلّ التعارض التسمي: الواجهة في Application هي العقد، وفئة Infrastructure هي التنفيذ.

## الروابط (Links)

- `src/MasrLab.Application/Common/Interfaces/IAuthenticationService.cs`
- `src/MasrLab.Application/Common/Models/AuthResult.cs`
- `src/MasrLab.Infrastructure/Services/AuthenticationService.cs`
