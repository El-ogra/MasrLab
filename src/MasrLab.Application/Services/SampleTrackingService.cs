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
    private readonly IVisitRepository _visits;

    public SampleTrackingService(IVisitRepository visits)
        => _visits = visits ?? throw new ArgumentNullException(nameof(visits));

    /// <summary>
    /// يتحقق مما إذا تم جمع العينة المرتبطة باختبار معين.
    /// INV: لا يُعيّن CollectionStatus مباشرة — يقرأ فقط.
    /// </summary>
    public async Task<bool> IsSampleCollectedAsync(int visitTestId, CancellationToken ct = default)
    {
        var visits = await _visits.GetAllAsync(ct);
        foreach (var visit in visits)
        {
            var sample = visit.Samples.FirstOrDefault(s => s.TestId == visitTestId);
            if (sample is not null)
                return sample.CollectionStatus == SampleStatus.Collected;
        }

        return false;
    }

    /// <summary>
    /// يعدد العينات التي لم تُجمع بعد في زيارة معينة.
    /// INV: يقرأ CollectionStatus فقط — لا يُعدّل أي حالة.
    /// </summary>
    public async Task<int> GetUncollectedSamplesCountAsync(int visitId, CancellationToken ct = default)
    {
        var visit = await _visits.GetByIdAsync(visitId, ct);
        if (visit is null)
            return 0;

        return visit.Samples.Count(s => s.CollectionStatus == SampleStatus.NotCollected);
    }
}
