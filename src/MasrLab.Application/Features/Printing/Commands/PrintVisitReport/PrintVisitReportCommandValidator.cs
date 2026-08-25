using FluentValidation;
using MasrLab.Application.Common.Printing;

namespace MasrLab.Application.Features.Printing.Commands.PrintVisitReport;

public class PrintVisitReportCommandValidator : AbstractValidator<PrintVisitReportCommand>
{
    public PrintVisitReportCommandValidator()
    {
        RuleFor(x => x.VisitTestId).GreaterThan(0)
            .WithMessage("VisitTestId must be greater than zero.");
        RuleFor(x => x.UserId).GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
        RuleFor(x => x.Kind).IsInEnum()
            .WithMessage("Kind must be a valid visit report kind.");
        RuleFor(x => x.ReportId)
            .NotNull()
            .When(x => x.Kind is VisitReportKind.Consolidated or VisitReportKind.Blank)
            .WithMessage("ReportId is required for consolidated and blank reports.");
        RuleFor(x => x.ReportId)
            .GreaterThan(0)
            .When(x => x.ReportId.HasValue)
            .WithMessage("ReportId must be greater than zero when specified.");
    }
}
