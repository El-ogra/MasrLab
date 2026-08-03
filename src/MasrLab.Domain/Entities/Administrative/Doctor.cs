using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Administrative;

public class Doctor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public EgyptianPhone? Phone { get; set; }
    public string? Address { get; set; }

    private decimal _commissionPercent;
    public decimal CommissionPercent
    {
        get => _commissionPercent;
        set
        {
            if (value < 0 || value > 100)
                throw new BusinessRuleViolationException("CommissionPercent must be between 0 and 100.");
            _commissionPercent = value;
        }
    }
}
