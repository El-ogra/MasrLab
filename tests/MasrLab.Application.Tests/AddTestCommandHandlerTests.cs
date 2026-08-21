using FluentAssertions;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class AddTestCommandHandlerTests
{
    private readonly Mock<IRepository<Test>> _testRepo;
    private readonly Mock<IReferralEntityRepository> _referralEntityRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public AddTestCommandHandlerTests()
    {
        _testRepo = new Mock<IRepository<Test>>();
        _referralEntityRepo = new Mock<IReferralEntityRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private AddTestCommandHandler CreateHandler()
        => new(_testRepo.Object, _referralEntityRepo.Object, _unitOfWork.Object);

    private AddTestCommand CreateCommand(
        string name = "CBC",
        string unit = "g/dL")
        => new(
            Name: name,
            ReportName: "Report",
            ReceiptName: "Receipt",
            Group: "Hematology",
            Barcode: null,
            Price: 50m,
            TurnaroundTime: "24h",
            LabToLabFlag: false,
            Unit: unit,
            TestCode: null,
            HistoryName: null,
            ArabicName: null,
            Branch: null,
            LogGroup: null,
            SampleType: null,
            SeeReport: true,
            PrintWithOther: false,
            AddWithGroup: false,
            IsMainTest: true,
            TestTimeDays: 1,
            ArrangeNo: 1,
            ReferenceType: ReferenceType.General,
            LabToLabPrice: null,
            BarcodeName: null,
            Tube1: null,
            Tube2: null,
            Tube3: null,
            SentOutsideLab: false,
            OutsourcedLabName: null,
            OutsourcedCostPrice: null,
            PatientQuestion: null);

    [Fact]
    public async Task Handle_AddsExactlyOneTestComponent()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TestComponents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ComponentNameEqualsRequestName()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(name: "Lipid Profile"), CancellationToken.None);

        captured!.TestComponents.Single().Name.Should().Be("Lipid Profile");
    }

    [Fact]
    public async Task Handle_ComponentUnitEqualsRequestUnit()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(unit: "mg/dL"), CancellationToken.None);

        captured!.TestComponents.Single().Unit.Should().Be("mg/dL");
    }

    [Fact]
    public async Task Handle_ComponentDisplayOrderIsOne()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        captured!.TestComponents.Single().DisplayOrder.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ComponentResultEntryKindIsOrdinary()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        captured!.TestComponents.Single().ResultEntryKind.Should().Be(ResultEntryKind.Ordinary);
    }

    [Fact]
    public async Task Handle_Persists_CostPrice_When_Supplied()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand() with { CostPrice = 25.50m }, CancellationToken.None);

        captured!.CostPrice.Should().Be(25.50m);
    }

    [Fact]
    public async Task Handle_Preserves_Null_CostPrice_When_Not_Supplied()
    {
        Test? captured = null;
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        captured!.CostPrice.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsSaveChangesAsync()
    {
        _testRepo
            .Setup(r => r.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
