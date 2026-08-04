using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;

public class GenerateLabIdQueryHandler : IRequestHandler<GenerateLabIdQuery, string>
{
    private readonly LabIdGenerator _labIdGenerator;

    public GenerateLabIdQueryHandler(LabIdGenerator labIdGenerator)
    {
        _labIdGenerator = labIdGenerator;
    }

    public async Task<string> Handle(GenerateLabIdQuery request, CancellationToken cancellationToken)
    {
        return await _labIdGenerator.GenerateAsync(cancellationToken);
    }
}
