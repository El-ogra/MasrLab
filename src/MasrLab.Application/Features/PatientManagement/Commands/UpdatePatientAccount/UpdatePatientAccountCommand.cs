using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;

public record UpdatePatientAccountCommand(
    int Id,
    AccountType AccountType
) : IRequest<Unit>;
