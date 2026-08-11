using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteTest;

public class DeleteTestCommandValidator : AbstractValidator<DeleteTestCommand>
{
    public DeleteTestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
