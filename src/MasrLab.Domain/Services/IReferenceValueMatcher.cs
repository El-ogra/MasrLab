using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Services;

public interface IReferenceValueMatcher
{
    ReferenceMatchResult Match(
        IReadOnlyList<ReferenceValue> candidates,
        int testComponentId,
        string? patientGender,
        Age patientAge,
        bool isPregnant);
}
