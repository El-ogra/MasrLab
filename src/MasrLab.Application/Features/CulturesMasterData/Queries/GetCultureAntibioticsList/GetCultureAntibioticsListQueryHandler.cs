using MasrLab.Application.Common.DTOs;
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
            .Select(item => new CultureAntibioticDto
            {
                Id = item.Id,
                CultureTestId = item.CultureTestId,
                AntibioticId = item.AntibioticId,
                Symbol = item.Antibiotic?.Name ?? string.Empty,
                ScientificName = item.Antibiotic?.ScientificName ?? string.Empty,
                SensitivityText = item.SensitivityText,
                Pregnant = item.Pregnant,
                Children = item.Children,
                CommercialNames = item.CommercialNames
                    .Where(name => !name.IsDeleted)
                    .Select(name => new CultureAntibioticCommercialNameDto
                    {
                        Id = name.Id,
                        Name = name.Name,
                        Print = name.Print
                    })
                    .ToList()
            })
            .ToList();
    }
}
