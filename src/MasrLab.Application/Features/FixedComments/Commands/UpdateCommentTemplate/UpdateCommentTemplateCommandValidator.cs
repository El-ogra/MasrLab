using FluentValidation;

namespace MasrLab.Application.Features.FixedComments.Commands.UpdateCommentTemplate;

public class UpdateCommentTemplateCommandValidator : AbstractValidator<UpdateCommentTemplateCommand>
{
    public UpdateCommentTemplateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}
