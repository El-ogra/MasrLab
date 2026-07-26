using FluentValidation;

namespace MasrLab.Application.Features.Accounting.Commands.CreateDoctorDrawer;

public class CreateDoctorDrawerCommandValidator : AbstractValidator<CreateDoctorDrawerCommand>
{
    public CreateDoctorDrawerCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0);

        RuleFor(x => x.PeriodEnd)
            .Must((command, periodEnd) => periodEnd > command.PeriodStart)
            .WithMessage("Period end must be greater than period start.");
    }
}
