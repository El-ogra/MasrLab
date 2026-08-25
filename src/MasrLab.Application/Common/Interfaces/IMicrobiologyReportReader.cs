namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Features.Cultures.Queries.GetMicrobiologyReport;

public interface IMicrobiologyReportReader
{
    Task<MicrobiologyReportDto?> GetAsync(int cultureId, CancellationToken cancellationToken = default);
}
