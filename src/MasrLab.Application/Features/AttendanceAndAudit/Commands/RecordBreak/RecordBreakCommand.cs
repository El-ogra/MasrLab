using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordBreak;

public record RecordBreakCommand(int AttendanceLogId, string? BreakPeriod) : IRequest<Unit>;
