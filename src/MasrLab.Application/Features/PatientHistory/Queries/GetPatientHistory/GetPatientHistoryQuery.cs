using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;

public record GetPatientHistoryQuery(int PatientId) : IRequest<IReadOnlyList<PatientHistoryDto>>;
