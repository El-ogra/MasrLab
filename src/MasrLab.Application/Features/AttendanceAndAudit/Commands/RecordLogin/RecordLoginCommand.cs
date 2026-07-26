using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;

public record RecordLoginCommand(int UserId, DateTime LoginTime) : IRequest<Unit>;
