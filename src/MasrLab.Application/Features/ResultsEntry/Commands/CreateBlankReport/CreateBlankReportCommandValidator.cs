using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateBlankReport;

public class CreateBlankReportCommandValidator : AbstractValidator<CreateBlankReportCommand>
{
    public CreateBlankReportCommandValidator()
    {
        RuleFor(x => x.PatientVisitId)
            .GreaterThan(0);
    }
}
