using MediatR;
using MasrLab.Application.Features.ResultsEntry.Common;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EditTestResult;

public record EditTestResultCommand(
    int TestResultId,
    int EditedByUserId,
    string? NewValue,
    CommentPatch? CommentPatch) : IRequest<Unit>;
