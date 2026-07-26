using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordBreak;

public class RecordBreakCommandHandler : IRequestHandler<RecordBreakCommand, Unit>
{
    public Task<Unit> Handle(RecordBreakCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
