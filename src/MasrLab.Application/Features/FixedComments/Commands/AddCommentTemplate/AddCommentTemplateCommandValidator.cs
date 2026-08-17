using FluentValidation;

namespace MasrLab.Application.Features.FixedComments.Commands.AddCommentTemplate;

public class AddCommentTemplateCommandValidator : AbstractValidator<AddCommentTemplateCommand>
{
    public AddCommentTemplateCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}
