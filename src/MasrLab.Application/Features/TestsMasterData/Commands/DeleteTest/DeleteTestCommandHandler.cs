using MasrLab.Application.Features.TestsMasterData.Commands.DeleteTest;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteTest;

public class DeleteTestCommandHandler : IRequestHandler<DeleteTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestCommandHandler(IRepository<Test> testRepository, IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdAsync(request.Id, cancellationToken);
        if (test is null)
            throw new EntityNotFoundException(nameof(Test), request.Id);

        _testRepository.Delete(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
