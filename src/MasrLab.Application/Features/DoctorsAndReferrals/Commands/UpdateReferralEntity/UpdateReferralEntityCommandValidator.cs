using FluentValidation;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;

public class UpdateReferralEntityCommandValidator : AbstractValidator<UpdateReferralEntityCommand>
{
    public UpdateReferralEntityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).When(x => x.Discount.HasValue);
        RuleFor(x => x.Commission).GreaterThanOrEqualTo(0).When(x => x.Commission.HasValue);
        RuleFor(x => x.PriceListId).GreaterThan(0).When(x => x.PriceListId.HasValue);
    }
}
