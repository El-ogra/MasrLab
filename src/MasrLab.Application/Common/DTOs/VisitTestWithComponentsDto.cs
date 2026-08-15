namespace MasrLab.Application.Common.DTOs;

public class VisitTestWithComponentsDto
{
    public int VisitTestId { get; set; }
    public int TestId { get; set; }
    public string TestNameSnapshot { get; set; } = string.Empty;
    public string ReportNameSnapshot { get; set; } = string.Empty;
    public bool IsCompoundSnapshot { get; set; }
    public decimal Price { get; set; }
    public List<VisitTestResultItemWithResultDto> Components { get; set; } = new();
}

public class VisitTestResultItemWithResultDto
{
    public int Id { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentUnit { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string ResultEntryKind { get; set; } = string.Empty;
    public TestResultDto? TestResult { get; set; }
    public CultureResultDto? CultureResult { get; set; }
    public bool IsComplete => ResultEntryKind == "CultureDetail"
        ? CultureResult is not null
        : TestResult is not null;
}
