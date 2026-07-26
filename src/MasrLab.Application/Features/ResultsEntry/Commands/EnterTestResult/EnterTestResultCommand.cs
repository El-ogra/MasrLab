using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public record EnterTestResultCommand : IRequest<Unit>;
