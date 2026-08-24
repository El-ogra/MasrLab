using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibiotics;

public sealed class GetCultureAntibioticsQueryHandler
    : IRequestHandler<GetCultureAntibioticsQuery, IReadOnlyList<CultureAntibioticDto>>
{
    private readonly ICultureAntibioticRepository _assignmentRepository;

    public GetCultureAntibioticsQueryHandler(ICultureAntibioticRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<IReadOnlyList<CultureAntibioticDto>> Handle(
        GetCultureAntibioticsQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _assignmentRepository.GetByCultureTestIdAsync(
            request.CultureTestId, cancellationToken);

        return assignments
            .Where(item => !item.IsDeleted)
            .Where(item => CultureAntibioticVisibility.IsVisible(
                item.Pregnant,
                item.Children,
                request.PatientIsPregnant,
                request.PatientAgeYears))
            .Select(ToDto)
            .ToList();
    }

    internal static CultureAntibioticDto ToDto(Domain.Entities.Culture.CultureAntibiotic item)
    {
        return new CultureAntibioticDto
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
        };
    }
}
