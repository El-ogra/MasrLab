using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public record UpdatePatientDataCommand : IRequest<Unit>;
