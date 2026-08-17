using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class TestResultEditHistory : BaseEntity
{
    public int TestResultId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? OldComment { get; set; }
    public string? NewComment { get; set; }
    public ResultEditChangeType ChangeType { get; set; }
    public int EditedByUserId { get; set; }
    public DateTime EditedAt { get; set; }
}
