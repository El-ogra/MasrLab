using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientManagement.Queries.GetPatientById;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto?>
{
    public Task<PatientDto?> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
