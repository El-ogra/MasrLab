using MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class AddTestToVisitCommandHandlerTests
{
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IPriceListRepository> _priceListRepository;
    private readonly Mock<IPriceListResolverService> _priceListResolverService;
    private readonly Mock<IRepository<Sample>> _sampleRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public AddTestToVisitCommandHandlerTests()
    {
        _visitRepository = new Mock<IVisitRepository>();
        _priceListRepository = new Mock<IPriceListRepository>();
        _priceListResolverService = new Mock<IPriceListResolverService>();
        _sampleRepository = new Mock<IRepository<Sample>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private AddTestToVisitCommandHandler CreateHandler()
        => new(
            _visitRepository.Object,
            _priceListRepository.Object,
            _priceListResolverService.Object,
            _sampleRepository.Object,
            _unitOfWork.Object);

    private void SetupVisit(int visitId = 1, VisitStatus status = VisitStatus.Registered)
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        // Simulate EF setting the Id
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, visitId);

        if (status == VisitStatus.Closed)
            visit.Close(0m);

        _visitRepository
            .Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);
    }

    private void SetupDefaultPriceList(int priceListId = 10)
    {
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = priceListId, Name = "Default", IsDefault = true });
    }

    private void SetupPrice(int testId, int priceListId, decimal price)
    {
        _priceListResolverService
            .Setup(s => s.ResolvePriceAsync(testId, priceListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);
    }

    [Fact]
    public async Task Handle_WithDefaultPriceList_AddsTestAndCreatesSample()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, false),
            CancellationToken.None);

        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExplicitPriceList_AddsTestAndCreatesSample()
    {
        SetupVisit();
        SetupPrice(testId: 5, priceListId: 20, price: 200m);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, 20, false),
            CancellationToken.None);

        _priceListResolverService.Verify(s =>
            s.ResolvePriceAsync(5, 20, It.IsAny<CancellationToken>()), Times.Once);
        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoDefaultAndNoExplicit_ThrowsBusinessRuleViolationException()
    {
        SetupVisit();
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));

        Assert.Contains("No default price list configured", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPriceMissingForTest_ThrowsBusinessRuleViolationException()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        _priceListResolverService
            .Setup(s => s.ResolvePriceAsync(5, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleViolationException("No price item found for test 5 in price list 10."));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenVisitIsClosed_ThrowsBusinessRuleViolationException()
    {
        SetupVisit(status: VisitStatus.Closed);
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenVisitNotFound_ThrowsEntityNotFoundException()
    {
        _visitRepository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientVisit?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(99, new[] { 5 }, 10, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleTests_AddsAllTestsAndCreatesSamples()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupPrice(testId: 6, priceListId: 10, price: 200m);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5, 6 }, null, false),
            CancellationToken.None);

        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 6),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMarkOutsourced_SetsIsOutsourcedOnVisitTest()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);

        PatientVisit? capturedVisit = null;
        _visitRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                capturedVisit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
                typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
                    .SetValue(capturedVisit, 1);
            })
            .ReturnsAsync(() => capturedVisit!);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, true),
            CancellationToken.None);

        Assert.Contains(capturedVisit!.VisitTests, vt => vt.TestId == 5 && vt.IsOutsourced);
    }
}
