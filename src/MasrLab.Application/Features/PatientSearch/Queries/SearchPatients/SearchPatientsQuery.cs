using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;

public record SearchPatientsQuery(string SearchTerm) : IRequest<IReadOnlyList<PatientDto>>;
