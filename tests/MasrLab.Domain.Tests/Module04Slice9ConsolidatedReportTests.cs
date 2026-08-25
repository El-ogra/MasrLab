using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 9 — M4-BR-10/11 composition invariants and reorder mechanics.
public class Module04Slice9ConsolidatedReportTests
{
    [Fact]
    public void AddItem_RejectsDuplicateVisitTests()
    {
        var report = ConsolidatedReport.Create(1);
        report.AddItem(10);

        Assert.Throws<BusinessRuleViolationException>(() => report.AddItem(10));
    }

    [Fact]
    public void Items_KeepUserOrderingWithDenseDisplayOrders()
    {
        var report = ConsolidatedReport.Create(1);
        report.AddItem(10);
        report.AddItem(20);
        report.AddItem(30);

        Assert.Equal(new[] { 1, 2, 3 }, report.Items.Select(i => i.DisplayOrder));

        report.MoveUp(30);

        Assert.Equal(new[] { 10, 30, 20 }, report.Items.Select(i => i.VisitTestId));
        Assert.Equal(new[] { 1, 2, 3 }, report.Items.Select(i => i.DisplayOrder)); // stays dense.
    }

    [Fact]
    public void MoveDown_SwapsAndStaysDense()
    {
        var report = ConsolidatedReport.Create(1);
        report.AddItem(10);
        report.AddItem(20);
        report.AddItem(30);

        report.MoveDown(10);

        Assert.Equal(new[] { 20, 10, 30 }, report.Items.Select(i => i.VisitTestId));
    }

    [Fact]
    public void MovingPastTheEdges_IsANoop()
    {
        var report = ConsolidatedReport.Create(1);
        report.AddItem(10);
        report.AddItem(20);

        report.MoveUp(10);
        report.MoveDown(20);

        Assert.Equal(new[] { 10, 20 }, report.Items.Select(i => i.VisitTestId));
    }

    [Fact]
    public void RemoveItem_RenumbersRemainingRows()
    {
        var report = ConsolidatedReport.Create(1);
        report.AddItem(10);
        report.AddItem(20);
        report.AddItem(30);

        report.RemoveItem(10);

        Assert.Equal(new[] { 20, 30 }, report.Items.Select(i => i.VisitTestId));
        Assert.Equal(new[] { 1, 2 }, report.Items.Select(i => i.DisplayOrder));
    }

    [Fact]
    public void RemoveItem_WhenMissing_ShouldThrowBusinessRuleViolation()
    {
        var report = ConsolidatedReport.Create(1);
        Assert.Throws<BusinessRuleViolationException>(() => report.RemoveItem(99));
    }

    [Fact]
    public void SubtitleToggle_AndComment_AreStored()
    {
        var report = ConsolidatedReport.Create(1, printGroupSubtitles: false);
        Assert.False(report.PrintGroupSubtitles);

        report.SetPrintGroupSubtitles(true);
        report.SetComment("  ملاحظة  ");

        Assert.True(report.PrintGroupSubtitles);
        Assert.Equal("ملاحظة", report.Comment);
    }
}
