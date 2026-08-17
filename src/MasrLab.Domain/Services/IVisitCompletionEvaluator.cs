using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Services;

public interface IVisitCompletionEvaluator
{
    Task EvaluateAsync(PatientVisit visit, CancellationToken ct = default);
}
