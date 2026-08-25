using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.SetVisitTestWorkflowFlags;

public class SetVisitTestWorkflowFlagsCommandValidator : AbstractValidator<SetVisitTestWorkflowFlagsCommand>
{
    public SetVisitTestWorkflowFlagsCommandValidator()
    {
        RuleFor(x => x.VisitTestId).GreaterThan(0)
            .WithMessage("VisitTestId must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
