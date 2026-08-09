using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Common.Helpers;

public class VisitLabIdGenerator : IVisitLabIdGenerator
{
    private readonly IVisitRepository _visitRepository;

    public VisitLabIdGenerator(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository
            ?? throw new ArgumentNullException(nameof(visitRepository));
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var prefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var maxSuffix = await _visitRepository.GetMaxVisitLabIdSuffixAsync(prefix, cancellationToken);
        var nextNumber = (maxSuffix ?? 0) + 1;
        return $"{prefix}-{nextNumber:D4}";
    }
}
