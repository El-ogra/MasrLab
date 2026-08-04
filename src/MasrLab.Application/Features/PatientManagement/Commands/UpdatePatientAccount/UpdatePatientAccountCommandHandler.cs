using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;

public class UpdatePatientAccountCommandHandler : IRequestHandler<UpdatePatientAccountCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientAccountCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePatientAccountCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id);
        if (patient is null)
            throw new InvalidOperationException($"Patient with Id {request.Id} not found.");

        patient.AccountType = request.AccountType;

        _patientRepository.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
