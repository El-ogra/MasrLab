using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, RegisterPatientResult>
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

    public async Task<RegisterPatientResult> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = Patient.Register(request.Name, request.LabId, request.DoctorId, request.ReferralEntityId);

        var duplicatePatients = await _patientRepository.FindProbableDuplicatesAsync(
            request.Name,
            request.NationalId,
            request.Phone,
            cancellationToken);

        var potentialDuplicates = duplicatePatients
            .Select(patient => new DuplicatePatientDto(
                patient.Id,
                patient.Name,
                patient.LabId,
                patient.NationalId,
                patient.Phone?.Value))
            .ToList();

        if (potentialDuplicates.Count > 0 && !request.ConfirmDuplicate)
        {
            return new RegisterPatientResult(false, potentialDuplicates);
        }

        patient.Age = new Age(request.AgeYears, request.AgeMonths, request.AgeDays, request.AgeUnit);
        patient.Gender = request.Gender;
        patient.Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null;
        patient.Address = request.Address;
        patient.NationalId = request.NationalId;
        patient.Notes = request.Notes;
        patient.HasDiabetes = request.HasDiabetes;
        patient.OnBloodPressureTreatment = request.OnBloodPressureTreatment;
        patient.OnAntiviralTreatment = request.OnAntiviralTreatment;
        patient.OnAntibiotic = request.OnAntibiotic;
        patient.BloodThinning = request.BloodThinning;
        patient.HasLiverDisease = request.HasLiverDisease;
        patient.HasAnemia = request.HasAnemia;
        patient.HasLupus = request.HasLupus;
        patient.HasRenalFailure = request.HasRenalFailure;
        patient.HasHypertension = request.HasHypertension;
        patient.HasJointDisease = request.HasJointDisease;
        patient.RecentContrastOrUltrasound = request.RecentContrastOrUltrasound;
        await _patientRepository.AddAsync(patient, cancellationToken);

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new RegisterPatientResult(true, potentialDuplicates, patient.Id);
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
