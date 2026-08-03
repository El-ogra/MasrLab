using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Tests;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldHaveDefaultValues()
    {
        var patient = new Patient();

        Assert.Equal(0, patient.Id);
        Assert.Equal(default(DateTime), patient.CreatedAt);
        Assert.Equal(0, patient.CreatedByUserId);
        Assert.Null(patient.UpdatedAt);
        Assert.Null(patient.UpdatedByUserId);
        Assert.False(patient.IsDeleted);
    }

    [Fact]
    public void BaseEntity_ShouldImplementIAuditableEntity()
    {
        var patient = new Patient();
        Assert.IsAssignableFrom<IAuditableEntity>(patient);
    }

    [Fact]
    public void BaseEntity_ShouldImplementISoftDeletable()
    {
        var patient = new Patient();
        Assert.IsAssignableFrom<ISoftDeletable>(patient);
    }

    [Fact]
    public void Patient_CanSetAllProperties()
    {
        var patient = new Patient
        {
            Name = "Ahmed",
            Age = new Age(30, 6, 15),
            Gender = Gender.Male,
            Phone = new EgyptianPhone("01234567890"),
            Address = "Cairo",
            NationalId = "12345678901234",
            LabId = "LAB-001",
            DoctorId = 1,
            ReferralEntityId = 1,
            AccountType = AccountType.Cash,
            IsDeleted = false
        };

        Assert.Equal("Ahmed", patient.Name);
        Assert.Equal(30, patient.Age.Years);
        Assert.Equal(Gender.Male, patient.Gender);
        Assert.Equal("LAB-001", patient.LabId);
        Assert.Equal(AccountType.Cash, patient.AccountType);
    }
}

public class EnumTests
{
    [Fact]
    public void VisitStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal(0, (int)VisitStatus.Registered);
        Assert.Equal(1, (int)VisitStatus.ResultsEntered);
        Assert.Equal(2, (int)VisitStatus.Printed);
    }

    [Fact]
    public void Gender_ShouldHaveExpectedValues()
    {
        Assert.Equal(0, (int)Gender.Male);
        Assert.Equal(1, (int)Gender.Female);
    }

    [Fact]
    public void AccountType_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<AccountType>();
        Assert.True(values.Length > 0);
    }
}

public class EntityTests
{
    [Fact]
    public void User_CanCreate()
    {
        var user = new User
        {
            Username = "admin",
            Password = "pass123",
            IsAdmin = true,
            IsActive = true
        };

        Assert.Equal("admin", user.Username);
        Assert.True(user.IsAdmin);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Test_CanCreate()
    {
        var test = new Test
        {
            Name = "CBC",
            ReportName = "Complete Blood Count",
            ReceiptName = "CBC Receipt",
            Group = "Hematology",
            Price = 150m,
            TurnaroundTime = "2h",
            Unit = "cells/uL"
        };

        Assert.Equal("CBC", test.Name);
        Assert.Equal(150m, test.Price);
    }

    [Fact]
    public void Doctor_CanCreate()
    {
        var doctor = new Doctor
        {
            Name = "Dr. Smith",
            CommissionPercent = 10m
        };

        Assert.Equal("Dr. Smith", doctor.Name);
        Assert.Equal(10m, doctor.CommissionPercent);
    }
}
