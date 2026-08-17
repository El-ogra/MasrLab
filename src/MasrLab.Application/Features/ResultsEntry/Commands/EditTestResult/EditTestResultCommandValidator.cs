using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EditTestResult;

public class EditTestResultCommandValidator : AbstractValidator<EditTestResultCommand>
{
    public EditTestResultCommandValidator()
    {
        RuleFor(x => x.TestResultId).GreaterThan(0);
        RuleFor(x => x.EditedByUserId).GreaterThan(0);
        RuleFor(x => x.NewValue).MaximumLength(500);
    }
}
