using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandHandler : IRequestHandler<UpdateAccountSettingsCommand, Unit>
{
    public Task<Unit> Handle(UpdateAccountSettingsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
