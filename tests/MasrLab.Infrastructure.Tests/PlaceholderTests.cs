using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class GenericRepositoryTests
{
    private MasrLabDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        using var context = CreateInMemoryContext();
        var repo = new GenericRepository<Patient>(context);

        var patient = new Patient { Name = "Test Patient", LabId = "LAB-001", DoctorId = 1, ReferralEntityId = 1 };
        await repo.AddAsync(patient);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetByIdAsync(patient.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Patient", result!.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        using var context = CreateInMemoryContext();
        var repo = new GenericRepository<Patient>(context);

        await repo.AddAsync(new Patient { Name = "Patient 1", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "Patient 2", LabId = "L2", DoctorId = 1, ReferralEntityId = 1 });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetAllAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = CreateInMemoryContext();
        var repo = new GenericRepository<Patient>(context);

        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        using var context = CreateInMemoryContext();
        var repo = new GenericRepository<Patient>(context);

        var patient = new Patient { Name = "Original", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 };
        await repo.AddAsync(patient);
        await context.SaveChangesAsync(CancellationToken.None);

        patient.Name = "Updated";
        repo.Update(patient);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetByIdAsync(patient.Id);
        Assert.Equal("Updated", result!.Name);
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity()
    {
        using var context = CreateInMemoryContext();
        var repo = new GenericRepository<Patient>(context);

        var patient = new Patient { Name = "To Delete", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 };
        await repo.AddAsync(patient);
        await context.SaveChangesAsync(CancellationToken.None);

        repo.Delete(patient);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetByIdAsync(patient.Id);
        Assert.Null(result);
    }
}

public class UnitOfWorkTests
{
    private MasrLabDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);

        var patient = new Patient { Name = "Test", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 };
        context.Patients.Add(patient);

        var result = await unitOfWork.SaveChangesAsync(CancellationToken.None);
        Assert.True(result >= 1);
    }
}

public class PatientRepositoryTests
{
    private MasrLabDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingPatients()
    {
        using var context = CreateInMemoryContext();
        var repo = new PatientRepository(context);

        await repo.AddAsync(new Patient { Name = "Ahmed Ali", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "Sara Mohamed", LabId = "L2", DoctorId = 1, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "Ahmed Hassan", LabId = "L3", DoctorId = 1, ReferralEntityId = 1 });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.SearchByNameAsync("Ahmed");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByLabIdAsync_ShouldReturnCorrectPatient()
    {
        using var context = CreateInMemoryContext();
        var repo = new PatientRepository(context);

        await repo.AddAsync(new Patient { Name = "Test", LabId = "LAB-001", DoctorId = 1, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "Test2", LabId = "LAB-002", DoctorId = 1, ReferralEntityId = 1 });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetByLabIdAsync("LAB-001");
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
    }

    [Fact]
    public async Task GetByDoctorIdAsync_ShouldReturnCorrectPatients()
    {
        using var context = CreateInMemoryContext();
        var repo = new PatientRepository(context);

        await repo.AddAsync(new Patient { Name = "P1", LabId = "L1", DoctorId = 1, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "P2", LabId = "L2", DoctorId = 2, ReferralEntityId = 1 });
        await repo.AddAsync(new Patient { Name = "P3", LabId = "L3", DoctorId = 1, ReferralEntityId = 1 });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await repo.GetByDoctorIdAsync(1);
        Assert.Equal(2, result.Count);
    }
}
