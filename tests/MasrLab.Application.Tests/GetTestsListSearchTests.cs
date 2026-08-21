using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetTestsListSearchTests
{
    [Fact]
    public async Task Handle_SearchText_Numeric_Returns_Test_By_Id()
    {
        var handler = CreateHandler(
            CreateTest(7, "CBC", "Hematology"),
            CreateTest(8, "CRP", "Chemistry"));

        var result = await handler.Handle(
            new GetTestsListQuery(null, null, null, " 7 "),
            CancellationToken.None);

        var test = Assert.Single(result);
        Assert.Equal(7, test.Id);
    }

    [Fact]
    public async Task Handle_SearchText_Text_Returns_Tests_By_Name_Or_Group()
    {
        var handler = CreateHandler(
            CreateTest(1, "CBC", "Hematology"),
            CreateTest(2, "CRP", "Chemistry"),
            CreateTest(3, "Glucose", "Immunology"));

        var nameResult = await handler.Handle(
            new GetTestsListQuery(null, null, null, "cbc"),
            CancellationToken.None);
        var groupResult = await handler.Handle(
            new GetTestsListQuery(null, null, null, "chemistry"),
            CancellationToken.None);

        Assert.Equal(1, Assert.Single(nameResult).Id);
        Assert.Equal(2, Assert.Single(groupResult).Id);
    }

    [Fact]
    public async Task Handle_Empty_SearchText_Returns_Full_List()
    {
        var handler = CreateHandler(
            CreateTest(1, "CBC", "Hematology"),
            CreateTest(2, "CRP", "Chemistry"));

        var result = await handler.Handle(
            new GetTestsListQuery(null, null, null, "   "),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_SearchText_Composes_With_Existing_Filters()
    {
        var handler = CreateHandler(
            CreateTest(1, "CBC", "Hematology"),
            CreateTest(2, "CBC Panel", "Chemistry"),
            CreateTest(3, "CRP", "Chemistry"));

        var result = await handler.Handle(
            new GetTestsListQuery("CBC", "Chemistry", null, "CBC"),
            CancellationToken.None);

        var test = Assert.Single(result);
        Assert.Equal(2, test.Id);
    }

    private static GetTestsListQueryHandler CreateHandler(params Test[] tests)
    {
        var repository = new Mock<ITestRepository>();
        repository
            .Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tests.ToList());

        return new GetTestsListQueryHandler(repository.Object, new Mock<IReferralEntityRepository>().Object, new Mock<AutoMapper.IMapper>().Object);
    }

    private static Test CreateTest(int id, string name, string group) => new()
    {
        Id = id,
        Name = name,
        ReportName = name,
        ReceiptName = name,
        Group = group,
        Price = 100m,
        TurnaroundTime = "24h",
        Unit = "mg/dL",
        TestComponents = new List<TestComponent>()
    };
}
