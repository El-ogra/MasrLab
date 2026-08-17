using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public record EnterTestResultCommand(
    int VisitTestResultItemId,
    string Value,
    string Unit,
    int EnteredByUserId,
    int PatientId,
    int AgeYears,
    int AgeMonths,
    int AgeDays,
    string? OverrideReason) : IRequest<Unit>;
