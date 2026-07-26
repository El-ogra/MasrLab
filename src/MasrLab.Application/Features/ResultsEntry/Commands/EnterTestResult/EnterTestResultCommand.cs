using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public record EnterTestResultCommand(int VisitTestId, string Value, string Unit, string ReferenceRange, ResultStatus Status, int EnteredByUserId) : IRequest<Unit>;
