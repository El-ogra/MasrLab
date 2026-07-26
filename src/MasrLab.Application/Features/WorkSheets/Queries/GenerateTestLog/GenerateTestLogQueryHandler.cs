using MediatR;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;

public class GenerateTestLogQueryHandler : IRequestHandler<GenerateTestLogQuery, IReadOnlyList<object>>
{
    public Task<IReadOnlyList<object>> Handle(GenerateTestLogQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
