using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public class RenameTestGroupCommandValidator : AbstractValidator<RenameTestGroupCommand>
{
    public RenameTestGroupCommandValidator()
    {
        RuleFor(x => x.TestGroupId).GreaterThan(0);
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
