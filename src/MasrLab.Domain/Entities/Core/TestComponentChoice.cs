using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class TestComponentChoice : BaseEntity
{
    public int TestComponentId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
