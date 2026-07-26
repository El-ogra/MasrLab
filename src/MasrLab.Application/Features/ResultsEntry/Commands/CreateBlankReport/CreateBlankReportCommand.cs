using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateBlankReport;

public record CreateBlankReportCommand(int PatientVisitId) : IRequest<Unit>;
