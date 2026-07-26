using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientManagement.Queries.GetPatientById;

public record GetPatientByIdQuery : IRequest<PatientDto?>;
