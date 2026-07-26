using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;

public record RecordLoginCommand : IRequest<Unit>;
