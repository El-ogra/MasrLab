using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Culture;

public class Culture : BaseEntity
{
    public string SampleType { get; set; } = string.Empty;
    public string? OrganismA { get; set; }
    public string? OrganismB { get; set; }
    public string? OrganismC { get; set; }
    public string CultureCondition { get; set; } = string.Empty;
    public int ColonyCount { get; set; }
}
