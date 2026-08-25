using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitExtraCharge;

public class RecordVisitExtraChargeCommandValidator : AbstractValidator<RecordVisitExtraChargeCommand>
{
    public RecordVisitExtraChargeCommandValidator()
    {
        RuleFor(x => x.ReceiptId).GreaterThan(0)
            .WithMessage("ReceiptId must be greater than zero.");
        RuleFor(x => x.Description).NotEmpty()
            .WithMessage("Description cannot be empty.");
        RuleFor(x => x.Amount).GreaterThan(0)
            .WithMessage("Extra service amount cannot be negative.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
