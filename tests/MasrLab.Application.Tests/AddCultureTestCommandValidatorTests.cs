using FluentAssertions;
using FluentValidation.TestHelper;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddCultureTest;
using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class AddCultureTestCommandValidatorTests
{
    private readonly AddCultureTestCommandValidator _validator = new();

    private static AddCultureTestCommand CreateCommand() => new(
        Name: "Urine Culture",
        ReportName: "Urine Culture Report",
        ReceiptName: "Urine Culture",
        Group: CultureGroup.Name,
        Barcode: null,
        Price: 100m,
        TurnaroundTime: "48h",
        LabToLabFlag: false,
        Unit: "Result",
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
    public void Valid_culture_command_passes()
    {
        var result = _validator.TestValidate(CreateCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Wrong_group_is_rejected()
    {
        var result = _validator.TestValidate(CreateCommand() with { Group = "Hematology" });

        result.ShouldHaveValidationErrorFor(command => command.Group);
    }

    [Fact]
    public void Empty_required_fields_are_rejected()
    {
        var result = _validator.TestValidate(CreateCommand() with
        {
            Name = "",
            ReportName = "",
            ReceiptName = "",
            TurnaroundTime = "",
            Unit = ""
        });

        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.ReportName);
        result.ShouldHaveValidationErrorFor(command => command.ReceiptName);
        result.ShouldHaveValidationErrorFor(command => command.TurnaroundTime);
        result.ShouldHaveValidationErrorFor(command => command.Unit);
    }

    [Fact]
    public void Negative_numeric_fields_are_rejected()
    {
        var result = _validator.TestValidate(CreateCommand() with
        {
            Price = -1,
            TestTimeDays = -1,
            ArrangeNo = -1,
            LabToLabPrice = -1,
            OutsourcedCostPrice = -1,
            CostPrice = -1
        });

        result.ShouldHaveValidationErrorFor(command => command.Price);
        result.ShouldHaveValidationErrorFor(command => command.TestTimeDays);
        result.ShouldHaveValidationErrorFor(command => command.ArrangeNo);
        result.ShouldHaveValidationErrorFor(command => command.LabToLabPrice);
        result.ShouldHaveValidationErrorFor(command => command.OutsourcedCostPrice);
        result.ShouldHaveValidationErrorFor(command => command.CostPrice);
    }

    [Fact]
    public void Outsourcing_requires_lab_and_cost()
    {
        var result = _validator.TestValidate(CreateCommand() with { SentOutsideLab = true });

        result.ShouldHaveValidationErrorFor(command => command.OutsourcedLabReferralEntityId);
        result.ShouldHaveValidationErrorFor(command => command.OutsourcedCostPrice);
    }

    [Fact]
    public void Non_outsourcing_rejects_lab_and_cost_values()
    {
        var result = _validator.TestValidate(CreateCommand() with
        {
            SentOutsideLab = false,
            OutsourcedLabReferralEntityId = 7,
            OutsourcedCostPrice = 10m
        });

        result.ShouldHaveValidationErrorFor(command => command.OutsourcedLabReferralEntityId);
        result.ShouldHaveValidationErrorFor(command => command.OutsourcedCostPrice);
    }

    [Fact]
    public void Valid_outsourcing_values_pass()
    {
        var result = _validator.TestValidate(CreateCommand() with
        {
            SentOutsideLab = true,
            OutsourcedLabReferralEntityId = 7,
            OutsourcedCostPrice = 10m
        });

        result.IsValid.Should().BeTrue();
    }
}
