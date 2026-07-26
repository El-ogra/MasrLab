using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordBreak;

public record RecordBreakCommand : IRequest<Unit>;
