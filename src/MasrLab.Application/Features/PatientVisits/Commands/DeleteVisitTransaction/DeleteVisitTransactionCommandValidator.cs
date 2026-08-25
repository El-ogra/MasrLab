using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.DeleteVisitTransaction;

public class DeleteVisitTransactionCommandValidator : AbstractValidator<DeleteVisitTransactionCommand>
{
    public DeleteVisitTransactionCommandValidator()
    {
        RuleFor(x => x.ReceiptId).GreaterThan(0)
            .WithMessage("ReceiptId must be greater than zero.");
        RuleFor(x => x.TransactionId).GreaterThan(0)
            .WithMessage("TransactionId must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
