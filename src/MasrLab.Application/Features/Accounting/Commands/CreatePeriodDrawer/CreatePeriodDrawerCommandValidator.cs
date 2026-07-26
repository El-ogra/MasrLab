using FluentValidation;

namespace MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;

public class CreatePeriodDrawerCommandValidator : AbstractValidator<CreatePeriodDrawerCommand>
{
    public CreatePeriodDrawerCommandValidator()
    {
        RuleFor(x => x.PeriodEnd)
            .Must((command, periodEnd) => periodEnd > command.PeriodStart)
            .WithMessage("Period end must be greater than period start.");
    }
}
