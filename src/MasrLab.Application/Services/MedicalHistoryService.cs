using MasrLab.Domain.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// خدمة التاريخ الطبي للمريض — تبني سجل الاختبارات السابقة.
/// ⚠️ ملاحظة: هذه الخدمة تعتمد على IPatientHistoryRepository الذي لا يُنفَّذ حالياً.
/// ستنجح في البناء لكن لا يمكن اختبارها end-to-end حتى تُنفَّذ المرحلة 6a.
/// </summary>
public class MedicalHistoryService : IMedicalHistoryService
{
    private readonly IPatientHistoryRepository _patientHistoryRepo;

    public MedicalHistoryService(IPatientHistoryRepository patientHistoryRepo)
        => _patientHistoryRepo = patientHistoryRepo ?? throw new ArgumentNullException(nameof(patientHistoryRepo));

    /// <summary>
    /// يبني سجل الاختبارات السابقة للمريض.
    /// INV: يعتمد على PatientHistoryView في قاعدة البيانات.
    /// </summary>
    public async Task<IReadOnlyList<PatientHistoryEntry>> BuildHistoryAsync(int patientId, CancellationToken ct = default)
    {
        return await _patientHistoryRepo.GetByPatientIdAsync(patientId, ct);
    }

    /// <summary>
    /// يحدد ما إذا كان يجب إدراج سجل تلقائي عند إدخال نتيجة اختبار.
    /// INV: إذا وُجدت سجلات سابقة للمريض+الاختبار، يُرجع true.
    /// </summary>
    public async Task<bool> ShouldAutoInsertHistoryAsync(int patientId, int testId, CancellationToken ct = default)
    {
        var history = await _patientHistoryRepo.GetByPatientAndTestAsync(patientId, testId, ct);
        return history.Any();
    }
}
