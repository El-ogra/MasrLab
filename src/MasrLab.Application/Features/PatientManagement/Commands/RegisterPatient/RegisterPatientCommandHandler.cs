using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Unit>
{
    private const int MaxLabIdRetries = 5;
    private const int RetryBaseDelayMilliseconds = 50;

    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LabIdGenerator _labIdGenerator;

    public RegisterPatientCommandHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        LabIdGenerator labIdGenerator)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _labIdGenerator = labIdGenerator;
    }

    public async Task<Unit> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = Patient.Register(request.Name, request.LabId, request.DoctorId, request.ReferralEntityId);

        patient.Age = new Age(request.AgeYears, request.AgeMonths, request.AgeDays, request.AgeUnit);
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

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            catch (DuplicateLabIdException) when (attempt < MaxLabIdRetries - 1)
            {
                var delayMilliseconds = (RetryBaseDelayMilliseconds * (1 << attempt)) + Random.Shared.Next(0, 31);
                await Task.Delay(delayMilliseconds, cancellationToken);
                patient.LabId = await _labIdGenerator.GenerateAsync(cancellationToken);
            }
        }
    }
}
