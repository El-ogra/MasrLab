using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class TestResult : BaseEntity
{
    public int VisitTestId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public ResultStatus Status { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; }
    public int? PrintedByUserId { get; set; }
    public DateTime? PrintedAt { get; set; }
    public int PrintCount { get; set; }
}
