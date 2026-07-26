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
    }
}
