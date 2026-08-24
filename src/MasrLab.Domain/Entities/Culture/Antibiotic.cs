using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Culture;

public class Antibiotic : BaseEntity
{
    /// <summary>
    /// The antibiotic symbol or abbreviation displayed in the culture configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
}
