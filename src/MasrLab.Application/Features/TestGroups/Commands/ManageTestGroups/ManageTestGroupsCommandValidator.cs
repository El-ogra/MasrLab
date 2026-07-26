using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

public class ManageTestGroupsCommandValidator : AbstractValidator<ManageTestGroupsCommand>
{
    public ManageTestGroupsCommandValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty();
        RuleFor(x => x.GroupPrice).GreaterThan(0);
        RuleFor(x => x.TestIds).NotEmpty();
    }
}
