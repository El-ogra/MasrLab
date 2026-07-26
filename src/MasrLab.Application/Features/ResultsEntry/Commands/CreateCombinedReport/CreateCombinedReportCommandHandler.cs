using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

public class CreateCombinedReportCommandHandler : IRequestHandler<CreateCombinedReportCommand, Unit>
{
    public Task<Unit> Handle(CreateCombinedReportCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
