using FluentValidation;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;

public class UpdateReferralEntityCommandValidator : AbstractValidator<UpdateReferralEntityCommand>
{
    public UpdateReferralEntityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
    }
}
