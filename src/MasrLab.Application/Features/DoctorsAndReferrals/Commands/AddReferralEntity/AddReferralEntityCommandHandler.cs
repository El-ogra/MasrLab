using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public class AddReferralEntityCommandHandler : IRequestHandler<AddReferralEntityCommand, Unit>
{
    private readonly IRepository<ReferralEntity> _referralEntityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReferralEntityCommandHandler(IRepository<ReferralEntity> referralEntityRepository, IUnitOfWork unitOfWork)
    {
        _referralEntityRepository = referralEntityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddReferralEntityCommand request, CancellationToken cancellationToken)
    {
        var referralEntity = new ReferralEntity
        {
            Name = request.Name,
            EntityType = request.EntityType,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null,
            Fax = request.Fax,
            Address = request.Address,
            PriceListId = request.PriceListId,
            AccountBalance = request.AccountBalance
        };

        await _referralEntityRepository.AddAsync(referralEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
