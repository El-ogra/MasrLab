using FluentValidation;

namespace MasrLab.Application.Features.CasesFollowUp.Commands.AddCaseFollowUp;

public class AddCaseFollowUpCommandValidator : AbstractValidator<AddCaseFollowUpCommand>
{
    public AddCaseFollowUpCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Notes).NotEmpty();
    }
}
