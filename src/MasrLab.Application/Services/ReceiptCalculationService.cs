using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// حسابات الإيصال — خدمة حسابية بحتة لا تحتاج قاعدة بيانات.
/// </summary>
public class ReceiptCalculationService : IReceiptCalculationService
{
    /// <summary>
    /// يحسب المبلغ المتبقي بعد الخصم والمدفوعات.
    /// </summary>
    /// <param name="total">الإجمالي قبل الخصم.</param>
    /// <param name="paid">المبلغ المدفوع.</param>
    /// <param name="discount">قيمة الخصم.</param>
    /// <returns>المبلغ المتبقي (لا يقل عن صفر).</returns>
    public decimal CalculateRemaining(decimal total, decimal paid, decimal discount)
    {
        var remaining = total - paid - discount;
        return remaining < 0 ? 0 : remaining;
    }

    /// <summary>
    /// يحسب المبلغ المستحق كـ فرق (Change) للمريض.
    /// </summary>
    /// <param name="paid">المبلغ المدفوع.</param>
    /// <param name="total">الإجمالي المطلوب.</param>
    /// <returns>المبلغ المستحق — موجب يعني فرق للمريض، سالب يعني لم يدفع بالكامل.</returns>
    public decimal CalculateChangeDue(decimal paid, decimal total)
    {
        return paid - total;
    }
}
