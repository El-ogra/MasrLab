using MasrLab.Application.Common.Behaviors;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace MasrLab.Application.Tests;

public class AuditBehaviorTests
{
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IDateTimeService> _dateTimeService;
    private readonly Mock<IRequestAuditLogRepository> _auditLogRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AuditBehavior<EnterTestResultCommand, Unit>>> _logger;
    private RequestAuditLog? _savedEntry;

    private static readonly DateTime FixedUtcNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public AuditBehaviorTests()
    {
        _currentUserService = new Mock<ICurrentUserService>();
        _currentUserService.Setup(s => s.UserId).Returns(42);

        _dateTimeService = new Mock<IDateTimeService>();
        _dateTimeService.Setup(s => s.UtcNow).Returns(FixedUtcNow);

        _auditLogRepository = new Mock<IRequestAuditLogRepository>();
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<RequestAuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<RequestAuditLog, CancellationToken>((entry, _) => _savedEntry = entry);

        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<AuditBehavior<EnterTestResultCommand, Unit>>>();
    }

    private AuditBehavior<EnterTestResultCommand, Unit> CreateBehavior()
        => new(
            _currentUserService.Object,
            _dateTimeService.Object,
            _auditLogRepository.Object,
            _unitOfWork.Object,
            _logger.Object);

    private static EnterTestResultCommand CreateCommand()
        => new(
            VisitTestId: 100,
            Value: "5.0",
            Unit: "cells/uL",
            ReferenceRange: "1-10",
            Status: ResultStatus.Normal,
            EnteredByUserId: 1,
            PatientId: 1,
            AgeYears: 30,
            OverrideReason: null);

    [Fact]
    public async Task Handle_WhenHandlerSucceeds_PersistsSuccessAuditEntry()
    {
        var behavior = CreateBehavior();
        RequestHandlerDelegate<Unit> next = _ => Task.FromResult(Unit.Value);

        var result = await behavior.Handle(CreateCommand(), next, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        Assert.NotNull(_savedEntry);
        Assert.Equal("EnterTestResultCommand", _savedEntry!.RequestName);
        Assert.Equal(42, _savedEntry.UserId);
        Assert.Equal(FixedUtcNow, _savedEntry.ActionTimeUtc);
        Assert.True(_savedEntry.Success);
        Assert.Null(_savedEntry.ErrorMessage);
        _auditLogRepository.Verify(r => r.AddAsync(It.IsAny<RequestAuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_PersistsFailedAuditEntryAndRethrowsOriginal()
    {
        var behavior = CreateBehavior();
        var expected = new InvalidOperationException("handler exploded");
        RequestHandlerDelegate<Unit> next = _ => throw expected;

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(CreateCommand(), next, CancellationToken.None));

        Assert.Same(expected, thrown);
        Assert.NotNull(_savedEntry);
        Assert.Equal("EnterTestResultCommand", _savedEntry!.RequestName);
        Assert.False(_savedEntry.Success);
        Assert.Equal("handler exploded", _savedEntry.ErrorMessage);
        _auditLogRepository.Verify(r => r.AddAsync(It.IsAny<RequestAuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuditPersistenceFails_DoesNotAbortRequestAndLogsWarning()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var behavior = CreateBehavior();
        var nextCalled = false;
        RequestHandlerDelegate<Unit> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        };

        var result = await behavior.Handle(CreateCommand(), next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal(Unit.Value, result);
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("EnterTestResultCommand")),
                It.Is<Exception>(ex => ex.Message == "db down"),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
