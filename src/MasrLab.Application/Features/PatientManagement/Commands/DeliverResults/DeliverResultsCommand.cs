using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;

public record DeliverResultsCommand : IRequest<Unit>;
