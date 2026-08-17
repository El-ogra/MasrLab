using MediatR;

using MasrLab.Domain.Common.Enums;

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
    AgeUnit AgeUnit,
    string? OverrideReason) : IRequest<Unit>;
