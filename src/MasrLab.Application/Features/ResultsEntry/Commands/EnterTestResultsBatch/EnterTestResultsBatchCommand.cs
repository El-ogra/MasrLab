using MediatR;
using MasrLab.Application.Features.ResultsEntry.Common;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResultsBatch;

public record EnterTestResultsBatchCommand(
    int PatientVisitId,
    int EnteredByUserId,
    IReadOnlyList<BatchResultItemRequest> Items) : IRequest<Unit>;

public record BatchResultItemRequest(
    int VisitTestResultItemId,
    string Value,
    string Unit,
    CommentPatch? CommentPatch);
