using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public class UpdatePatientDataCommandHandler : IRequestHandler<UpdatePatientDataCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientDataCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePatientDataCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id);
        if (patient is null)
            throw new InvalidOperationException($"Patient with Id {request.Id} not found.");

        patient.UpdateProfile(request.Name, request.Address, request.Notes, request.NationalId);

        patient.Age = new Age(request.AgeYears, request.AgeMonths, request.AgeDays);
        patient.Gender = request.Gender;
        patient.Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null;
        patient.DoctorId = request.DoctorId;
        patient.ReferralEntityId = request.ReferralEntityId;
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

        _patientRepository.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
