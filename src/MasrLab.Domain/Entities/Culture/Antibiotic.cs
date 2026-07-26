using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Culture;

public class Antibiotic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
}
