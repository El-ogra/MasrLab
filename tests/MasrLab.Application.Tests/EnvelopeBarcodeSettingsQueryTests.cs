using MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class EnvelopeBarcodeSettingsQueryTests
{
    [Fact]
    public async Task Handle_MapsEnvelopeBarcodeSettings_AndUsesDefaultsForInvalidValues()
    {
        var repository = new Mock<ISystemSettingRepository>();
        repository.Setup(x => x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SystemSetting>
            {
                new() { SettingKey = "Envelope_UseBarcode", SettingValue = "true" },
                new() { SettingKey = "Envelope_BarcodeWidth", SettingValue = "601" },
                new() { SettingKey = "Envelope_BarcodeHeight", SettingValue = "120" }
            });

        var result = await new GetSystemSettingsQueryHandler(repository.Object).Handle(new GetSystemSettingsQuery(), default);

        Assert.True(result.EnvelopeBarcodeSettings.UseBarcode);
        Assert.Equal(300, result.EnvelopeBarcodeSettings.BarcodeWidth);
        Assert.Equal(120, result.EnvelopeBarcodeSettings.BarcodeHeight);
    }
}
