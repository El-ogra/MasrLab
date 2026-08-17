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
        RuleFor(x => x.OutsourcedLabName).NotEmpty().When(x => x.SentOutsideLab);
    }
}
