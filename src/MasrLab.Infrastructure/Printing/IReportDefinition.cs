using MasrLab.Application.Common.Printing;

namespace MasrLab.Infrastructure.Printing;

public interface IReportDefinition
{
    string ReportName { get; }
    Type PayloadType { get; }
    byte[] Render(IPrintPayload payload);
}
