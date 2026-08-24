using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Services;

public sealed class CultureTemplateSeeder : ICultureTemplateSeeder
{
    public const string TemplateSettingKey = "Culture.DefaultTemplateTestId";

    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IRepository<CultureAntibioticCommercialName> _commercialNameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CultureTemplateSeeder(
        ISystemSettingRepository systemSettingRepository,
        ICultureAntibioticRepository assignmentRepository,
        IRepository<CultureAntibioticCommercialName> commercialNameRepository,
        IUnitOfWork unitOfWork)
    {
        _systemSettingRepository = systemSettingRepository;
        _assignmentRepository = assignmentRepository;
        _commercialNameRepository = commercialNameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedFromTemplateAsync(int newCultureTestId, CancellationToken cancellationToken = default)
    {
        var settings = await _systemSettingRepository.GetByKeysAsync(
            new[] { TemplateSettingKey }, cancellationToken);
        var setting = settings.SingleOrDefault();
        if (setting is null || !int.TryParse(setting.SettingValue, out var templateTestId) || templateTestId <= 0)
            return;
        if (templateTestId == newCultureTestId)
            return;

        var templateAssignments = await _assignmentRepository.GetByCultureTestIdAsync(
            templateTestId, cancellationToken);
        if (templateAssignments.Count == 0)
            return;

        foreach (var template in templateAssignments.Where(item => !item.IsDeleted))
        {
            var clone = CultureAntibiotic.Create(
                newCultureTestId,
                template.AntibioticId,
                template.SensitivityText,
                template.Pregnant,
                template.Children);
            await _assignmentRepository.AddAsync(clone, cancellationToken);

            foreach (var name in template.CommercialNames.Where(item => !item.IsDeleted))
            {
                await _commercialNameRepository.AddAsync(new CultureAntibioticCommercialName
                {
                    CultureAntibiotic = clone,
                    Name = name.Name,
                    Print = name.Print
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
