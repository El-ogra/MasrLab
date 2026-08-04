using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record SensitivityDto
{
    public int Id { get; init; }
    public int CultureId { get; init; }
    public int AntibioticId { get; init; }
    public SensitivityLevel SensitivityLevel { get; init; }
}
