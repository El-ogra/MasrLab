using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class CommentTemplate : BaseEntity
{
    public int TestId { get; set; }
    public string Text { get; set; } = string.Empty;
}
