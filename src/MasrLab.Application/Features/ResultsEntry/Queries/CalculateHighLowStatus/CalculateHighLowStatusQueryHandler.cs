using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.CalculateHighLowStatus;

public class CalculateHighLowStatusQueryHandler : IRequestHandler<CalculateHighLowStatusQuery, ResultStatus>
{
    public Task<ResultStatus> Handle(CalculateHighLowStatusQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value) || string.IsNullOrWhiteSpace(request.ReferenceRange))
            return Task.FromResult(ResultStatus.Normal);

        var parts = request.ReferenceRange.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return Task.FromResult(ResultStatus.Normal);

        if (!decimal.TryParse(parts[0], out var lowRef) || !decimal.TryParse(parts[1], out var highRef))
            return Task.FromResult(ResultStatus.Normal);

        if (!decimal.TryParse(request.Value, out var numericValue))
            return Task.FromResult(ResultStatus.Normal);

        if (numericValue > highRef)
            return Task.FromResult(ResultStatus.High);

        if (numericValue < lowRef)
            return Task.FromResult(ResultStatus.Low);

        return Task.FromResult(ResultStatus.Normal);
    }
}
