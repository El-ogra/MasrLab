using FluentAssertions;
using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureTestsList;
using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetCultureTestsListQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_only_active_culture_tests()
    {
        var repository = new Mock<ITestRepository>();
        var referralEntityRepository = new Mock<IReferralEntityRepository>();
        repository
            .Setup(item => item.GetByGroupAsync(CultureGroup.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test>
            {
                new() { Id = 1, Name = "Urine Culture", Group = CultureGroup.Name, ArrangeNo = 2 },
                new() { Id = 2, Name = "Deleted Culture", Group = CultureGroup.Name, IsDeleted = true, ArrangeNo = 1 },
                new() { Id = 3, Name = "CBC", Group = "Hematology", ArrangeNo = 0 }
            });

        var handler = new GetCultureTestsListQueryHandler(
            repository.Object,
            referralEntityRepository.Object);

        var result = await handler.Handle(new GetCultureTestsListQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
        result[0].Group.Should().Be(CultureGroup.Name);
        repository.Verify(
            item => item.GetByGroupAsync(CultureGroup.Name, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_orders_culture_tests_by_arrange_number()
    {
        var repository = new Mock<ITestRepository>();
        var referralEntityRepository = new Mock<IReferralEntityRepository>();
        repository
            .Setup(item => item.GetByGroupAsync(CultureGroup.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test>
            {
                new() { Id = 2, Name = "Second", Group = CultureGroup.Name, ArrangeNo = 20 },
                new() { Id = 1, Name = "First", Group = CultureGroup.Name, ArrangeNo = 10 }
            });

        var handler = new GetCultureTestsListQueryHandler(
            repository.Object,
            referralEntityRepository.Object);

        var result = await handler.Handle(new GetCultureTestsListQuery(), CancellationToken.None);

        result.Select(item => item.Id).Should().Equal(1, 2);
    }
}
