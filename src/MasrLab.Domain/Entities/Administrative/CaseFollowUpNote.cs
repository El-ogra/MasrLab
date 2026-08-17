using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Administrative;

public class CaseFollowUpNote : BaseEntity
{
    public int TestId { get; set; }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Case follow-up notes cannot be empty.");
            if (value.Length > 2000)
                throw new BusinessRuleViolationException("Case follow-up notes cannot exceed 2000 characters.");
            _notes = value;
        }
    }
}
