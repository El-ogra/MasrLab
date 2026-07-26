using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, IReadOnlyList<PatientDto>>
{
    public Task<IReadOnlyList<PatientDto>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
