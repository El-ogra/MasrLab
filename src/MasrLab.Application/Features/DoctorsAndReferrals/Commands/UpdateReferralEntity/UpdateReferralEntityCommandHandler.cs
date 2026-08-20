using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;

public class UpdateReferralEntityCommandHandler : IRequestHandler<UpdateReferralEntityCommand, Unit>
{
    private readonly IReferralEntityRepository _repository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferralEntityCommandHandler(
        IReferralEntityRepository repository,
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReferralEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new EntityNotFoundException(nameof(ReferralEntity), request.Id);

        int? resolvedPriceListId = request.PriceListId;

        if (entity.EntityType == ReferralEntityType.TreatingDoctor)
        {
            resolvedPriceListId = null;
        }
        else if (entity.EntityType == ReferralEntityType.OutsourcedSamples)
        {
            var labToLabList = await _priceListRepository.GetLabToLabAsync(cancellationToken);
            if (labToLabList is null)
                throw new BusinessRuleViolationException("No Lab-to-Lab price list exists. Create one first.");
            resolvedPriceListId = labToLabList.Id;
        }
        else if (entity.EntityType == ReferralEntityType.ReferralEntity && request.PriceListId is not null)
        {
            var priceList = await _priceListRepository.GetByIdAsync(request.PriceListId.Value, cancellationToken);
            if (priceList is null)
                throw new EntityNotFoundException(nameof(MasrLab.Domain.Entities.Settings.PriceList), request.PriceListId.Value);
        }
        else if (entity.EntityType == ReferralEntityType.ReferralEntity && request.PriceListId is null)
        {
            throw new BusinessRuleViolationException("Referral/Contract entities require a price list.");
        }

        entity.Name = request.Name;
        entity.ContactPerson = request.ContactPerson;
        entity.Phone = request.Phone is not null ? new EgyptianPhone(request.Phone) : null;
        entity.Fax = request.Fax;
        entity.Address = request.Address;
        entity.City = request.City;
        entity.Discount = request.Discount;
        entity.Commission = request.Commission;
        entity.PriceListId = resolvedPriceListId;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
