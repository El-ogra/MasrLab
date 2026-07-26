using FluentValidation;

namespace MasrLab.Application.Features.FixedComments.Commands.ManageComments;

public class ManageCommentsCommandValidator : AbstractValidator<ManageCommentsCommand>
{
    public ManageCommentsCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.CommentText).NotEmpty();
    }
}
