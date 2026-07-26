using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

public class CreateCombinedReportCommandValidator : AbstractValidator<CreateCombinedReportCommand>
{
    public CreateCombinedReportCommandValidator()
    {
        RuleFor(x => x.PatientVisitId)
            .GreaterThan(0);

        RuleFor(x => x.TestIds)
            .NotEmpty();
    }
}
