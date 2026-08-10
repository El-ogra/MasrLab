namespace MasrLab.Application.Common.Printing;
public sealed record OperationalReportPrintDto(string Title, DateTime From, DateTime To, IReadOnlyList<OperationalReportLineDto> Lines) : IPrintPayload;
public sealed record OperationalReportLineDto(string Label, string Value, string? Detail = null);
public interface IOperationalReportDataReader { Task<OperationalReportPrintDto> ReadAsync(string reportName, DateTime from, DateTime to, int? id, CancellationToken ct = default); }
