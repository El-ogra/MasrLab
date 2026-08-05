using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;

public class UpdateReceiptSettingsCommandHandler : IRequestHandler<UpdateReceiptSettingsCommand, Unit>
{
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReceiptSettingsCommandHandler(ISystemSettingRepository settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReceiptSettingsCommand request, CancellationToken cancellationToken)
    {
        var keys = new Dictionary<string, string>
        {
            { "Receipt_HeaderText", request.HeaderText ?? string.Empty },
            { "Receipt_FooterText", request.FooterText ?? string.Empty }
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
