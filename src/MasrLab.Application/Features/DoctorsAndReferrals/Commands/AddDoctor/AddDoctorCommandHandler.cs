using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;

public class AddDoctorCommandHandler : IRequestHandler<AddDoctorCommand, Unit>
{
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddDoctorCommandHandler(IRepository<Doctor> doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = new Doctor
        {
            Name = request.Name,
            Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null,
            Address = request.Address,
            CommissionPercent = request.CommissionPercent
        };

        await _doctorRepository.AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
