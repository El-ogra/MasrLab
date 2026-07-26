using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestWorkSheet;

public class GenerateTestWorkSheetQueryHandler : IRequestHandler<GenerateTestWorkSheetQuery, WorkSheetDto>
{
    public Task<WorkSheetDto> Handle(GenerateTestWorkSheetQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
