using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;

public class UpdateReportSettingsCommandHandler : IRequestHandler<UpdateReportSettingsCommand, Unit>
{
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReportSettingsCommandHandler(ISystemSettingRepository settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReportSettingsCommand request, CancellationToken cancellationToken)
    {
        var keys = new Dictionary<string, string>
        {
            { "Report_Margins", request.Margins },
            { "Report_PaperSize", request.PaperSize.ToString() },
            { "Report_HeaderImage", request.HeaderImage ?? string.Empty },
            { "Report_HeaderText", request.HeaderText ?? string.Empty },
            { "Report_FooterText", request.FooterText ?? string.Empty },
            { "Report_HeaderColor", request.HeaderColor ?? string.Empty },
            { "Report_FooterColor", request.FooterColor ?? string.Empty }
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
