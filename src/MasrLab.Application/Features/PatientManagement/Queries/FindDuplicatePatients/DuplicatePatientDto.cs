namespace MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;

public sealed record DuplicatePatientDto(
    int Id,
    string Name,
    string LabId,
    string? NationalId,
    string? Phone);

public sealed record RegisterPatientResult(
    bool IsRegistered,
    IReadOnlyList<DuplicatePatientDto> PotentialDuplicates,
    int? PatientId = null)
{
    public bool HasPotentialDuplicates => PotentialDuplicates.Count > 0;
}
