using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public record RegisterPatientCommand : IRequest<Unit>;
