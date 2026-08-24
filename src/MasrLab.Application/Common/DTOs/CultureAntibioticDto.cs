namespace MasrLab.Application.Common.DTOs;

public record CultureAntibioticDto
{
    public int Id { get; init; }
    public int CultureTestId { get; init; }
    public int AntibioticId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string ScientificName { get; init; } = string.Empty;
    public string? SensitivityText { get; init; }
    public bool Pregnant { get; init; }
    public bool Children { get; init; }
    public IReadOnlyList<CultureAntibioticCommercialNameDto> CommercialNames { get; init; } = Array.Empty<CultureAntibioticCommercialNameDto>();
}

public record CultureAntibioticCommercialNameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Print { get; init; }
}
