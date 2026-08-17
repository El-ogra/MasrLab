// TODO: Section 5 of Docs/Test_for_domain.md documents deliberate domain gaps that are
// intentionally NOT covered by tests here (unraised events such as PatientRegistered and
// PatientUpdated, unimplemented invariants such as INV-01/INV-02, and partial state machines).

using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class VisitTestTests
{
    [Fact]
    public void PriceSetter_WhenValueNegative_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, -1m, false)));

        Assert.Equal("Price cannot be negative.", ex.Message);
    }

    [Fact]
    public void PriceSetter_WhenValueZero_ShouldAcceptZero()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 0m, false));

        Assert.Equal(0m, Assert.Single(visit.VisitTests).Price);
    }

    [Fact]
    public void PriceSetter_WhenValuePositive_ShouldStoreValue()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 250m, false));

        Assert.Equal(250m, Assert.Single(visit.VisitTests).Price);
    }
}

public class CashTransactionTests
{
    [Fact]
    public void AmountSetter_WhenZero_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new CashTransaction { Amount = 0m });

        Assert.Equal("CashTransaction amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void AmountSetter_WhenNegative_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new CashTransaction { Amount = -50m });

        Assert.Equal("CashTransaction amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void AmountSetter_WhenPositive_ShouldStoreValue()
    {
        var transaction = new CashTransaction { Amount = 100m };

        Assert.Equal(100m, transaction.Amount);
    }
}

public class CommentTests
{
    [Fact]
    public void CommentTextSetter_WhenNull_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new Comment { CommentText = null! });

        Assert.Equal("Comment text cannot be empty.", ex.Message);
    }

    [Fact]
    public void CommentTextSetter_WhenWhitespace_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new Comment { CommentText = "   " });

        Assert.Equal("Comment text cannot be empty.", ex.Message);
    }

    [Fact]
    public void CommentTextSetter_WhenExactly1000Chars_ShouldAccept()
    {
        var comment = new Comment { CommentText = new string('a', 1000) };

        Assert.Equal(1000, comment.CommentText.Length);
    }

    [Fact]
    public void CommentTextSetter_WhenExceeds1000Chars_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Comment { CommentText = new string('a', 1001) });

        Assert.Equal("Comment text cannot exceed 1000 characters.", ex.Message);
    }

    [Fact]
    public void CommentTextSetter_WhenValid_ShouldStoreValue()
    {
        var comment = new Comment { CommentText = "Elevated" };

        Assert.Equal("Elevated", comment.CommentText);
    }
}

public class CommentTemplateTests
{
    [Fact]
    public void TextSetter_WhenNull_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new CommentTemplate { Text = null! });

        Assert.Equal("Comment template text cannot be empty.", ex.Message);
    }

    [Fact]
    public void TextSetter_WhenWhitespace_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new CommentTemplate { Text = "   " });

        Assert.Equal("Comment template text cannot be empty.", ex.Message);
    }

    [Fact]
    public void TextSetter_WhenExactly1000Chars_ShouldAccept()
    {
        var template = new CommentTemplate { Text = new string('a', 1000) };

        Assert.Equal(1000, template.Text.Length);
    }

    [Fact]
    public void TextSetter_WhenExceeds1000Chars_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new CommentTemplate { Text = new string('a', 1001) });

        Assert.Equal("Comment template text cannot exceed 1000 characters.", ex.Message);
    }
}

public class DoctorTests
{
    [Fact]
    public void CommissionPercentSetter_WhenNegative_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new Doctor { CommissionPercent = -1m });

        Assert.Equal("CommissionPercent must be between 0 and 100.", ex.Message);
    }

    [Fact]
    public void CommissionPercentSetter_WhenGreaterThan100_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => new Doctor { CommissionPercent = 100.01m });

        Assert.Equal("CommissionPercent must be between 0 and 100.", ex.Message);
    }

    [Fact]
    public void CommissionPercentSetter_WhenZero_ShouldAccept()
    {
        var doctor = new Doctor { CommissionPercent = 0m };

        Assert.Equal(0m, doctor.CommissionPercent);
    }

    [Fact]
    public void CommissionPercentSetter_WhenExactly100_ShouldAccept()
    {
        var doctor = new Doctor { CommissionPercent = 100m };

        Assert.Equal(100m, doctor.CommissionPercent);
    }

    [Fact]
    public void CommissionPercentSetter_WhenValidMid_ShouldStoreValue()
    {
        var doctor = new Doctor { CommissionPercent = 25m };

        Assert.Equal(25m, doctor.CommissionPercent);
    }
}

public class SensitivityTests
{
    [Fact]
    public void Constructor_WhenValidIds_ShouldCreateInstance()
    {
        var sensitivity = new Sensitivity(1, 5, SensitivityLevel.HighlySensitive);

        Assert.Equal(1, sensitivity.CultureId);
        Assert.Equal(5, sensitivity.AntibioticId);
        Assert.Equal(SensitivityLevel.HighlySensitive, sensitivity.SensitivityLevel);
    }

    [Fact]
    public void Constructor_WhenCultureIdZero_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Sensitivity(0, 5, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid CultureId.", ex.Message);
    }

    [Fact]
    public void Constructor_WhenCultureIdNegative_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Sensitivity(-3, 5, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid CultureId.", ex.Message);
    }

    [Fact]
    public void Constructor_WhenAntibioticIdZero_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Sensitivity(1, 0, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid AntibioticId.", ex.Message);
    }

    [Fact]
    public void Constructor_WhenAntibioticIdNegative_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => new Sensitivity(1, -1, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid AntibioticId.", ex.Message);
    }
}

public class ReceiptPaymentAndDiscountTests
{
    private static Receipt CreateReceipt(decimal total)
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, total, false));
        receipt.Issue();
        return receipt;
    }

    [Fact]
    public void AddPayment_WhenAmountZero_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateReceipt(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(0m));

        Assert.Equal("Payment amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void AddPayment_WhenAmountNegative_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateReceipt(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(-10m));

        Assert.Equal("Payment amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void AddPayment_WhenExceedsRemaining_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateReceipt(100m);
        receipt.AddPayment(60m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(50m));

        Assert.Equal("Total payment cannot exceed receipt total.", ex.Message);
    }

    [Fact]
    public void AddPayment_WhenExactRemaining_ShouldAcceptAndSetRemainingZero()
    {
        var receipt = CreateReceipt(100m);
        receipt.AddPayment(40m);

        receipt.AddPayment(60m);

        Assert.Equal(100m, receipt.PaidNow);
        Assert.Equal(0m, receipt.Remaining);
    }

    [Fact]
    public void AddPayment_WhenPartial_ShouldUpdatePaidAndRemaining()
    {
        var receipt = CreateReceipt(100m);

        receipt.AddPayment(30m);

        Assert.Equal(30m, receipt.PaidNow);
        Assert.Equal(70m, receipt.Remaining);
    }

    [Fact]
    public void ApplyDiscount_WhenNegative_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateReceipt(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.ApplyDiscount(-1m));

        Assert.Equal("Discount cannot be negative.", ex.Message);
    }

    [Fact]
    public void ApplyDiscount_WhenExceedsTotal_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateReceipt(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.ApplyDiscount(150m));

        Assert.Equal("Discount cannot exceed receipt total.", ex.Message);
    }

    [Fact]
    public void ApplyDiscount_WhenZero_ShouldAccept()
    {
        var receipt = CreateReceipt(100m);

        receipt.ApplyDiscount(0m);

        Assert.Equal(0m, receipt.Discount);
    }

    [Fact]
    public void ApplyDiscount_WhenEqualsTotal_ShouldAccept()
    {
        var receipt = CreateReceipt(100m);

        receipt.ApplyDiscount(100m);

        Assert.Equal(100m, receipt.Discount);
        Assert.Equal(0m, receipt.Remaining);
    }

    [Fact]
    public void ApplyDiscount_WhenValidAndPaidNowSet_ShouldClampRemainingAtZero()
    {
        var receipt = CreateReceipt(100m);
        receipt.AddPayment(80m);

        receipt.ApplyDiscount(30m);

        Assert.Equal(0m, receipt.Remaining);
    }
}

public class OutsourcedSampleSetPricesTests
{
    [Fact]
    public void SetPrices_WhenPatientPriceLessThanCost_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.SetPrices(80m, 100m));

        Assert.Equal("Patient price must be greater than or equal to cost price.", ex.Message);
    }

    [Fact]
    public void SetPrices_WhenEqual_ShouldAccept()
    {
        var sample = new OutsourcedSample();

        sample.SetPrices(100m, 100m);

        Assert.Equal(100m, sample.PatientPrice);
        Assert.Equal(100m, sample.CostPrice);
    }

    [Fact]
    public void SetPrices_WhenPatientPriceGreater_ShouldStoreBoth()
    {
        var sample = new OutsourcedSample();

        sample.SetPrices(150m, 100m);

        Assert.Equal(150m, sample.PatientPrice);
        Assert.Equal(100m, sample.CostPrice);
    }
}
