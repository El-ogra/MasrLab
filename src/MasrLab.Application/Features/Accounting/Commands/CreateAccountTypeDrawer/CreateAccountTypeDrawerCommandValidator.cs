using FluentValidation;

namespace MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;

public class CreateAccountTypeDrawerCommandValidator : AbstractValidator<CreateAccountTypeDrawerCommand>
{
    public CreateAccountTypeDrawerCommandValidator()
    {
        RuleFor(x => x.PeriodEnd)
            .Must((command, periodEnd) => periodEnd > command.PeriodStart)
            .WithMessage("Period end must be greater than period start.");
    }
}
