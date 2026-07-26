using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;

public record GenerateLabIdQuery : IRequest<string>;
