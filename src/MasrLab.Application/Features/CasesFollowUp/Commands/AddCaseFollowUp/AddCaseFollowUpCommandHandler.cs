using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.CasesFollowUp.Commands.AddCaseFollowUp;

public class AddCaseFollowUpCommandHandler : IRequestHandler<AddCaseFollowUpCommand, Unit>
{
    private readonly IRepository<CaseFollowUpNote> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCaseFollowUpCommandHandler(IRepository<CaseFollowUpNote> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddCaseFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUpNote = new CaseFollowUpNote
        {
            TestId = request.TestId,
            Notes = request.Notes
        };

        await _repository.AddAsync(followUpNote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
