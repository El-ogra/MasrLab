using AutoMapper;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetTestsListCostPriceTests
{
    [Fact]
    public async Task Handle_Returns_Supplied_CostPrice()
    {
        var repo = new Mock<ITestRepository>();
        var test = new Test
        {
            Id = 1,
            Name = "CBC",
            ReportName = "CBC Report",
            ReceiptName = "CBC Receipt",
            Group = "Hematology",
            Price = 100m,
            TurnaroundTime = "24h",
            Unit = "mg/dL",
            CostPrice = 55.75m,
            TestComponents = new List<TestComponent>()
        };
        repo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var handler = new GetTestsListQueryHandler(repo.Object, new Mock<IReferralEntityRepository>().Object, new Mock<IMapper>().Object);
        var result = await handler.Handle(new GetTestsListQuery(null, null, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(55.75m, result[0].CostPrice);
    }

    [Fact]
    public async Task Handle_Returns_Null_CostPrice_When_Not_Set()
    {
        var repo = new Mock<ITestRepository>();
        var test = new Test
        {
            Id = 2,
            Name = "CRP",
            ReportName = "CRP Report",
            ReceiptName = "CRP Receipt",
            Group = "Chemistry",
            Price = 200m,
            TurnaroundTime = "48h",
            Unit = "mg/L",
            CostPrice = null,
            TestComponents = new List<TestComponent>()
        };
        repo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var handler = new GetTestsListQueryHandler(repo.Object, new Mock<IReferralEntityRepository>().Object, new Mock<IMapper>().Object);
        var result = await handler.Handle(new GetTestsListQuery(null, null, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Null(result[0].CostPrice);
    }
}
