using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

public record CreateCombinedReportCommand : IRequest<Unit>;
