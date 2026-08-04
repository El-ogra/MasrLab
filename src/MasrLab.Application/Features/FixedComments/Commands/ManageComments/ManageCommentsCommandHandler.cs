using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.FixedComments.Commands.ManageComments;

public class ManageCommentsCommandHandler : IRequestHandler<ManageCommentsCommand, Unit>
{
    private readonly IRepository<Comment> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ManageCommentsCommandHandler(IRepository<Comment> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ManageCommentsCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            var existingComment = await _repository.GetByIdAsync(request.Id.Value);
            if (existingComment != null)
            {
                existingComment.CommentText = request.CommentText;
                _repository.Update(existingComment);
            }
        }
        else
        {
            var comment = Comment.AttachToResult(request.TestId, request.CommentText);
            await _repository.AddAsync(comment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
