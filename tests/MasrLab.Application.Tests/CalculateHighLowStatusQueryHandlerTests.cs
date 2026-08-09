using MasrLab.Application.Features.ResultsEntry.Queries.CalculateHighLowStatus;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class CalculateHighLowStatusQueryHandlerTests
{
    private readonly CalculateHighLowStatusQueryHandler _handler = new();

    [Fact]
    public async Task Handle_returns_high_when_value_exceeds_the_upper_reference_limit()
    {
        var result = await _handler.Handle(new CalculateHighLowStatusQuery("12.1", "4.0 - 10.0", Gender.Male, 30, 0, AgeUnit.Years), CancellationToken.None);

        Assert.Equal(ResultStatus.High, result);
    }

    [Fact]
    public async Task Handle_returns_normal_for_an_unparseable_reference_range()
    {
        var result = await _handler.Handle(new CalculateHighLowStatusQuery("12.1", "not-a-range", Gender.Male, 30, 0, AgeUnit.Years), CancellationToken.None);

        Assert.Equal(ResultStatus.Normal, result);
    }
}
