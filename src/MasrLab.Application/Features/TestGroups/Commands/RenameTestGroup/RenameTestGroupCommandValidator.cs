using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public class RenameTestGroupCommandValidator : AbstractValidator<RenameTestGroupCommand>
{
    public RenameTestGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.GroupName).NotEmpty();
    }
}
