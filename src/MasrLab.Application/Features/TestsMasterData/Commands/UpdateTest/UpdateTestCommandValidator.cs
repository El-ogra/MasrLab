using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;

public class UpdateTestCommandValidator : AbstractValidator<UpdateTestCommand>
{
    public UpdateTestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.ReportName).NotEmpty();
        RuleFor(x => x.ReceiptName).NotEmpty();
        RuleFor(x => x.Group).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.TurnaroundTime).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
    }
}
