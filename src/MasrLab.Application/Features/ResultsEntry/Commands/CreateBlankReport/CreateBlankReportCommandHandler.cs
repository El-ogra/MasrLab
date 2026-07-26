using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateBlankReport;

public class CreateBlankReportCommandHandler : IRequestHandler<CreateBlankReportCommand, Unit>
{
    public Task<Unit> Handle(CreateBlankReportCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
