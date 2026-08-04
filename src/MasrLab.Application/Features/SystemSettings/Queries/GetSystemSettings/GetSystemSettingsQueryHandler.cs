using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, SystemSettingsDto>
{
    public Task<SystemSettingsDto> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
