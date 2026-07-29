using FluentValidation;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public class SettleOutsourcedAccountCommandValidator : AbstractValidator<SettleOutsourcedAccountCommand>
{
    public SettleOutsourcedAccountCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SettlementStatus).IsInEnum();
    }
}
