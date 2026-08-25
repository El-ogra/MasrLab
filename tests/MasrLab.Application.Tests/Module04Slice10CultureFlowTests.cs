using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;
using MasrLab.Application.Features.Cultures.Commands.RecordSensitivity;
using MasrLab.Application.Features.Cultures.Queries.GetMicrobiologyReport;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 10 — handler flows for sample type, microscopic rows, per-organism sensitivity.
public class Module04Slice10CultureFlowTests
{
    private static Culture PendingCultureWithOrganisms()
    {
        var culture = Culture.Create(50);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(culture, 9);
        culture.OrganismA = "E.coli";
        culture.OrganismB = "Klebsiella";
        return culture;
    }

    private static Culture RecordedCulture()
    {
        var culture = PendingCultureWithOrganisms();
        culture.Record(100000, culture.OrganismA, culture.OrganismB, null);
        return culture;
    }

    [Fact]
    public async Task EnterCultureResult_persists_sample_type_microscopic_rows_and_derived_bacteria()
    {
        var culture = PendingCultureWithOrganisms();
        var repository = new Mock<ICultureRepository>();
        repository.Setup(x => x.GetWithSensitivitiesAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(culture);
        var uow = new Mock<IUnitOfWork>();

        await new EnterCultureResultCommandHandler(repository.Object, uow.Object).Handle(
            new EnterCultureResultCommand(9, "E.coli", "Klebsiella", null, "Aerobic", 100000,
                SampleType: "Urine",
                MicroscopicFindings: new[] { new MicroscopicFindingInput(MicroscopicFindingRow.PusCells, "8/HPF") }),
            default);

        Assert.Equal("Urine", culture.SampleType); // real gap closed.
        Assert.Equal("8/HPF", culture.MicroscopicFindings.Single(f => f.RowKey == MicroscopicFindingRow.PusCells).Value);
        var bacteria = culture.MicroscopicFindings.Single(f => f.RowKey == MicroscopicFindingRow.Bacteria);
        Assert.Equal("E.coli, Klebsiella", bacteria.Value); // OQ-M4-11 derived row.
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordSensitivityCommand_records_slot_row_and_zone_override()
    {
        var culture = RecordedCulture();
        var repository = new Mock<ICultureRepository>();
        repository.Setup(x => x.GetWithSensitivitiesAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(culture);
        var uow = new Mock<IUnitOfWork>();
        var handler = new RecordSensitivityCommandHandler(repository.Object, uow.Object);

        await handler.Handle(new RecordSensitivityCommand(9, OrganismSlot.A, 7, SensitivityLevel.HighlySensitive, InhibitionZoneOverride: "24 mm"), default);
        await handler.Handle(new RecordSensitivityCommand(9, OrganismSlot.A, 8, SensitivityLevel.Resistant), default);

        Assert.Equal(2, culture.Sensitivities.Count);
        Assert.All(culture.Sensitivities, s => Assert.Equal(OrganismSlot.A, s.OrganismSlot));
        Assert.Equal("24 mm", culture.Sensitivities.ElementAt(0).InhibitionZoneOverride);
        Assert.Null(culture.Sensitivities.ElementAt(1).InhibitionZoneOverride); // falls back to M13 text at read time.
    }

    [Fact]
    public async Task RecordSensitivityCommand_rejects_unrecorded_organism_slots()
    {
        var culture = Culture.Create(50);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(culture, 9);
        culture.Record(100000, "E.coli", null, null); // only organism A recorded.
        var repository = new Mock<ICultureRepository>();
        repository.Setup(x => x.GetWithSensitivitiesAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(culture);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            new RecordSensitivityCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new RecordSensitivityCommand(9, OrganismSlot.B, 7, SensitivityLevel.Low), default));
    }

    [Fact]
    public async Task RecordSensitivityCommand_throws_when_culture_missing()
    {
        var repository = new Mock<ICultureRepository>();
        repository.Setup(x => x.GetWithSensitivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Culture?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new RecordSensitivityCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new RecordSensitivityCommand(99, OrganismSlot.A, 7, SensitivityLevel.Low), default));
    }

    [Fact]
    public async Task GetMicrobiologyReport_propagates_missing_cultures_as_key_not_found()
    {
        var handler = new GetMicrobiologyReportQueryHandler(new NullReader());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetMicrobiologyReportQuery(42), default));
    }

    private sealed class NullReader : IMicrobiologyReportReader
    {
        public Task<MicrobiologyReportDto?> GetAsync(int cultureId, CancellationToken cancellationToken = default)
            => Task.FromResult<MicrobiologyReportDto?>(null);
    }
}
