using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public class AddTestGroupCommandHandler : IRequestHandler<AddTestGroupCommand, int>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestGroupCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new TestGroup
        {
            GroupName = request.GroupName
        };

        await _repository.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Id;
    }
}
