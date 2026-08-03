using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class NewEventTests
{
    [Fact]
    public void Patient_Register_ShouldRaisePatientRegistered()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");

        var evt = Assert.Single(patient.DomainEvents.OfType<PatientRegistered>());
        Assert.Equal(patient.Id, evt.PatientId);
        Assert.Equal("Ahmed", evt.FullName);
    }

    [Fact]
    public void Patient_Register_WhenNameEmpty_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => Patient.Register("", "LAB-001"));

        Assert.Equal("Patient name cannot be empty.", ex.Message);
    }

    [Fact]
    public void Patient_UpdateProfile_ShouldRaisePatientUpdatedWithChangedFields()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");

        patient.UpdateProfile("Mohamed", "Cairo", null, null);

        var evt = Assert.Single(patient.DomainEvents.OfType<PatientUpdated>());
        Assert.Contains(nameof(Patient.Name), evt.ChangedFields);
        Assert.Contains(nameof(Patient.Address), evt.ChangedFields);
        Assert.Equal("Mohamed", patient.Name);
        Assert.Equal("Cairo", patient.Address);
    }

    [Fact]
    public void Patient_UpdateProfile_WhenNothingChanged_ShouldNotRaisePatientUpdated()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");

        patient.UpdateProfile("Ahmed", null, null, null);

        Assert.Empty(patient.DomainEvents.OfType<PatientUpdated>());
    }

    [Fact]
    public void PatientVisit_RemoveTest_ShouldRaiseVisitTestRemoved()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(10, 100m, false);

        visit.RemoveTest(10);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitTestRemoved>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(10, evt.TestId);
        Assert.Empty(visit.VisitTests);
    }

    [Fact]
    public void PatientVisit_RemoveTest_WhenTestNotFound_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(10, 100m, false);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.RemoveTest(99));

        Assert.Equal("Test is not part of this visit.", ex.Message);
    }

    [Fact]
    public void TestResult_Enter_ShouldRaiseTestResultEntered()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        var evt = Assert.Single(result.DomainEvents.OfType<TestResultEntered>());
        Assert.Equal(result.Id, evt.ResultId);
        Assert.Equal(5, evt.VisitTestId);
        Assert.Equal("12.5", evt.Value);
        Assert.Equal(7, evt.EnteredBy);
        Assert.Equal("12.5", result.Value);
    }

    [Fact]
    public void TestResult_Enter_WhenInvalid_ShouldThrowBusinessRuleViolation()
    {
        Assert.Throws<BusinessRuleViolationException>(() => TestResult.Enter(0, "x", 1));
        Assert.Throws<BusinessRuleViolationException>(() => TestResult.Enter(5, "   ", 1));
    }

    [Fact]
    public void TestResult_Edit_ShouldRaiseTestResultEditedWithOldAndNewValues()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.Edit("13.0", 9);

        var evt = Assert.Single(result.DomainEvents.OfType<TestResultEdited>());
        Assert.Equal(result.Id, evt.ResultId);
        Assert.Equal("12.5", evt.OldValue);
        Assert.Equal("13.0", evt.NewValue);
        Assert.Equal(9, evt.EditedBy);
        Assert.Equal("13.0", result.Value);
    }

    [Fact]
    public void TestResult_Edit_WhenNewValueEmpty_ShouldThrowBusinessRuleViolation()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => result.Edit("", 9));

        Assert.Equal("Test result value cannot be empty.", ex.Message);
    }

    [Fact]
    public void CashTransaction_Deposit_ShouldRaiseCashDeposited()
    {
        var transaction = CashTransaction.Deposit(500m, 1, 2);

        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(500m, transaction.Amount);

        var evt = Assert.Single(transaction.DomainEvents.OfType<CashDeposited>());
        Assert.Equal(transaction.Id, evt.TransactionId);
        Assert.Equal(500m, evt.Amount);
    }

    [Fact]
    public void CashTransaction_Withdraw_ShouldRaiseCashWithdrawn()
    {
        var transaction = CashTransaction.Withdraw(100m, 1, 2);

        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(100m, transaction.Amount);

        var evt = Assert.Single(transaction.DomainEvents.OfType<CashWithdrawn>());
        Assert.Equal(transaction.Id, evt.TransactionId);
        Assert.Equal(100m, evt.Amount);
    }

    [Fact]
    public void CashTransaction_Deposit_WhenAmountZero_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => CashTransaction.Deposit(0m, 1, 2));

        Assert.Equal("CashTransaction amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void Comment_AttachToResult_ShouldRaiseCommentAttachedToResult()
    {
        var comment = Comment.AttachToResult(5, "Repeated test");

        var evt = Assert.Single(comment.DomainEvents.OfType<CommentAttachedToResult>());
        Assert.Equal(comment.Id, evt.CommentId);
        Assert.Equal(5, evt.TestId);
        Assert.Equal("Repeated test", comment.CommentText);
    }

    [Fact]
    public void Comment_AttachToResult_WhenTestIdZero_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => Comment.AttachToResult(0, "x"));

        Assert.Equal("Comment requires a valid TestId.", ex.Message);
    }
}
