using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Commands.SaveMicroscopicFindings;

// M4-BR-17: persists the microscopic examination block rows (Bacteria is derived).
public sealed record SaveMicroscopicFindingsCommand(
    int CultureId,
    IReadOnlyList<MicroscopicFindingRowInput> Findings) : IRequest;

public sealed record MicroscopicFindingRowInput(MicroscopicFindingRow RowKey, string Value, bool IncludeInPrint = true);

public class SaveMicroscopicFindingsCommandHandler : IRequestHandler<SaveMicroscopicFindingsCommand>
{
    private readonly ICultureRepository _cultureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveMicroscopicFindingsCommandHandler(ICultureRepository cultureRepository, IUnitOfWork unitOfWork)
    {
        _cultureRepository = cultureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SaveMicroscopicFindingsCommand request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Culture), request.CultureId);

        foreach (var finding in request.Findings)
        {
            if (finding.RowKey == MicroscopicFindingRow.Bacteria)
                continue; // system-derived — silently ignored for user saves.
            culture.SetMicroscopicFinding(finding.RowKey, finding.Value);
            var persisted = culture.MicroscopicFindings.FirstOrDefault(f => f.RowKey == finding.RowKey);
            if (persisted is not null)
                persisted.IncludeInPrint = finding.IncludeInPrint;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
