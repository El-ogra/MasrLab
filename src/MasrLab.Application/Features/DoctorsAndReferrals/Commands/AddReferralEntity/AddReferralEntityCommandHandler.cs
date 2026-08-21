using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public class AddReferralEntityCommandHandler : IRequestHandler<AddReferralEntityCommand, Unit>
{
    private readonly IRepository<ReferralEntity> _referralEntityRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReferralEntityCommandHandler(
        IRepository<ReferralEntity> referralEntityRepository,
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _referralEntityRepository = referralEntityRepository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddReferralEntityCommand request, CancellationToken cancellationToken)
    {
        int? resolvedPriceListId = request.PriceListId;

        if (request.EntityType == ReferralEntityType.TreatingDoctor)
        {
            resolvedPriceListId = null;
        }
        else if (request.EntityType == ReferralEntityType.OutsourcedSamples)
        {
            var labToLabList = await _priceListRepository.GetLabToLabAsync(cancellationToken);
            if (labToLabList is null)
                throw new BusinessRuleViolationException("No Lab-to-Lab price list exists. Create one first.");
            resolvedPriceListId = labToLabList.Id;
        }
        else if (request.EntityType == ReferralEntityType.ReferralEntity && request.PriceListId is null)
        {
            throw new BusinessRuleViolationException("Referral/Contract entities require a price list.");
        }
        else if (request.EntityType == ReferralEntityType.ReferralEntity && request.PriceListId is not null)
        {
            var priceList = await _priceListRepository.GetByIdAsync(request.PriceListId.Value, cancellationToken);
            if (priceList is null)
                throw new EntityNotFoundException(nameof(MasrLab.Domain.Entities.Settings.PriceList), request.PriceListId.Value);
        }

        var referralEntity = new ReferralEntity
        {
            Name = request.Name,
            EntityType = request.EntityType,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null,
            Fax = request.Fax,
            Address = request.Address,
            City = request.City,
            Discount = request.Discount,
            Commission = request.Commission,
            PriceListId = resolvedPriceListId
        };

        await _referralEntityRepository.AddAsync(referralEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
