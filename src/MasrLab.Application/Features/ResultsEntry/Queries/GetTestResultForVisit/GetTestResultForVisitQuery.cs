using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetTestResultForVisit;

public record GetTestResultForVisitQuery : IRequest<IReadOnlyList<TestResultDto>>;
