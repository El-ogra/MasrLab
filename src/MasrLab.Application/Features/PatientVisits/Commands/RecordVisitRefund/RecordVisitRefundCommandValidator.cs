using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitRefund;

public class RecordVisitRefundCommandValidator : AbstractValidator<RecordVisitRefundCommand>
{
    public RecordVisitRefundCommandValidator()
    {
        RuleFor(x => x.ReceiptId).GreaterThan(0)
            .WithMessage("ReceiptId must be greater than zero.");
        RuleFor(x => x.Amount).GreaterThan(0)
            .WithMessage("Refund amount must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
