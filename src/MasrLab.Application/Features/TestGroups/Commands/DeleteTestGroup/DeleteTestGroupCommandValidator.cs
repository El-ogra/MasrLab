using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public class DeleteTestGroupCommandValidator : AbstractValidator<DeleteTestGroupCommand>
{
    public DeleteTestGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
