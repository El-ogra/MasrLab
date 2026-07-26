using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogout;

public class RecordLogoutCommandHandler : IRequestHandler<RecordLogoutCommand, Unit>
{
    public Task<Unit> Handle(RecordLogoutCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
