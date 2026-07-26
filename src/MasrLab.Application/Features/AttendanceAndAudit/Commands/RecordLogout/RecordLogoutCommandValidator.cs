using FluentValidation;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogout;

public class RecordLogoutCommandValidator : AbstractValidator<RecordLogoutCommand>
{
    public RecordLogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.AttendanceLogId)
            .GreaterThan(0);
    }
}
