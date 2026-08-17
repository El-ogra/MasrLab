using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.FixedComments.Commands.UpdateCommentTemplate;

public class UpdateCommentTemplateCommandHandler : IRequestHandler<UpdateCommentTemplateCommand, Unit>
{
    private readonly IRepository<CommentTemplate> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentTemplateCommandHandler(
        IRepository<CommentTemplate> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCommentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CommentTemplate), request.Id);

        if (template.TestId != request.TestId)
            throw new BusinessRuleViolationException("The comment template does not belong to the specified test.");

        template.Text = request.Text;
        _repository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
