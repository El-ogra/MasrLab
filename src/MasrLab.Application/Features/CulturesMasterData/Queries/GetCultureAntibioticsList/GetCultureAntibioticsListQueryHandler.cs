using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibiotics;
using MasrLab.Domain.Common;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibioticsList;

public sealed class GetCultureAntibioticsListQueryHandler : IRequestHandler<GetCultureAntibioticsListQuery, IReadOnlyList<CultureAntibioticDto>>
{
    private readonly ICultureAntibioticRepository _assignmentRepository;

    public GetCultureAntibioticsListQueryHandler(ICultureAntibioticRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<IReadOnlyList<CultureAntibioticDto>> Handle(
        GetCultureAntibioticsListQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _assignmentRepository.GetByCultureTestIdAsync(
            request.CultureTestId, cancellationToken);

        return assignments
            .Where(item => !item.IsDeleted)
            .Select(GetCultureAntibioticsQueryHandler.ToDto)
            .ToList();
    }
}
