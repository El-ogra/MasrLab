using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;

public record AddDoctorCommand : IRequest<Unit>;
