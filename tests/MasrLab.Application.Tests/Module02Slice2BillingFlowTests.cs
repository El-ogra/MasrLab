using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientVisits.Commands.IssueReceipt;
using MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using FluentValidation;
using MediatR;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 2 — dual discount issuance (OQ-M2-3) and the single "المدفوع سابقا" entry point (OQ-M2-2).
public class Module02Slice2BillingFlowTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void IssueReceiptValidator_rejects_percent_outside_0_to_100(decimal percent)
    {
        var validator = new IssueReceiptCommandValidator();
        var result = validator.Validate(new IssueReceiptCommand(1, 0, 0, 7, null, percent));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueReceiptCommand.DiscountPercent));
    }

    [Fact]
    public void IssueReceiptValidator_accepts_percent_bounds_and_non_negative_values()
    {
        var validator = new IssueReceiptCommandValidator();
        Assert.True(validator.Validate(new IssueReceiptCommand(1, 0, 0, 7, null, 0)).IsValid);
        Assert.True(validator.Validate(new IssueReceiptCommand(1, 0, 0, 7, null, 100)).IsValid);
        Assert.True(validator.Validate(new IssueReceiptCommand(1, 30, 50, 7, null, 10)).IsValid);
        Assert.False(validator.Validate(new IssueReceiptCommand(1, -1, 0, 7, null, 0)).IsValid);
        Assert.False(validator.Validate(new IssueReceiptCommand(1, 0, -5, 7, null, 0)).IsValid);
    }

    [Fact]
    public async Task IssueReceiptHandler_applies_percent_and_absolute_discounts()
    {
        var visit = PatientVisit.Create(3, 7, "L1", null, null);
        visit.AddVisitTest(new VisitTest(visit.Id, 1, 100m, false) { TestNameSnapshot = "A", ReportNameSnapshot = "A", ReceiptNameSnapshot = "A" });
        visit.AddVisitTest(new VisitTest(visit.Id, 2, 100m, false) { TestNameSnapshot = "B", ReportNameSnapshot = "B", ReceiptNameSnapshot = "B" });

        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetByIdWithTestsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        visits.Setup(x => x.GetOpenReceiptAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((Receipt?)null);

        Receipt? saved = null;
        var receipts = new Mock<IRepository<Receipt>>();
        receipts.Setup(x => x.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
            .Callback<Receipt, CancellationToken>((r, _) => saved = r);

        var handler = new IssueReceiptCommandHandler(
            visits.Object,
            receipts.Object,
            new Mock<IRepository<CashTransaction>>().Object,
            new PricingService(),
            new ReceiptCalculationService(),
            new Mock<IUnitOfWork>().Object);

        await handler.Handle(new IssueReceiptCommand(3, Discount: 30, PaidNow: 0, ReceivedByUserId: 7, CashAccountId: null, DiscountPercent: 10), default);

        Assert.NotNull(saved!);
        Assert.Equal(10m, saved!.DiscountPercent);
        Assert.Equal(50m, saved.Discount); // % first (20), then absolute (30).
        Assert.Equal(150m, saved.TotalAfterDiscount);
    }

    [Fact]
    public async Task Intake_PaidPrevious_pipes_into_exactly_one_initial_payment_OQ_M2_2()
    {
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<RegisterPatientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>(), 11));
        sender.Setup(x => x.Send(It.IsAny<CreatePatientVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(55);
        sender.Setup(x => x.Send(It.IsAny<AddTestsToVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        sender.Setup(x => x.Send(It.IsAny<GetVisitTestCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        IssueReceiptCommand? issued = null;
        sender.Setup(x => x.Send(It.Is<IssueReceiptCommand>(c => c.PaidNow > 0), It.IsAny<CancellationToken>()))
            .Returns<IssueReceiptCommand, CancellationToken>((c, _) =>
            {
                issued = c;
                return Task.FromResult(777);
            });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<RegisterPatientIntakeResult>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<RegisterPatientIntakeResult>>, CancellationToken>(
                (work, ct) => work(ct));

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(9);

        var handler = new RegisterPatientIntakeCommandHandler(sender.Object, unitOfWork.Object, currentUser.Object);

        var command = MinimalIntakeCommand with { PaidPrevious = 120m };
        var result = await handler.Handle(command, default);

        Assert.Equal(777, result.ReceiptId);
        Assert.NotNull(issued!);
        // ONE payment entry point: the advance payment is the receipt's single initial payment.
        Assert.Equal(55, issued!.PatientVisitId);
        Assert.Equal(120m, issued.PaidNow);
        Assert.Equal(9, issued.ReceivedByUserId);
        Assert.Equal(0m, issued.Discount);
        sender.Verify(x => x.Send(It.IsAny<IssueReceiptCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Intake_without_advance_payment_issues_no_receipt()
    {
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<RegisterPatientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>(), 11));
        sender.Setup(x => x.Send(It.IsAny<CreatePatientVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(55);
        sender.Setup(x => x.Send(It.IsAny<AddTestsToVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        sender.Setup(x => x.Send(It.IsAny<GetVisitTestCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<RegisterPatientIntakeResult>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<RegisterPatientIntakeResult>>, CancellationToken>(
                (work, ct) => work(ct));

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(9);

        var handler = new RegisterPatientIntakeCommandHandler(sender.Object, unitOfWork.Object, currentUser.Object);

        var result = await handler.Handle(MinimalIntakeCommand with { PaidPrevious = 0 }, default);

        Assert.Null(result.ReceiptId);
        sender.Verify(x => x.Send(It.IsAny<IssueReceiptCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static readonly RegisterPatientIntakeCommand MinimalIntakeCommand = new(
        Name: "Test Patient",
        AgeYears: 30,
        AgeMonths: 0,
        AgeDays: 0,
        AgeUnit: AgeUnit.Years,
        Gender: Gender.Male,
        Phone: null,
        Address: null,
        NationalId: null,
        Notes: null,
        LabId: "L-1",
        DoctorId: null,
        ReferralEntityId: null);
}
