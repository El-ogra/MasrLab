using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.SettleVisitAccount;

public class SettleVisitAccountCommandValidator : AbstractValidator<SettleVisitAccountCommand>
{
    public SettleVisitAccountCommandValidator()
    {
        RuleFor(x => x.PatientVisitId).GreaterThan(0)
            .WithMessage("PatientVisitId must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
