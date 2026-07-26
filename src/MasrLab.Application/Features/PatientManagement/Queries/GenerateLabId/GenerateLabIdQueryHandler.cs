using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;

public class GenerateLabIdQueryHandler : IRequestHandler<GenerateLabIdQuery, string>
{
    public Task<string> Handle(GenerateLabIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
