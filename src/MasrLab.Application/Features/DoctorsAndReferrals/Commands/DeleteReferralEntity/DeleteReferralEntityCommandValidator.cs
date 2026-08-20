using FluentValidation;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.DeleteReferralEntity;

public class DeleteReferralEntityCommandValidator : AbstractValidator<DeleteReferralEntityCommand>
{
    public DeleteReferralEntityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
