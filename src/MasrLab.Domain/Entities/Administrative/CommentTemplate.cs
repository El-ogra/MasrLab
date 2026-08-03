using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Administrative;

public class CommentTemplate : BaseEntity
{
    public int TestId { get; set; }

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Comment template text cannot be empty.");
            if (value.Length > 1000)
                throw new BusinessRuleViolationException("Comment template text cannot exceed 1000 characters.");
            _text = value;
        }
    }
}
