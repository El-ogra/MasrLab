using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// تتبع حالة جمع العينات — يتحقق مما إذا تم جمع عينة معينة.
/// INV:Sample.CollectionStatus يُ changed فقط عبر Sample.Collect() و Sample.RevertCollection().
/// </summary>
public class SampleTrackingService : ISampleTrackingService
{
    private readonly ISampleRepository _samples;

    public SampleTrackingService(ISampleRepository samples)
        => _samples = samples ?? throw new ArgumentNullException(nameof(samples));

    /// <summary>
    /// يتحقق مما إذا تم جمع العينة المرتبطة باختبار معين في زيارة معينة.
    /// INV: لا يُعيّن CollectionStatus مباشرة — يقرأ فقط.
    /// </summary>
    public async Task<bool> IsSampleCollectedAsync(int patientVisitId, int testId, CancellationToken ct = default)
    {
        var samples = await _samples.GetByPatientVisitAndTestAsync(new[] { patientVisitId }, testId, ct);
        var sample = samples.FirstOrDefault();
        return sample is not null && sample.CollectionStatus == SampleStatus.Collected;
    }

    /// <summary>
    /// يعدد العينات التي لم تُجمع بعد في زيارة معينة.
    /// INV: يقرأ CollectionStatus فقط — لا يُعدّل أي حالة.
    /// </summary>
    public async Task<int> GetUncollectedSamplesCountAsync(int visitId, CancellationToken ct = default)
    {
        var samples = await _samples.GetPendingAsync(visitId, ct);
        return samples.Count;
    }
}
