using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;

public sealed class AddAntibioticToCultureTestCommandHandler : IRequestHandler<AddAntibioticToCultureTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IAntibioticRepository _antibioticRepository;
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IRepository<CultureAntibioticCommercialName> _commercialNameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAntibioticToCultureTestCommandHandler(
        IRepository<Test> testRepository,
        IAntibioticRepository antibioticRepository,
        ICultureAntibioticRepository assignmentRepository,
        IRepository<CultureAntibioticCommercialName> commercialNameRepository,
        IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _antibioticRepository = antibioticRepository;
        _assignmentRepository = assignmentRepository;
        _commercialNameRepository = commercialNameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddAntibioticToCultureTestCommand request, CancellationToken cancellationToken)
    {
        var culture = await _testRepository.GetByIdAsync(request.CultureTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Test), request.CultureTestId);
        if (!CultureGroup.IsCulture(culture.Group))
            throw new BusinessRuleViolationException("The selected test is not a Culture and Sensitivity test.");

        var antibiotic = await _antibioticRepository.GetByIdAsync(request.AntibioticId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Antibiotic), request.AntibioticId);
        if (await _assignmentRepository.ExistsAsync(request.CultureTestId, request.AntibioticId, cancellationToken))
            throw new BusinessRuleViolationException("The antibiotic is already assigned to this culture; edit the existing row.");

        var assignment = CultureAntibiotic.Create(
            request.CultureTestId,
            antibiotic.Id,
            request.SensitivityText,
            request.Pregnant,
            request.Children);
        await _assignmentRepository.AddAsync(assignment, cancellationToken);

        foreach (var input in request.CommercialNames ?? Array.Empty<CommercialNameInput>())
        {
            await _commercialNameRepository.AddAsync(new CultureAntibioticCommercialName
            {
                CultureAntibiotic = assignment,
                Name = input.Name.Trim(),
                Print = input.Print
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
