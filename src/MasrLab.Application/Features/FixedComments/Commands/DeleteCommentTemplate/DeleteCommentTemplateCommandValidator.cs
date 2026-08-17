using FluentValidation;

namespace MasrLab.Application.Features.FixedComments.Commands.DeleteCommentTemplate;

public class DeleteCommentTemplateCommandValidator : AbstractValidator<DeleteCommentTemplateCommand>
{
    public DeleteCommentTemplateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
    }
}
