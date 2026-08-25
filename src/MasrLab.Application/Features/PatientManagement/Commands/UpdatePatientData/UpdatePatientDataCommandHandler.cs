using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public class UpdatePatientDataCommandHandler : IRequestHandler<UpdatePatientDataCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePatientDataCommandHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdatePatientDataCommand request, CancellationToken cancellationToken)
    {
        _ = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            throw new EntityNotFoundException(nameof(Patient), request.Id);

        patient.UpdateProfile(request.Name, request.Address, request.Notes, request.NationalId);

        patient.Age = new Age(request.AgeYears, request.AgeMonths, request.AgeDays, request.AgeUnit);
        patient.Gender = request.Gender;
        patient.Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null;
        patient.DoctorId = request.DoctorId;
        patient.ReferralEntityId = request.ReferralEntityId;
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

        _patientRepository.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
