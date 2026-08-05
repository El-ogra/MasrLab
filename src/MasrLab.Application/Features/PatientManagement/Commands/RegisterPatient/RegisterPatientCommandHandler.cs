using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterPatientCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = Patient.Register(request.Name, request.LabId, request.DoctorId, request.ReferralEntityId);

        patient.Age = new Age(request.AgeYears, request.AgeMonths, request.AgeDays);
        patient.Gender = request.Gender;
        patient.Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null;
        patient.Address = request.Address;
        patient.NationalId = request.NationalId;
        patient.Notes = request.Notes;
        patient.AccountType = request.AccountType;
        patient.DrugAllergy = request.DrugAllergy;
        patient.Pregnancy = request.Pregnancy;
        patient.BloodThinning = request.BloodThinning;
        patient.HasDiabetes = request.HasDiabetes;
        patient.HasHypertension = request.HasHypertension;
        patient.HasLiverDisease = request.HasLiverDisease;
        patient.HasJointDisease = request.HasJointDisease;
        patient.HasRenalFailure = request.HasRenalFailure;
        patient.HasLupus = request.HasLupus;
        patient.HasHeartDisease = request.HasHeartDisease;
        patient.HasThyroidDisorder = request.HasThyroidDisorder;
        patient.ChronicDiseases = request.ChronicDiseases;

        await _patientRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
