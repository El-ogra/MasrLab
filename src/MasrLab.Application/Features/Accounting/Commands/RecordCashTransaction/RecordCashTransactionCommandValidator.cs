using FluentValidation;

namespace MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;

public class RecordCashTransactionCommandValidator : AbstractValidator<RecordCashTransactionCommand>
{
    public RecordCashTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.EntityId)
            .GreaterThan(0);

        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}
