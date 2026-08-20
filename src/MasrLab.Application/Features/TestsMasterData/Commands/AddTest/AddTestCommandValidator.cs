using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTest;

public class AddTestCommandValidator : AbstractValidator<AddTestCommand>
{
    public AddTestCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.ReportName).NotEmpty();
        RuleFor(x => x.ReceiptName).NotEmpty();
        RuleFor(x => x.Group).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.TurnaroundTime).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.TestTimeDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ArrangeNo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LabToLabPrice).GreaterThanOrEqualTo(0).When(x => x.LabToLabPrice.HasValue);
        RuleFor(x => x.OutsourcedCostPrice).GreaterThanOrEqualTo(0).When(x => x.OutsourcedCostPrice.HasValue);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue);

        When(x => x.SentOutsideLab, () =>
        {
            RuleFor(x => x.OutsourcedLabReferralEntityId)
                .NotNull().WithMessage("External lab must be selected when test is sent outside.")
                .GreaterThan(0).WithMessage("External lab must be selected when test is sent outside.");
            RuleFor(x => x.OutsourcedCostPrice)
                .NotNull().WithMessage("Cost price is required when test is sent outside.")
                .GreaterThanOrEqualTo(0).WithMessage("Cost price must be zero or positive.");
        });

        When(x => !x.SentOutsideLab, () =>
        {
            RuleFor(x => x.OutsourcedLabReferralEntityId).Null();
            RuleFor(x => x.OutsourcedCostPrice).Null();
        });
    }
}
