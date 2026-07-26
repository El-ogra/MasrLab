using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.LabId).NotEmpty();
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.ReferralEntityId).GreaterThan(0);
        RuleFor(x => x.AgeYears).InclusiveBetween(0, 150);
    }
}
