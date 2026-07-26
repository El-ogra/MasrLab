using FluentValidation;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;

public class RecordLoginCommandValidator : AbstractValidator<RecordLoginCommand>
{
    public RecordLoginCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}
