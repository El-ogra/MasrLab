using MasrLab.Domain.Common;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class Comment : BaseEntity
{
    public int TestId { get; set; }

    private string _commentText = string.Empty;
    public string CommentText
    {
        get => _commentText;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Comment text cannot be empty.");
            if (value.Length > 1000)
                throw new BusinessRuleViolationException("Comment text cannot exceed 1000 characters.");
            _commentText = value;
        }
    }

    public static Comment AttachToResult(int testId, string text)
    {
        if (testId <= 0)
            throw new BusinessRuleViolationException("Comment requires a valid TestId.");
        var comment = new Comment
        {
            TestId = testId,
            CommentText = text
        };
        comment.AddDomainEvent(new CommentAttachedToResult(comment.Id, testId));
        return comment;
    }
}
