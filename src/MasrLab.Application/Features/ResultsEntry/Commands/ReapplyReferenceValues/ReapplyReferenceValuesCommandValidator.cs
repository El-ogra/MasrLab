using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ReapplyReferenceValues;

public sealed class ReapplyReferenceValuesCommandValidator
    : AbstractValidator<ReapplyReferenceValuesCommand>
{
    public ReapplyReferenceValuesCommandValidator()
    {
        RuleFor(x => x.TestResultId).GreaterThan(0);
        RuleFor(x => x.AppliedByUserId).GreaterThan(0);
    }
}
