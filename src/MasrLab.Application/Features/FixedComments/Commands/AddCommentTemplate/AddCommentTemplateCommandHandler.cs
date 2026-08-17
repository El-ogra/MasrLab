using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.FixedComments.Commands.AddCommentTemplate;

public class AddCommentTemplateCommandHandler : IRequestHandler<AddCommentTemplateCommand, int>
{
    private readonly IRepository<CommentTemplate> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentTemplateCommandHandler(
        IRepository<CommentTemplate> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddCommentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new CommentTemplate
        {
            TestId = request.TestId,
            Text = request.Text
        };

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
