using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public sealed class CultureTemplateSeederTests
{
    [Fact]
    public async Task Seeds_active_assignments_and_commercial_names_from_designated_template()
    {
        var settings = new Mock<ISystemSettingRepository>();
        settings.Setup(x => x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new SystemSetting { SettingKey = CultureTemplateSeeder.TemplateSettingKey, SettingValue = "99" } });
        var templateAssignment = CultureAntibiotic.Create(99, 7, "S", true, false);
        templateAssignment.Id = 40;
        templateAssignment.Antibiotic = new Antibiotic { Id = 7, Name = "AMX", ScientificName = "Amoxicillin" };
        templateAssignment.CommercialNames.Add(new CultureAntibioticCommercialName { Id = 1, Name = "Amoxil", Print = true });
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.GetByCultureTestIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { templateAssignment });
        CultureAntibiotic? clone = null;
        assignments.Setup(x => x.AddAsync(It.IsAny<CultureAntibiotic>(), It.IsAny<CancellationToken>()))
            .Callback<CultureAntibiotic, CancellationToken>((entity, _) => { entity.Id = 50; clone = entity; });
        var names = new Mock<IRepository<CultureAntibioticCommercialName>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        await new CultureTemplateSeeder(settings.Object, assignments.Object, names.Object, unitOfWork.Object)
            .SeedFromTemplateAsync(12);

        Assert.NotNull(clone);
        Assert.Equal(12, clone!.CultureTestId);
        Assert.Equal(7, clone.AntibioticId);
        Assert.Equal("S", clone.SensitivityText);
        names.Verify(x => x.AddAsync(It.Is<CultureAntibioticCommercialName>(name => name.Name == "Amoxil" && name.Print && name.CultureAntibiotic == clone), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Does_nothing_when_setting_is_absent_or_template_has_no_assignments()
    {
        var settings = new Mock<ISystemSettingRepository>();
        settings.Setup(x => x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SystemSetting>());
        var assignments = new Mock<ICultureAntibioticRepository>();
        var names = new Mock<IRepository<CultureAntibioticCommercialName>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var seeder = new CultureTemplateSeeder(settings.Object, assignments.Object, names.Object, unitOfWork.Object);

        await seeder.SeedFromTemplateAsync(12);
        assignments.Verify(x => x.GetByCultureTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        settings.Setup(x => x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new SystemSetting { SettingKey = CultureTemplateSeeder.TemplateSettingKey, SettingValue = "99" } });
        assignments.Setup(x => x.GetByCultureTestIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CultureAntibiotic>());
        await seeder.SeedFromTemplateAsync(12);
        assignments.Verify(x => x.AddAsync(It.IsAny<CultureAntibiotic>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
