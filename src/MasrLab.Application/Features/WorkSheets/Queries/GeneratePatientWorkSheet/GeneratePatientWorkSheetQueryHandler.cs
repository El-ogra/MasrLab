using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.WorkSheets.Queries.GeneratePatientWorkSheet;

public class GeneratePatientWorkSheetQueryHandler : IRequestHandler<GeneratePatientWorkSheetQuery, WorkSheetDto>
{
    public Task<WorkSheetDto> Handle(GeneratePatientWorkSheetQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
