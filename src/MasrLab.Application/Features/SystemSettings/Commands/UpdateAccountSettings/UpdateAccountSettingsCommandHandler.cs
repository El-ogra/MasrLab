using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandHandler : IRequestHandler<UpdateAccountSettingsCommand, Unit>
{
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountSettingsCommandHandler(ISystemSettingRepository settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateAccountSettingsCommand request, CancellationToken cancellationToken)
    {
        var keys = new Dictionary<string, string>
        {
            { "Account_LabName", request.LabName ?? string.Empty },
            { "Account_Currency", request.Currency ?? string.Empty }
        };

        var settings = await _settingRepository.GetByKeysAsync(keys.Keys.ToList(), cancellationToken);

        foreach (var kvp in keys)
        {
            var existing = settings.FirstOrDefault(s => s.SettingKey == kvp.Key);
            if (existing != null)
            {
                existing.SettingValue = kvp.Value;
                _settingRepository.Update(existing);
            }
            else
            {
                await _settingRepository.AddAsync(new SystemSetting
                {
                    SettingKey = kvp.Key,
                    SettingValue = kvp.Value
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
