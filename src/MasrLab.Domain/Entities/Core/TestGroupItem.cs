using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class TestGroupItem : BaseEntity
{
    public int TestGroupId { get; set; }
    public int TestId { get; set; }
}
