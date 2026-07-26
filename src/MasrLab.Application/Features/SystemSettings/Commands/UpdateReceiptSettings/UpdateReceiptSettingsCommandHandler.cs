using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;

public class UpdateReceiptSettingsCommandHandler : IRequestHandler<UpdateReceiptSettingsCommand, Unit>
{
    public Task<Unit> Handle(UpdateReceiptSettingsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
