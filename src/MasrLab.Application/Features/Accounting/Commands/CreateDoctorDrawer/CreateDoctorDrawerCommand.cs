using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreateDoctorDrawer;

public record CreateDoctorDrawerCommand(DateTime PeriodStart, DateTime PeriodEnd, int DoctorId) : IRequest<Unit>;
