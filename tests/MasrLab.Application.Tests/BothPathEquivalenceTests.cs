using MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class BothPathEquivalenceTests
{
    private readonly VisitTestSnapshotter _realSnapshotter = new();

    [Fact]
    public async Task BothHandlers_ProduceEquivalentSnapshots_ForSameTest()
    {
        var testId = 42;
        var visitId = 1;
        var price = 150m;

        var test = new Test
        {
            Id = testId,
            Name = "CBC Panel",
            ReportName = "CBC Panel Report",
            ReceiptName = "CBC Panel Receipt",
            Group = "CBC",
            Price = price
        };
        test.TestComponents.Add(new TestComponent
        {
            Id = 421, TestId = testId, Name = "WBC", Unit = "10^3/uL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 422, TestId = testId, Name = "RBC", Unit = "10^6/uL",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.Ordinary
        });

        // --- Legacy path (AddTestToVisitCommandHandler) ---
        var legacyVisit = PatientVisit.Create(1, 1, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(legacyVisit, visitId);

        var legacyVisitRepo = new Mock<IVisitRepository>();
        legacyVisitRepo.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(legacyVisit);

        var legacyTestRepo = new Mock<ITestRepository>();
        legacyTestRepo.Setup(r => r.GetByIdWithComponentsAsync(testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(test);

        var priceListRepo = new Mock<IPriceListRepository>();
        priceListRepo.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 10, Name = "Default", IsDefault = true });

        var priceResolver = new Mock<IPriceListResolverService>();
        priceResolver.Setup(s => s.ResolvePriceAsync(testId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);

        var sampleRepo = new Mock<IRepository<Sample>>();
        var legacyUow = new Mock<IUnitOfWork>();

        var legacyHandler = new AddTestToVisitCommandHandler(
            legacyVisitRepo.Object, priceListRepo.Object, priceResolver.Object,
            legacyTestRepo.Object, _realSnapshotter, sampleRepo.Object, legacyUow.Object);

        await legacyHandler.Handle(
            new AddTestToVisitCommand(visitId, new[] { testId }, null, false),
            CancellationToken.None);

        // --- Composer path (AddTestsToVisitCommandHandler) ---
        var composerVisit = PatientVisit.Create(1, 1, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(composerVisit, visitId);

        var composerVisitRepo = new Mock<IVisitRepository>();
        composerVisitRepo.Setup(r => r.GetByIdWithTestsAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(composerVisit);

        var composerTestRepo = new Mock<ITestRepository>();
        composerTestRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var composerPriceListRepo = new Mock<IPriceListRepository>();
        composerPriceListRepo.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 10, Name = "Default", IsDefault = true });

        var composerPriceResolver = new Mock<IPriceListResolverService>();
        composerPriceResolver.Setup(s => s.ResolvePriceAsync(testId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);

        var composerUow = new Mock<IUnitOfWork>();

        var composerHandler = new AddTestsToVisitCommandHandler(
            composerVisitRepo.Object, composerTestRepo.Object,
            new Mock<IRepository<TestGroup>>().Object,
            new Mock<ITestGroupItemRepository>().Object,
            new Mock<ICommercialPackageRepository>().Object,
            new Mock<IRepository<VisitCommercialPackage>>().Object,
            composerPriceResolver.Object, composerPriceListRepo.Object,
            _realSnapshotter, composerUow.Object);

        await composerHandler.Handle(
            new AddTestsToVisitCommand(visitId, "Direct", null, null, testId.ToString(), false),
            CancellationToken.None);

        // --- Assert equivalence ---
        var legacyVt = Assert.Single(legacyVisit.VisitTests);
        var composerVt = Assert.Single(composerVisit.VisitTests);

        Assert.Equal(legacyVt.TestNameSnapshot, composerVt.TestNameSnapshot);
        Assert.Equal(legacyVt.ReportNameSnapshot, composerVt.ReportNameSnapshot);
        Assert.Equal(legacyVt.ReceiptNameSnapshot, composerVt.ReceiptNameSnapshot);
        Assert.Equal(legacyVt.IsCompoundSnapshot, composerVt.IsCompoundSnapshot);
        Assert.Equal(legacyVt.Price, composerVt.Price);
        Assert.Equal(legacyVt.IsOutsourced, composerVt.IsOutsourced);
        Assert.Equal(legacyVt.TestId, composerVt.TestId);

        Assert.Equal(legacyVt.ResultItems.Count, composerVt.ResultItems.Count);

        var legacyItems = legacyVt.ResultItems.OrderBy(r => r.DisplayOrder).ToList();
        var composerItems = composerVt.ResultItems.OrderBy(r => r.DisplayOrder).ToList();

        for (int i = 0; i < legacyItems.Count; i++)
        {
            Assert.Equal(legacyItems[i].SourceTestComponentId, composerItems[i].SourceTestComponentId);
            Assert.Equal(legacyItems[i].ComponentName, composerItems[i].ComponentName);
            Assert.Equal(legacyItems[i].ComponentUnit, composerItems[i].ComponentUnit);
            Assert.Equal(legacyItems[i].DisplayOrder, composerItems[i].DisplayOrder);
            Assert.Equal(legacyItems[i].ResultEntryKind, composerItems[i].ResultEntryKind);
        }
    }
}
