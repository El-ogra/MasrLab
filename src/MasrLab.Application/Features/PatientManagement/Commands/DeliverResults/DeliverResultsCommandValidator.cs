using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;

public class DeliverResultsCommandValidator : AbstractValidator<DeliverResultsCommand>
{
    public DeliverResultsCommandValidator()
    {
        RuleFor(x => x.PatientVisitId).GreaterThan(0);
    }
}
