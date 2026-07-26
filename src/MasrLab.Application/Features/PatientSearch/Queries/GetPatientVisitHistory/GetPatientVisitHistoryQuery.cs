using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;

public record GetPatientVisitHistoryQuery : IRequest<IReadOnlyList<VisitDto>>;
