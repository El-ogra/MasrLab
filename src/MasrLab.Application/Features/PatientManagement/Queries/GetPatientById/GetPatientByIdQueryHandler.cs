using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientManagement.Queries.GetPatientById;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.GetPatientById;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto?>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDto?> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id);
        if (patient is null)
            return null;

        return new PatientDto
        {
            Id = patient.Id,
            Name = patient.Name,
            AgeYears = patient.Age.Years,
            AgeMonths = patient.Age.Months,
            AgeDays = patient.Age.Days,
            Gender = patient.Gender,
            Phone = patient.Phone?.Value,
            Address = patient.Address,
            NationalId = patient.NationalId,
            Notes = patient.Notes,
            LabId = patient.LabId,
            DoctorId = patient.DoctorId,
            ReferralEntityId = patient.ReferralEntityId,
            AccountType = patient.AccountType,
            DrugAllergy = patient.DrugAllergy,
            Pregnancy = patient.Pregnancy,
            BloodThinning = patient.BloodThinning,
            HasDiabetes = patient.HasDiabetes,
            HasHypertension = patient.HasHypertension,
            HasLiverDisease = patient.HasLiverDisease,
            HasJointDisease = patient.HasJointDisease,
            HasRenalFailure = patient.HasRenalFailure,
            HasLupus = patient.HasLupus,
            HasHeartDisease = patient.HasHeartDisease,
            HasThyroidDisorder = patient.HasThyroidDisorder,
            ChronicDiseases = patient.ChronicDiseases
        };
    }
}
