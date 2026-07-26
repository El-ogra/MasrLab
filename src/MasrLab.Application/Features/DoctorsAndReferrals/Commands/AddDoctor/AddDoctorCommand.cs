using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;

public record AddDoctorCommand(
    string Name,
    string? Phone,
    string? Address,
    decimal CommissionPercent
) : IRequest<Unit>;
