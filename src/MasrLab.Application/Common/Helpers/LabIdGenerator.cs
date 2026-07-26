using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Common.Helpers;

/// <summary>
/// توليد Lab ID الفريد للمريض.
/// موضعه في طبقة Application بقرار DD-08، لأنه يعتمد على IPatientRepository
/// (تحقق من التفرّد مقابل المخزَّن) وليس دالة صرفة — فلا يصلح لطبقة Domain，
/// ولا يجوز وضعه في Presentation/Helpers.
/// هيكل أساسي: منطق التوليد الفعلي يُكتب في مرحلة Business Logic，
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
