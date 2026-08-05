using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.CasesFollowUp.Commands.AddCaseFollowUp;

public class AddCaseFollowUpCommandHandler : IRequestHandler<AddCaseFollowUpCommand, Unit>
{
    private readonly IRepository<CommentTemplate> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCaseFollowUpCommandHandler(IRepository<CommentTemplate> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddCaseFollowUpCommand request, CancellationToken cancellationToken)
    {
        var commentTemplate = new CommentTemplate
        {
            TestId = request.TestId,
            Text = request.Notes
        };

        await _repository.AddAsync(commentTemplate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
