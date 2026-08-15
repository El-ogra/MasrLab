using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class TestComponent : BaseEntity
{
    public int TestId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public ResultEntryKind ResultEntryKind { get; set; }

    public static TestComponent Create(int testId, string name, string unit, int displayOrder, ResultEntryKind resultEntryKind = ResultEntryKind.Ordinary)
    {
        if (testId <= 0)
            throw new BusinessRuleViolationException("TestComponent requires a valid TestId.");
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("TestComponent name cannot be empty.");
        if (displayOrder <= 0)
            throw new BusinessRuleViolationException("TestComponent DisplayOrder must be positive.");

        return new TestComponent
        {
            TestId = testId,
            Name = name,
            Unit = unit,
            DisplayOrder = displayOrder,
            ResultEntryKind = resultEntryKind
        };
    }
}
