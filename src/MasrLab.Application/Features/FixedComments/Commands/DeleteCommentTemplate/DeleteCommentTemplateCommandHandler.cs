using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.FixedComments.Commands.DeleteCommentTemplate;

public class DeleteCommentTemplateCommandHandler : IRequestHandler<DeleteCommentTemplateCommand, Unit>
{
    private readonly IRepository<CommentTemplate> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentTemplateCommandHandler(
        IRepository<CommentTemplate> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCommentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CommentTemplate), request.Id);

        if (template.TestId != request.TestId)
            throw new BusinessRuleViolationException("The comment template does not belong to the specified test.");

        template.IsDeleted = true;
        _repository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
