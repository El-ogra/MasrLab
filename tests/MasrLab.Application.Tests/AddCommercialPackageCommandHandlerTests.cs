using MasrLab.Application.Features.CommercialPackages.Commands.AddCommercialPackage;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class AddCommercialPackageCommandHandlerTests
{
    private readonly Mock<IRepository<CommercialPackage>> _packageRepo;
    private readonly Mock<IRepository<Test>> _testRepo;
    private readonly Mock<IPriceListRepository> _priceListRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public AddCommercialPackageCommandHandlerTests()
    {
        _packageRepo = new Mock<IRepository<CommercialPackage>>();
        _testRepo = new Mock<IRepository<Test>>();
        _priceListRepo = new Mock<IPriceListRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private AddCommercialPackageCommandHandler CreateHandler()
        => new(
            _packageRepo.Object,
            _testRepo.Object,
            _priceListRepo.Object,
            _unitOfWork.Object);

    private void SetupTests(params int[] testIds)
    {
        var tests = testIds.Select(id => new Test { Id = id, IsDeleted = false }).ToList();
        _testRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tests);
    }

    private void SetupPriceLists(params int[] priceListIds)
    {
        var lists = priceListIds.Select(id => new PriceList { Id = id, Name = $"PL{id}", IsDefault = false }).ToList();
        _priceListRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);
    }

    [Fact]
    public async Task Handle_ValidInput_CreatesPackage()
    {
        SetupTests(1, 2);
        SetupPriceLists(10);
        _packageRepo
            .Setup(r => r.AddAsync(It.IsAny<CommercialPackage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = await CreateHandler().Handle(
            new AddCommercialPackageCommand("Test Package", "1,2", "10:200"),
            CancellationToken.None);

        Assert.Equal(0, id);
        _packageRepo.Verify(r => r.AddAsync(
            It.Is<CommercialPackage>(p =>
                p.Name == "Test Package" &&
                p.Items.Count == 2 &&
                p.Prices.Count == 1 &&
                p.Prices.First().PriceListId == 10 &&
                p.Prices.First().Price == 200m),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTests_Throws()
    {
        SetupTests();

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddCommercialPackageCommand("Pkg", "", "10:200"),
                CancellationToken.None));

        Assert.Contains("at least one test", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_DuplicateTests_Throws()
    {
        SetupTests(1, 2);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddCommercialPackageCommand("Pkg", "1,1", "10:200"),
                CancellationToken.None));

        Assert.Contains("Duplicate", ex.Message);
    }

    [Fact]
    public async Task Handle_MissingTest_Throws()
    {
        SetupTests(1);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddCommercialPackageCommand("Pkg", "1,999", "10:200"),
                CancellationToken.None));

        Assert.Contains("Tests not found", ex.Message);
    }

    [Fact]
    public async Task Handle_NoPrices_Throws()
    {
        SetupTests(1);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddCommercialPackageCommand("Pkg", "1", ""),
                CancellationToken.None));

        Assert.Contains("at least one price", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_InvalidPriceList_Throws()
    {
        SetupTests(1);
        SetupPriceLists(10);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddCommercialPackageCommand("Pkg", "1", "99:200"),
                CancellationToken.None));

        Assert.Contains("Price lists not found", ex.Message);
    }

    [Fact]
    public async Task Handle_MultiplePrices_AllIncluded()
    {
        SetupTests(1, 2);
        SetupPriceLists(10, 20);

        await CreateHandler().Handle(
            new AddCommercialPackageCommand("Pkg", "1,2", "10:100,20:200"),
            CancellationToken.None);

        _packageRepo.Verify(r => r.AddAsync(
            It.Is<CommercialPackage>(p =>
                p.Prices.Count == 2 &&
                p.Prices.Any(x => x.PriceListId == 10 && x.Price == 100m) &&
                p.Prices.Any(x => x.PriceListId == 20 && x.Price == 200m)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
