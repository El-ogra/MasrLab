using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;

public class UpdatePrinterSettingsCommandHandler : IRequestHandler<UpdatePrinterSettingsCommand, Unit>
{
    public Task<Unit> Handle(UpdatePrinterSettingsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
