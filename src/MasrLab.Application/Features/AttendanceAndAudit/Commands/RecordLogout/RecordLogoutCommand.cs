using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogout;

public record RecordLogoutCommand : IRequest<Unit>;
