using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Common.Helpers;

public class LabIdGenerator
{
    private readonly IPatientRepository _patientRepository;

    public LabIdGenerator(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository
            ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var prefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var maxSuffix = await _patientRepository.GetMaxLabIdSuffixAsync(prefix, cancellationToken);
        var nextNumber = (maxSuffix ?? 0) + 1;
        return $"{prefix}-{nextNumber:D4}";
    }
}
