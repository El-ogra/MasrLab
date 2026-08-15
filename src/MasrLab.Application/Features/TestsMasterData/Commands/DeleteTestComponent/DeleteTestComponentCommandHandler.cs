using MasrLab.Application.Features.TestsMasterData.Commands.DeleteTestComponent;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteTestComponent;

public class DeleteTestComponentCommandHandler : IRequestHandler<DeleteTestComponentCommand, Unit>
{
    private readonly ITestRepository _testRepository;
    private readonly IRepository<TestComponent> _componentRepository;
    private readonly ITestComponentCardinalityService _cardinalityService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestComponentCommandHandler(
        ITestRepository testRepository,
        IRepository<TestComponent> componentRepository,
        ITestComponentCardinalityService cardinalityService,
        IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _componentRepository = componentRepository;
        _cardinalityService = cardinalityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTestComponentCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdWithComponentsAsync(request.TestId, cancellationToken);
        if (test is null)
            throw new EntityNotFoundException(nameof(Test), request.TestId);

        var component = await _componentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (component is null)
            throw new EntityNotFoundException(nameof(TestComponent), request.Id);

        await _cardinalityService.ValidateRemoveComponentAsync(test, request.Id, cancellationToken);

        test.TestComponents.Remove(component);
        _testRepository.Update(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
