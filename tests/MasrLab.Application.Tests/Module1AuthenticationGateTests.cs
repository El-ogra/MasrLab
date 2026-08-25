using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;
using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;
using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;
using MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;
using MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;
using MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;
using Moq;

namespace MasrLab.Application.Tests;

public sealed class Module1AuthenticationGateTests
{
    private const string AuthenticationMessage = "Current user is not authenticated.";

    [Fact]
    public async Task RegisterPatient_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var patients = new Mock<IPatientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new TestCurrentUserService();
        patients.Setup(repository => repository.FindProbableDuplicatesAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Patient>());

        var handler = new RegisterPatientCommandHandler(
            patients.Object,
            unitOfWork.Object,
            new LabIdGenerator(patients.Object),
            currentUser);
        var command = new RegisterPatientCommand(
            "Patient One", 35, 0, 0, AgeUnit.Years, Gender.Female,
            null, null, null, null, "20260825-0001", null, null);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        patients.Setup(repository => repository.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()));
        await handler.Handle(command, CancellationToken.None);

        patients.Verify(repository => repository.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePatientData_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var patients = new Mock<IPatientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new TestCurrentUserService();
        patients.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = 1, Name = "Patient One" });

        var handler = new UpdatePatientDataCommandHandler(patients.Object, unitOfWork.Object, currentUser);
        var command = new UpdatePatientDataCommand(
            1, "Updated Patient", 35, 0, 0, AgeUnit.Years, Gender.Female,
            null, null, null, null, null, null);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        await handler.Handle(command, CancellationToken.None);

        patients.Verify(repository => repository.Update(It.IsAny<Patient>()), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPatientIntake_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var sender = new Mock<ISender>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new TestCurrentUserService();
        unitOfWork
            .Setup(work => work.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<RegisterPatientIntakeResult>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<RegisterPatientIntakeResult>> operation, CancellationToken ct)
                => operation(ct));
        sender.Setup(mock => mock.Send(It.IsAny<RegisterPatientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>(), 42));
        sender.Setup(mock => mock.Send(It.IsAny<CreatePatientVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        sender.Setup(mock => mock.Send(It.IsAny<GetVisitTestCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var handler = new RegisterPatientIntakeCommandHandler(sender.Object, unitOfWork.Object, currentUser);
        var command = new RegisterPatientIntakeCommand(
            "Patient One", 35, 0, 0, AgeUnit.Years, Gender.Female,
            null, null, null, null, "20260825-0001", null, null);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsRegistered);
        Assert.Equal(100, result.PatientVisitId);
        sender.Verify(mock => mock.Send(It.IsAny<RegisterPatientCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        sender.Verify(mock => mock.Send(It.IsAny<CreatePatientVisitCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePatientVisit_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var patients = new Mock<IPatientRepository>();
        var visits = new Mock<IVisitRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var labIdGenerator = new Mock<IVisitLabIdGenerator>();
        var currentUser = new TestCurrentUserService();
        patients.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = 1, Name = "Patient One" });
        labIdGenerator.Setup(generator => generator.GenerateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("20260825-0001");

        var handler = new CreatePatientVisitCommandHandler(
            patients.Object, visits.Object, unitOfWork.Object, labIdGenerator.Object, currentUser);
        var command = new CreatePatientVisitCommand(1, null, null);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        await handler.Handle(command, CancellationToken.None);

        visits.Verify(repository => repository.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTestToVisit_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var visits = new Mock<IVisitRepository>();
        var priceLists = new Mock<IPriceListRepository>();
        var priceResolver = new Mock<IPriceListResolverService>();
        var tests = new Mock<ITestRepository>();
        var sampleRepository = new Mock<IRepository<Sample>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new TestCurrentUserService();
        visits.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PatientVisit.Create(1, 1, "20260825-0001", null, null));

        var handler = new AddTestToVisitCommandHandler(
            visits.Object,
            priceLists.Object,
            priceResolver.Object,
            tests.Object,
            new VisitTestSnapshotter(),
            sampleRepository.Object,
            unitOfWork.Object,
            currentUser);
        var command = new AddTestToVisitCommand(1, Array.Empty<int>(), 10, false);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        await handler.Handle(command, CancellationToken.None);

        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTestsToVisit_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var visit = PatientVisit.Create(1, 1, "20260825-0001", null, null);
        visit.VisitTests.Add(new VisitTest(1, 7, 100m, false));
        var visits = new Mock<IVisitRepository>();
        var currentUser = new TestCurrentUserService();
        visits.Setup(repository => repository.GetByIdWithTestsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);

        var handler = new AddTestsToVisitCommandHandler(
            visits.Object,
            new Mock<ITestRepository>().Object,
            new Mock<IRepository<TestGroup>>().Object,
            new Mock<ITestGroupItemRepository>().Object,
            new Mock<ICommercialPackageRepository>().Object,
            new Mock<IRepository<VisitCommercialPackage>>().Object,
            new Mock<IPriceListResolverService>().Object,
            new Mock<IPriceListRepository>().Object,
            new VisitTestSnapshotter(),
            new Mock<IUnitOfWork>().Object,
            currentUser);
        var command = new AddTestsToVisitCommand(1, "Direct", null, null, "7", false);

        await AssertUnauthenticatedAsync(() => handler.Handle(command, CancellationToken.None), currentUser);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task FindDuplicatePatients_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var patients = new Mock<IPatientRepository>();
        var currentUser = new TestCurrentUserService();
        patients.Setup(repository => repository.FindProbableDuplicatesAsync(
                "Patient One", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Patient>());

        var handler = new FindDuplicatePatientsQueryHandler(patients.Object, currentUser);
        var query = new FindDuplicatePatientsQuery("Patient One", null, null);

        await AssertUnauthenticatedAsync(() => handler.Handle(query, CancellationToken.None), currentUser);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVisitTestCount_requires_authentication_and_succeeds_for_authenticated_user()
    {
        var visits = new Mock<IVisitRepository>();
        var currentUser = new TestCurrentUserService();
        visits.Setup(repository => repository.GetByIdWithTestsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PatientVisit.Create(1, 1, "20260825-0001", null, null));

        var handler = new GetVisitTestCountQueryHandler(visits.Object, currentUser);
        var query = new GetVisitTestCountQuery(1);

        await AssertUnauthenticatedAsync(() => handler.Handle(query, CancellationToken.None), currentUser);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(0, result);
    }

    private static async Task AssertUnauthenticatedAsync<T>(
        Func<Task<T>> action,
        TestCurrentUserService currentUser)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal(AuthenticationMessage, exception.Message);
        currentUser.UserId = 1;
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public int? UserId { get; set; }
        public string? Username => null;
        public string? Role => null;
    }
}
