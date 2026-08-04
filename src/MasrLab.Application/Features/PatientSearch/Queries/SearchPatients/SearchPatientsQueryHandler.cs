using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, IReadOnlyList<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public SearchPatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IReadOnlyList<PatientDto>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await _patientRepository.SearchByNameAsync(request.SearchTerm);

        return patients.Select(p => new PatientDto
        {
            Id = p.Id,
            Name = p.Name,
            AgeYears = p.Age.Years,
            AgeMonths = p.Age.Months,
            AgeDays = p.Age.Days,
            Gender = p.Gender,
            Phone = p.Phone?.Value,
            Address = p.Address,
            NationalId = p.NationalId,
            Notes = p.Notes,
            LabId = p.LabId,
            DoctorId = p.DoctorId,
            ReferralEntityId = p.ReferralEntityId,
            AccountType = p.AccountType,
            DrugAllergy = p.DrugAllergy,
            Pregnancy = p.Pregnancy,
            BloodThinning = p.BloodThinning,
            HasDiabetes = p.HasDiabetes,
            HasHypertension = p.HasHypertension,
            HasLiverDisease = p.HasLiverDisease,
            HasJointDisease = p.HasJointDisease,
            HasRenalFailure = p.HasRenalFailure,
            HasLupus = p.HasLupus,
            HasHeartDisease = p.HasHeartDisease,
            HasThyroidDisorder = p.HasThyroidDisorder,
            ChronicDiseases = p.ChronicDiseases
        }).ToList();
    }
}
