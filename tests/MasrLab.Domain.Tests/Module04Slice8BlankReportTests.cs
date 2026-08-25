using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 8 — OQ-M4-8 persisted blank reports: title/row invariants and ordering.
public class Module04Slice8BlankReportTests
{
    [Fact]
    public void Create_WhenTitleMissing_ShouldThrowBusinessRuleViolation()
    {
        Assert.Throws<BusinessRuleViolationException>(
            () => BlankReport.Create(1, "  ", null, null));
        Assert.Throws<BusinessRuleViolationException>(
            () => BlankReport.Create(0, "Title", null, null)); // invalid visit id.
    }

    [Fact]
    public void AddRow_AssignsSequentialDisplayOrder()
    {
        var report = BlankReport.Create(1, "تقرير فارغ", null, null);
        report.AddRow("CBC", "", "", "", "");
        report.AddRow("LFT", "", "", "", "");
        report.AddRow("RFT", "", "", "", "");

        Assert.Equal(new[] { 1, 2, 3 }, report.Rows.Select(r => r.DisplayOrder));
    }

    [Fact]
    public void AddRow_WhenTestNameMissing_ShouldThrowBusinessRuleViolation()
    {
        var report = BlankReport.Create(1, "تقرير فارغ", null, null);

        Assert.Throws<BusinessRuleViolationException>(() => report.AddRow(" ", "", "", "", ""));
    }

    [Fact]
    public void MarkPrinted_IncrementsCountAndStampsMetadata()
    {
        var report = BlankReport.Create(1, "تقرير فارغ", null, null);

        report.MarkPrinted(7);
        report.MarkPrinted(7);

        Assert.Equal(2, report.PrintCount);
        Assert.Equal(7, report.PrintedByUserId);
        Assert.NotNull(report.PrintedAt);
    }

    [Fact]
    public void SetComment_TrimsAndNormalizesWhitespace()
    {
        var report = BlankReport.Create(1, "تقرير فارغ", "  ملاحظة  ", null);

        Assert.Equal("ملاحظة", report.Comment);

        report.SetComment("   ");
        Assert.Null(report.Comment);
    }
}
