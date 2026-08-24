using FluentAssertions;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddCultureTest;
using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class AddCultureTestCommandHandlerTests
{
    private readonly Mock<IRepository<Test>> _testRepository = new();
    private readonly Mock<IReferralEntityRepository> _referralEntityRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICultureTemplateSeeder> _cultureTemplateSeeder = new();

    private AddCultureTestCommandHandler CreateHandler()
        => new(
            _testRepository.Object,
            _referralEntityRepository.Object,
            _unitOfWork.Object,
            _cultureTemplateSeeder.Object);

    private static AddCultureTestCommand CreateCommand(
        string name = "Urine Culture",
        string group = CultureGroup.Name,
        string unit = "Result") => new(
        Name: name,
        ReportName: "Urine Culture Report",
        ReceiptName: "Urine Culture",
        Group: group,
        Barcode: null,
        Price: 100m,
        TurnaroundTime: "48h",
        LabToLabFlag: false,
        Unit: unit,
        TestCode: null,
        HistoryName: null,
        ArabicName: null,
        Branch: null,
        LogGroup: null,
        SampleType: "Urine",
        SeeReport: true,
        PrintWithOther: false,
        AddWithGroup: false,
        IsMainTest: true,
        TestTimeDays: 2,
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
    public async Task Handle_persists_a_culture_test_with_the_canonical_group()
    {
        Test? captured = null;
        _testRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((test, _) => captured = test)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(group: "CULTURE AND SENSITIVITY"), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Group.Should().Be(CultureGroup.Name);
        captured.Id.Should().Be(0);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_adds_one_ordinary_component_using_name_and_unit()
    {
        Test? captured = null;
        _testRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Callback<Test, CancellationToken>((test, _) => captured = test)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(name: "Blood Culture", unit: "Result"), CancellationToken.None);

        captured!.TestComponents.Should().ContainSingle();
        captured.TestComponents.Single().Name.Should().Be("Blood Culture");
        captured.TestComponents.Single().Unit.Should().Be("Result");
        captured.TestComponents.Single().DisplayOrder.Should().Be(1);
        captured.TestComponents.Single().ResultEntryKind.Should().Be(ResultEntryKind.Ordinary);
    }

    [Fact]
    public async Task Handle_invokes_template_seeder_once_after_save()
    {
        _testRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _cultureTemplateSeeder
            .Setup(seeder => seeder.SeedFromTemplateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        _cultureTemplateSeeder.Verify(
            seeder => seeder.SeedFromTemplateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
