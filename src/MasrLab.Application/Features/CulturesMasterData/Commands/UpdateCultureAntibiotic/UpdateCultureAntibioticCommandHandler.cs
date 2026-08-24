using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;

public sealed class UpdateCultureAntibioticCommandHandler : IRequestHandler<UpdateCultureAntibioticCommand, Unit>
{
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IRepository<CultureAntibioticCommercialName> _commercialNameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCultureAntibioticCommandHandler(
        ICultureAntibioticRepository assignmentRepository,
        IRepository<CultureAntibioticCommercialName> commercialNameRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _commercialNameRepository = commercialNameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCultureAntibioticCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetWithCommercialNamesAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CultureAntibiotic), request.Id);

        assignment.SensitivityText = request.SensitivityText;
        assignment.Pregnant = request.Pregnant;
        assignment.Children = request.Children;
        _assignmentRepository.Update(assignment);

        var submitted = (request.CommercialNames ?? Array.Empty<CommercialNameInput>())
            .Where(input => !string.IsNullOrWhiteSpace(input.Name))
            .GroupBy(input => input.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToList();
        var submittedNames = submitted
            .Select(input => input.Name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var existing in assignment.CommercialNames.ToList())
        {
            if (!submittedNames.Contains(existing.Name.Trim()))
            {
                _commercialNameRepository.Delete(existing);
                continue;
            }

            var input = submitted.First(item =>
                string.Equals(item.Name.Trim(), existing.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            existing.Name = input.Name.Trim();
            existing.Print = input.Print;
            _commercialNameRepository.Update(existing);
        }

        var existingNames = assignment.CommercialNames
            .Where(name => !name.IsDeleted)
            .Select(name => name.Name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var input in submitted.Where(item => !existingNames.Contains(item.Name.Trim())))
        {
            await _commercialNameRepository.AddAsync(new CultureAntibioticCommercialName
            {
                CultureAntibioticId = assignment.Id,
                Name = input.Name.Trim(),
                Print = input.Print
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
