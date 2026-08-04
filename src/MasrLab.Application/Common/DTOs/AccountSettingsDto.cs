using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record AccountSettingsDto
{
    public AccountType DefaultAccountType { get; init; }
    public bool AutoClosePeriod { get; init; }
    public bool AllowNegativeBalance { get; init; }
}
