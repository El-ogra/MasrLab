using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Financial;

public class ExternalLab : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
}
