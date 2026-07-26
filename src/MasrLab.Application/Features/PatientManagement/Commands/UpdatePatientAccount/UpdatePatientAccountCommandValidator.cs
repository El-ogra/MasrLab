using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;

public class UpdatePatientAccountCommandValidator : AbstractValidator<UpdatePatientAccountCommand>
{
    public UpdatePatientAccountCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
