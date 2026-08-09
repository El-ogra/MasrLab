using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// حسابات أسعار الزيارة من لقطات الأسعار المحفوظة على VisitTest.
/// </summary>
public class PricingService : IPricingService
{
    /// <summary>
    /// يحسب المجموع الفرعي لجميع اختبارات الزيارة.
    /// INV: يقرأ فقط من VisitTest.Price (محفوظ كـ snapshot عند إضافة الاختبار).
    /// </summary>
    public decimal CalculateSubtotal(PatientVisit visit)
    {
        return visit.VisitTests.Sum(vt => vt.Price);
    }

    /// <summary>
    /// يحسب الإجمالي النهائي مع الخدمات الإضافية والخصم.
    /// INV: الإجمالي لا يقل عن صفر (Math.Max).
    /// </summary>
    public decimal CalculateTotal(PatientVisit visit, decimal extraServicesTotal, decimal discount)
    {
        var subtotal = CalculateSubtotal(visit);
        var total = subtotal + extraServicesTotal - discount;
        return Math.Max(0m, total);
    }
}
