using System;
using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

[Obsolete("Use RenameTestGroupCommandValidator instead. This will be removed in a future version.")]
public class ManageTestGroupsCommandValidator : AbstractValidator<ManageTestGroupsCommand>
{
    public ManageTestGroupsCommandValidator()
    {
        RuleFor(x => x.TestGroupId).GreaterThan(0);
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
