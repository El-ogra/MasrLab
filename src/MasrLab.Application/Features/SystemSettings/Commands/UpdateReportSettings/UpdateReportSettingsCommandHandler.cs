using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;

public class UpdateReportSettingsCommandHandler : IRequestHandler<UpdateReportSettingsCommand, Unit>
{
    private readonly IRepository<SystemSetting> _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReportSettingsCommandHandler(IRepository<SystemSetting> settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReportSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settingRepository.GetAllAsync();

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
