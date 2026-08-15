using MasrLab.Application.Features.TestsMasterData.Commands.AddTestComponent;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTestComponent;

public class AddTestComponentCommandHandler : IRequestHandler<AddTestComponentCommand, int>
{
    private readonly IRepository<Test> _testRepository;
    private readonly ITestComponentCardinalityService _cardinalityService;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestComponentCommandHandler(
        IRepository<Test> testRepository,
        ITestComponentCardinalityService cardinalityService,
        IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _cardinalityService = cardinalityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddTestComponentCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdAsync(request.TestId, cancellationToken);
        if (test is null)
            throw new EntityNotFoundException(nameof(Test), request.TestId);

        await _cardinalityService.ValidateAddComponentAsync(test, cancellationToken);

        var component = TestComponent.Create(
            request.TestId,
            request.Name,
            request.Unit,
            request.DisplayOrder,
            request.ResultEntryKind);

        test.TestComponents.Add(component);
        _testRepository.Update(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return component.Id;
    }
}
