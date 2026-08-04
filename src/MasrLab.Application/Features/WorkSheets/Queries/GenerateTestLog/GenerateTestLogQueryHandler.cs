using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;

public class GenerateTestLogQueryHandler : IRequestHandler<GenerateTestLogQuery, IReadOnlyList<TestLogEntryDto>>
{
    public Task<IReadOnlyList<TestLogEntryDto>> Handle(GenerateTestLogQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
