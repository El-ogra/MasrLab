using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResultsBatch;

public class EnterTestResultsBatchCommandValidator : AbstractValidator<EnterTestResultsBatchCommand>
{
    public EnterTestResultsBatchCommandValidator()
    {
        RuleFor(x => x.PatientVisitId).GreaterThan(0);
        RuleFor(x => x.EnteredByUserId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VisitTestResultItemId).GreaterThan(0);
            item.RuleFor(i => i.Value).NotEmpty();
            item.RuleFor(i => i.Unit).NotEmpty();
        });
    }
}
