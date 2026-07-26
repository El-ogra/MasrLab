using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;

public class RecordLoginCommandHandler : IRequestHandler<RecordLoginCommand, Unit>
{
    public Task<Unit> Handle(RecordLoginCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
