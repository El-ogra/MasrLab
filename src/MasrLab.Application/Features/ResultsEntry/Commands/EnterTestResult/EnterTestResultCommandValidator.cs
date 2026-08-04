using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandValidator : AbstractValidator<EnterTestResultCommand>
{
    public EnterTestResultCommandValidator()
    {
        RuleFor(x => x.VisitTestId)
            .GreaterThan(0);

        RuleFor(x => x.Value)
            .NotEmpty();

        RuleFor(x => x.Unit)
            .NotEmpty();

        RuleFor(x => x.EnteredByUserId)
            .GreaterThan(0);

        RuleFor(x => x.PatientId)
            .GreaterThan(0);

        RuleFor(x => x.AgeYears)
            .GreaterThanOrEqualTo(0);
    }
}
