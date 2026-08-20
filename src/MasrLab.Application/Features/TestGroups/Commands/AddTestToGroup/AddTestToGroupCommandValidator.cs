using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public class AddTestToGroupCommandValidator : AbstractValidator<AddTestToGroupCommand>
{
    public AddTestToGroupCommandValidator()
    {
        RuleFor(x => x.TestGroupId).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
