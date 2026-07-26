using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;

public class UpdateEnvelopeBarcodeSettingsCommandHandler : IRequestHandler<UpdateEnvelopeBarcodeSettingsCommand, Unit>
{
    public Task<Unit> Handle(UpdateEnvelopeBarcodeSettingsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
