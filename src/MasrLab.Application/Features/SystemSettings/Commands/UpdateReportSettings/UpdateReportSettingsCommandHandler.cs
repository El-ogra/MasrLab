using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;

public class UpdateReportSettingsCommandHandler : IRequestHandler<UpdateReportSettingsCommand, Unit>
{
    public Task<Unit> Handle(UpdateReportSettingsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
