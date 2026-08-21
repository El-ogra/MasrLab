using FluentValidation;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public class AddReferralEntityCommandValidator : AbstractValidator<AddReferralEntityCommand>
{
    public AddReferralEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        When(x => x.EntityType == ReferralEntityType.TreatingDoctor, () =>
        {
            RuleFor(x => x.PriceListId).Null();
            RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).When(x => x.Discount.HasValue);
            RuleFor(x => x.Commission).GreaterThanOrEqualTo(0).When(x => x.Commission.HasValue);
        });

        When(x => x.EntityType == ReferralEntityType.ReferralEntity, () =>
        {
            RuleFor(x => x.PriceListId).NotNull().GreaterThan(0);
        });

        When(x => x.EntityType == ReferralEntityType.OutsourcedSamples, () =>
        {
            // PriceListId is not validated here — the handler enforces the Lab-to-Lab lock server-side (D-02)
        });
    }
}
