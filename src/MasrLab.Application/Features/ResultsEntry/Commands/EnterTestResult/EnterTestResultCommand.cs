using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public record EnterTestResultCommand(
    int VisitTestResultItemId,
    string Value,
    string Unit,
    string ReferenceRange,
    ResultStatus Status,
    int EnteredByUserId,
    int PatientId,
    int AgeYears,
    string? OverrideReason) : IRequest<Unit>;
