using FluentValidation;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;

public class AddDoctorCommandValidator : AbstractValidator<AddDoctorCommand>
{
    public AddDoctorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.CommissionPercent).InclusiveBetween(0, 100);
    }
}
