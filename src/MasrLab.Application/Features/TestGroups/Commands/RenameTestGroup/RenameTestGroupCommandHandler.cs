using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public class RenameTestGroupCommandHandler : IRequestHandler<RenameTestGroupCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameTestGroupCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RenameTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.Id);

        group.GroupName = request.GroupName;
        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
