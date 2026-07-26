using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, object>
{
    public Task<object> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
