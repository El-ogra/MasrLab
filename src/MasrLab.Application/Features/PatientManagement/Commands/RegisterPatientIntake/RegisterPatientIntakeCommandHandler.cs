using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;
using MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;

public sealed class RegisterPatientIntakeCommandHandler : IRequestHandler<RegisterPatientIntakeCommand, RegisterPatientIntakeResult>
{
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RegisterPatientIntakeCommandHandler(
        ISender sender,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public Task<RegisterPatientIntakeResult> Handle(
        RegisterPatientIntakeCommand request,
        CancellationToken cancellationToken)
    {
        _ = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        return _unitOfWork.ExecuteInTransactionAsync(
            ct => ExecuteAsync(request, ct),
            cancellationToken);
    }

    private async Task<RegisterPatientIntakeResult> ExecuteAsync(
        RegisterPatientIntakeCommand request,
        CancellationToken cancellationToken)
    {
        var registration = await _sender.Send(
            new RegisterPatientCommand(
                request.Name,
                request.AgeYears,
                request.AgeMonths,
                request.AgeDays,
                request.AgeUnit,
                request.Gender,
                request.Phone,
                request.Address,
                request.NationalId,
                request.Notes,
                request.LabId,
                request.DoctorId,
                request.ReferralEntityId,
                request.HasDiabetes,
                request.OnBloodPressureTreatment,
                request.OnAntiviralTreatment,
                request.OnAntibiotic,
                request.BloodThinning,
                request.HasLiverDisease,
                request.HasAnemia,
                request.HasLupus,
                request.HasRenalFailure,
                request.HasHypertension,
                request.HasJointDisease,
                request.RecentContrastOrUltrasound,
                request.ConfirmDuplicate),
            cancellationToken);

        if (!registration.IsRegistered)
        {
            return new RegisterPatientIntakeResult(
                false,
                null,
                null,
                0,
                registration.PotentialDuplicates);
        }

        var patientId = registration.PatientId
            ?? throw new InvalidOperationException("Registered patient did not return an identifier.");

        var visitId = await _sender.Send(
            new CreatePatientVisitCommand(
                patientId,
                request.DoctorId,
                request.ReferralEntityId,
                request.TakenOutsideLab,
                request.SpecimenUrine,
                request.SpecimenStool,
                request.SpecimenBlood,
                request.SpecimenSemen,
                request.SpecimenCsf),
            cancellationToken);

        if (ShouldAttachTests(request))
        {
            await _sender.Send(
                new AddTestsToVisitCommand(
                    visitId,
                    request.Source,
                    request.TestGroupId,
                    request.CommercialPackageId,
                    request.DirectTestIds,
                    request.ConfirmLargeExpansion),
                cancellationToken);
        }

        var attachedTestCount = await _sender.Send(
            new GetVisitTestCountQuery(visitId),
            cancellationToken);

        return new RegisterPatientIntakeResult(
            true,
            patientId,
            visitId,
            attachedTestCount,
            registration.PotentialDuplicates);
    }

    private static bool ShouldAttachTests(RegisterPatientIntakeCommand request)
    {
        if (request.Source is "Direct" or "LegacyGroup")
            return !string.IsNullOrWhiteSpace(request.DirectTestIds);

        return true;
    }
}
