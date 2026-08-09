using MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;
using MasrLab.Domain.Common.DTOs;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPatientHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNullableValuesAreNull_ReturnsEmptyStrings()
    {
        var service = new Mock<IMedicalHistoryService>();
        service
            .Setup(s => s.BuildHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientHistoryEntry>
            {
                new()
                {
                    PatientId = 1,
                    LabId = "LAB-001",
                    TestId = 10,
                    TestName = "CBC",
                    TestReportName = "Complete Blood Count",
                    PreviousValue = null,
                    PreviousUnit = null,
                    PreviousReferenceRange = null,
                    PreviousStatus = "Normal",
                    PreviousVisitDate = new DateTime(2026, 1, 1),
                    CurrentValue = null,
                    CurrentUnit = null,
                    CurrentReferenceRange = null,
                    CurrentStatus = "High",
                    CurrentVisitDate = new DateTime(2026, 2, 1),
                    ComparisonFlag = true
                }
            });

        var handler = new GetPatientHistoryQueryHandler(service.Object);

        var result = await handler.Handle(new GetPatientHistoryQuery(1), CancellationToken.None);

        var entry = Assert.Single(result);
        Assert.Equal(string.Empty, entry.PreviousValue);
        Assert.Equal(string.Empty, entry.PreviousUnit);
        Assert.Equal(string.Empty, entry.PreviousReferenceRange);
        Assert.Equal(string.Empty, entry.CurrentValue);
        Assert.Equal(string.Empty, entry.CurrentUnit);
        Assert.Equal(string.Empty, entry.CurrentReferenceRange);
        Assert.True(entry.ComparisonFlag);
        Assert.Equal("LAB-001", entry.LabId);
    }
}
