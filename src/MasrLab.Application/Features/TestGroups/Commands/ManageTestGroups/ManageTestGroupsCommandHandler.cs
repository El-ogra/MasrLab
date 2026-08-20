using System;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

[Obsolete("Use RenameTestGroupCommandHandler instead. This will be removed in a future version.")]
public class ManageTestGroupsCommandHandler : IRequestHandler<ManageTestGroupsCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ManageTestGroupsCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ManageTestGroupsCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdAsync(request.TestGroupId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId);

        if (await _repository.NameExistsAsync(request.NewName, request.TestGroupId, cancellationToken))
        {
            throw new BusinessRuleViolationException("A group with this name already exists.");
        }

        group.GroupName = request.NewName;
        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
