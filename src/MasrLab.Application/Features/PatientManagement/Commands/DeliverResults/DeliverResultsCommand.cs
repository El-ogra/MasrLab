using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;

public record DeliverResultsCommand(
    int PatientVisitId
) : IRequest<Unit>;
