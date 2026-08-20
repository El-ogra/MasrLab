using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public class AddTestGroupCommandValidator : AbstractValidator<AddTestGroupCommand>
{
    public AddTestGroupCommandValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty();
    }
}
