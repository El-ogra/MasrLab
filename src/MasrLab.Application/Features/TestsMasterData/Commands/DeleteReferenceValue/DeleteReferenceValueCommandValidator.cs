using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;

public class DeleteReferenceValueCommandValidator : AbstractValidator<DeleteReferenceValueCommand>
{
    public DeleteReferenceValueCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
