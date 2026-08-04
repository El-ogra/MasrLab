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
        var existingPatients = await _patientRepository.GetAllAsync();
        var existingLabIds = existingPatients
            .Select(p => p.LabId)
            .Where(id => id.StartsWith(prefix))
            .ToList();

        var nextNumber = existingLabIds.Count + 1;
        return $"{prefix}-{nextNumber:D4}";
    }
}
