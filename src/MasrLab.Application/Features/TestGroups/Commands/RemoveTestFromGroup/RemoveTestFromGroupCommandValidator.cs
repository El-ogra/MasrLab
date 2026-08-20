using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public class RemoveTestFromGroupCommandValidator : AbstractValidator<RemoveTestFromGroupCommand>
{
    public RemoveTestFromGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
