using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;

public sealed class AddManualAntibioticToCultureTestCommandHandler : IRequestHandler<AddManualAntibioticToCultureTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IAntibioticRepository _antibioticRepository;
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IRepository<CultureAntibioticCommercialName> _commercialNameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddManualAntibioticToCultureTestCommandHandler(
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

    public async Task<Unit> Handle(AddManualAntibioticToCultureTestCommand request, CancellationToken cancellationToken)
    {
        var culture = await _testRepository.GetByIdAsync(request.CultureTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Test), request.CultureTestId);
        if (!CultureGroup.IsCulture(culture.Group))
            throw new BusinessRuleViolationException("The selected test is not a Culture and Sensitivity test.");

        var symbol = request.Symbol.Trim();
        var scientificName = request.ScientificName.Trim();
        if (await _assignmentRepository.ExistsBySymbolOrScientificNameAsync(
                request.CultureTestId, symbol, scientificName, cancellationToken))
            throw new BusinessRuleViolationException("An antibiotic with this symbol or scientific name is already assigned to this culture.");

        var antibiotic = await _antibioticRepository.GetBySymbolAsync(symbol, cancellationToken);
        if (antibiotic is null)
        {
            antibiotic = new Antibiotic { Name = symbol, ScientificName = scientificName };
            await _antibioticRepository.AddAsync(antibiotic, cancellationToken);
        }

        var assignment = antibiotic.Id > 0
            ? CultureAntibiotic.Create(
                request.CultureTestId,
                antibiotic.Id,
                request.SensitivityText,
                request.Pregnant,
                request.Children)
            : CultureAntibiotic.CreateForNewAntibiotic(
                request.CultureTestId,
                antibiotic,
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
