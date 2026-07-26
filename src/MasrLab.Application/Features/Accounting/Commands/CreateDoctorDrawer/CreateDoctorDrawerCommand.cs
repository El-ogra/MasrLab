using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreateDoctorDrawer;

public record CreateDoctorDrawerCommand : IRequest<Unit>;
