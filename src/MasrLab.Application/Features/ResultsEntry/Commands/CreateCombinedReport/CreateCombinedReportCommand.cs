using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

public record CreateCombinedReportCommand(int PatientVisitId, string TestIds) : IRequest<Unit>;
