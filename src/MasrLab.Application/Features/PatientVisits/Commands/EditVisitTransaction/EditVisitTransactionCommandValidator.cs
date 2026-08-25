using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.EditVisitTransaction;

public class EditVisitTransactionCommandValidator : AbstractValidator<EditVisitTransactionCommand>
{
    public EditVisitTransactionCommandValidator()
    {
        RuleFor(x => x.ReceiptId).GreaterThan(0)
            .WithMessage("ReceiptId must be greater than zero.");
        RuleFor(x => x.TransactionId).GreaterThan(0)
            .WithMessage("TransactionId must be greater than zero.");
        RuleFor(x => x.NewAmount).GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
