using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Queries.CalculateHighLowStatus;

public class CalculateHighLowStatusQueryHandler : IRequestHandler<CalculateHighLowStatusQuery, ResultStatus>
{
    public Task<ResultStatus> Handle(CalculateHighLowStatusQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
