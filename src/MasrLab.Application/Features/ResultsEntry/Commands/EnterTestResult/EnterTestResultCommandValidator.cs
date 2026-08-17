using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandValidator : AbstractValidator<EnterTestResultCommand>
{
    public EnterTestResultCommandValidator()
    {
        RuleFor(x => x.VisitTestResultItemId)
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

        RuleFor(x => x.AgeMonths)
            .InclusiveBetween(0, 11);

        RuleFor(x => x.AgeDays)
            .InclusiveBetween(0, 30);

        RuleFor(x => x.AgeUnit)
            .IsInEnum();

        RuleFor(x => x.OverrideReason)
            .MaximumLength(500);
    }
}
