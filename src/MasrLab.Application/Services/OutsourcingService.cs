using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// خدمة العينات المرسلة للمختبرات الخارجية.
/// INV: OutsourcedSample.SetPrices() يفرض patientPrice >= costPrice (OutsourcedSample.cs:20).
/// INV: OutsourcedSample.ReceiveResult() يفرض ReceivedAt == null (OutsourcedSample.cs:38).
/// </summary>
public class OutsourcingService : IOutsourcingService
{
    private readonly IVisitRepository _visits;
    private readonly IRepository<OutsourcedSample> _outsourcedSamples;
    private readonly IUnitOfWork _unitOfWork;

    public OutsourcingService(
        IVisitRepository visits,
        IRepository<OutsourcedSample> outsourcedSamples,
        IUnitOfWork unitOfWork)
    {
        _visits = visits ?? throw new ArgumentNullException(nameof(visits));
        _outsourcedSamples = outsourcedSamples ?? throw new ArgumentNullException(nameof(outsourcedSamples));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// ينشئ عينة مرسلة جديدة ويسجلها.
    /// INV: يستدعي SetPrices() لضمان patientPrice >= costPrice.
    /// </summary>
    public async Task<OutsourcedSample> CreateOutsourcedSampleAsync(
        int patientVisitId, int testId, int externalLabId, decimal costPrice, decimal patientPrice)
    {
        var sample = new OutsourcedSample
        {
            PatientVisitId = patientVisitId,
            TestId = testId,
            ExternalLabId = externalLabId
        };

        sample.SetPrices(patientPrice, costPrice);

        await _outsourcedSamples.AddAsync(sample);
        await _unitOfWork.SaveChangesAsync();

        return sample;
    }

    /// <summary>
    /// يسجل استلام نتيجة العينة المرسلة.
    /// INV: يستدعي ReceiveResult() لضمان عدم الاستلام المزدوج.
    /// </summary>
    public async Task ReceiveOutsourcedResultAsync(int outsourcedSampleId)
    {
        var sample = await _outsourcedSamples.GetByIdAsync(outsourcedSampleId);
        if (sample is null)
            return;

        sample.ReceiveResult();
        _outsourcedSamples.Update(sample);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// يكمل تسوية حساب العينة المرسلة.
    /// INV: يستدعي CompleteSettlement() — يجب أن تكون الحالة PartiallySettled.
    /// </summary>
    public async Task SettleOutsourcedAccountAsync(int outsourcedSampleId)
    {
        var sample = await _outsourcedSamples.GetByIdAsync(outsourcedSampleId);
        if (sample is null)
            return;

        sample.CompleteSettlement();
        _outsourcedSamples.Update(sample);
        await _unitOfWork.SaveChangesAsync();
    }
}
