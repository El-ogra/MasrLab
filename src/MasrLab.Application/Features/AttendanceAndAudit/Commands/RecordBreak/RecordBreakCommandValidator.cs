using FluentValidation;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordBreak;

public class RecordBreakCommandValidator : AbstractValidator<RecordBreakCommand>
{
    public RecordBreakCommandValidator()
    {
        RuleFor(x => x.AttendanceLogId)
            .GreaterThan(0);
    }
}
