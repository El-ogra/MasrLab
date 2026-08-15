using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetResultTree;

public record GetResultTreeQuery(int PatientVisitId) : IRequest<IReadOnlyList<VisitTestWithComponentsDto>>;
