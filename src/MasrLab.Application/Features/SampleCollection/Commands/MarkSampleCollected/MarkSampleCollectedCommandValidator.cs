using FluentValidation;

namespace MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;

public class MarkSampleCollectedCommandValidator : AbstractValidator<MarkSampleCollectedCommand>
{
    public MarkSampleCollectedCommandValidator()
    {
        RuleFor(x => x.SampleId)
            .GreaterThan(0);

        RuleFor(x => x.PatientId)
            .GreaterThan(0);
    }
}
