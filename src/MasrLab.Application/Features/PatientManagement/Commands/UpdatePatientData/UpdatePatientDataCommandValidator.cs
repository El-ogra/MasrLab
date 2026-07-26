using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public class UpdatePatientDataCommandValidator : AbstractValidator<UpdatePatientDataCommand>
{
    public UpdatePatientDataCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.AgeYears).InclusiveBetween(0, 150);
    }
}
