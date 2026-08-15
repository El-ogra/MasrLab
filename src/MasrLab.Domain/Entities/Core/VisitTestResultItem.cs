using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class VisitTestResultItem : BaseEntity
{
    public int VisitTestId { get; set; }

    public int SourceTestComponentId { get; set; }

    public string ComponentName { get; set; } = string.Empty;
    public string ComponentUnit { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public ResultEntryKind ResultEntryKind { get; set; }
}
