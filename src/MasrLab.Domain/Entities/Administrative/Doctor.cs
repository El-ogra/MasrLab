using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class Doctor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal CommissionPercent { get; set; }
}
