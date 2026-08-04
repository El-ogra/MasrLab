using FluentValidation;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public class MarkTestAsOutsourcedCommandValidator : AbstractValidator<MarkTestAsOutsourcedCommand>
{
    public MarkTestAsOutsourcedCommandValidator()
    {
        RuleFor(x => x.PatientVisitId).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.ExternalLabId).GreaterThan(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PatientPrice).GreaterThanOrEqualTo(0);
    }
}
