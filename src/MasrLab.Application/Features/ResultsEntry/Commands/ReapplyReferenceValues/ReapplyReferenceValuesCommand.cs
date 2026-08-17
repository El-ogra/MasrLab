using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ReapplyReferenceValues;

public record ReapplyReferenceValuesCommand(
    int TestResultId,
    int AppliedByUserId) : IRequest<Unit>;
