using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;

public class CreatePatientVisitCommandHandler : IRequestHandler<CreatePatientVisitCommand, int>
{
    private const int MaxLabIdRetries = 5;
    private const int RetryBaseDelayMilliseconds = 50;

    private readonly IPatientRepository _patientRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVisitLabIdGenerator _visitLabIdGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreatePatientVisitCommandHandler(
        IPatientRepository patientRepository,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IVisitLabIdGenerator visitLabIdGenerator,
        ICurrentUserService currentUserService)
    {
        _patientRepository = patientRepository;
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
        _visitLabIdGenerator = visitLabIdGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreatePatientVisitCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
            throw new EntityNotFoundException(nameof(Patient), request.PatientId);

        var doctorId = request.DoctorId ?? patient.DoctorId;
        var labId = await _visitLabIdGenerator.GenerateAsync(cancellationToken);
        var registeredByUserId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        var visit = PatientVisit.Create(
            request.PatientId,
            registeredByUserId,
            labId,
            doctorId,
            request.ReferralEntityId);

        await _visitRepository.AddAsync(visit, cancellationToken);

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return visit.Id;
            }
            catch (DuplicateVisitLabIdException) when (attempt < MaxLabIdRetries - 1)
            {
                var delayMilliseconds = (RetryBaseDelayMilliseconds * (1 << attempt)) + Random.Shared.Next(0, 31);
                await Task.Delay(delayMilliseconds, cancellationToken);
                labId = await _visitLabIdGenerator.GenerateAsync(cancellationToken);
                visit.LabId = labId;
            }
        }
    }
}
