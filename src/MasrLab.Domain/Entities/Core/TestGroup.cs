using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class TestGroup : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;
    public decimal GroupPrice { get; set; }

    public ICollection<TestGroupItem> TestGroupItems { get; set; } = new List<TestGroupItem>();
}
