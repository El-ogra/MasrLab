namespace MasrLab.Infrastructure.Printing;

public sealed class ReportDefinitionRegistry
{
    private readonly IReadOnlyDictionary<string, IReportDefinition> _reports;

    public ReportDefinitionRegistry(IEnumerable<IReportDefinition> reports)
    {
        _reports = reports.ToDictionary(report => report.ReportName, StringComparer.OrdinalIgnoreCase);
    }

    public IReportDefinition Get(string reportName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportName);
        return _reports.TryGetValue(reportName, out var report)
            ? report
            : throw new KeyNotFoundException($"No report is registered with the name '{reportName}'.");
    }
}
