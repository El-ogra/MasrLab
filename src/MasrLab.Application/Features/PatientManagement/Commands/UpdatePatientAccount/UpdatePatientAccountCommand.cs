using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;

public record UpdatePatientAccountCommand : IRequest<Unit>;
