using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.IssueReceipt;

public class IssueReceiptCommandValidator : AbstractValidator<IssueReceiptCommand>
{
    public IssueReceiptCommandValidator()
    {
        RuleFor(x => x.PatientVisitId)
            .GreaterThan(0)
            .WithMessage("PatientVisitId must be greater than zero.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount cannot be negative.");

        RuleFor(x => x.PaidNow)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PaidNow cannot be negative.");

        RuleFor(x => x.ReceivedByUserId)
            .GreaterThan(0)
            .WithMessage("ReceivedByUserId must be greater than zero.");

        RuleFor(x => x.CashAccountId)
            .GreaterThan(0)
            .When(x => x.CashAccountId.HasValue)
            .WithMessage("CashAccountId must be greater than zero when specified.");
    }
}
