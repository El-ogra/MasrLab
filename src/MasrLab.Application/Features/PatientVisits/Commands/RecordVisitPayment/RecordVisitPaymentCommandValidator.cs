using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitPayment;

public class RecordVisitPaymentCommandValidator : AbstractValidator<RecordVisitPaymentCommand>
{
    public RecordVisitPaymentCommandValidator()
    {
        RuleFor(x => x.ReceiptId).GreaterThan(0)
            .WithMessage("ReceiptId must be greater than zero.");
        RuleFor(x => x.Amount).GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
