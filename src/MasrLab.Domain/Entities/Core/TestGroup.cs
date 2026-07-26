using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class TestGroup : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;
    public decimal GroupPrice { get; set; }
    public string TestIds { get; set; } = string.Empty;
}
