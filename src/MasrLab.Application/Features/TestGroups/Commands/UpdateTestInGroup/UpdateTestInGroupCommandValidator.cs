using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public class UpdateTestInGroupCommandValidator : AbstractValidator<UpdateTestInGroupCommand>
{
    public UpdateTestInGroupCommandValidator()
    {
        RuleFor(x => x.TestGroupItemId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0)
            .When(x => x.DisplayOrder.HasValue);
    }
}
