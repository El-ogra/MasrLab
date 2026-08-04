using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;

public class UpdatePrinterSettingsCommandHandler : IRequestHandler<UpdatePrinterSettingsCommand, Unit>
{
    private readonly IRepository<SystemSetting> _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePrinterSettingsCommandHandler(IRepository<SystemSetting> settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePrinterSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settingRepository.GetAllAsync();

        var keys = new Dictionary<string, string>
        {
            { "Printer_Name", request.PrinterName ?? string.Empty },
            { "Printer_PurposeType", request.PurposeType.ToString() }
        };

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
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
