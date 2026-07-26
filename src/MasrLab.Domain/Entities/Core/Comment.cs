using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class Comment : BaseEntity
{
    public int TestId { get; set; }
    public string CommentText { get; set; } = string.Empty;
}
