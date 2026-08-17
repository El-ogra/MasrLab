using FluentValidation;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ApplyCommentTemplate;

public class ApplyCommentTemplateCommandValidator : AbstractValidator<ApplyCommentTemplateCommand>
{
    public ApplyCommentTemplateCommandValidator()
    {
        RuleFor(x => x.TestResultId).GreaterThan(0);
        RuleFor(x => x.CommentTemplateId).GreaterThan(0);
        RuleFor(x => x.AppliedByUserId).GreaterThan(0);
    }
}
