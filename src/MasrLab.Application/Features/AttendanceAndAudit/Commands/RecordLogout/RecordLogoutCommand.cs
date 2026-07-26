using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogout;

public record RecordLogoutCommand(int UserId, int AttendanceLogId, DateTime LogoutTime) : IRequest<Unit>;
