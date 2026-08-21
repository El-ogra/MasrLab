using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.DeleteReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class Module12_HandlerTests
{
    #region AddReferralEntityCommand — D-01 price-list existence check

    [Fact]
    public async Task AddReferralEntity_ReferralEntity_NonExistentPriceList_Throws()
    {
        var repo = new Mock<IRepository<ReferralEntity>>();
        var priceRepo = new Mock<IPriceListRepository>();
        priceRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);
        priceRepo.Setup(x => x.GetLabToLabAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var handler = new AddReferralEntityCommandHandler(repo.Object, priceRepo.Object, new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new("Clinic", ReferralEntityType.ReferralEntity, null, null, null, null, null, null, null, 99), default));

        repo.Verify(x => x.AddAsync(It.IsAny<ReferralEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateReferralEntity — OQ-2 forward-only (no billing interaction)

    [Fact]
    public async Task UpdateReferralEntity_PriceListSwap_NoBillingInteraction()
    {
        var repo = new Mock<IReferralEntityRepository>();
        var priceRepo = new Mock<IPriceListRepository>();
        var uow = new Mock<IUnitOfWork>();

        var existing = new ReferralEntity
        {
            Id = 1,
            Name = "Hospital",
            EntityType = ReferralEntityType.ReferralEntity,
            PriceListId = 1
        };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var newList = new PriceList { Id = 2, Name = "New List" };
        priceRepo.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(newList);

        var handler = new UpdateReferralEntityCommandHandler(repo.Object, priceRepo.Object, uow.Object);
        await handler.Handle(new(1, "Hospital Updated", null, null, null, null, null, null, null, 2), default);

        Assert.Equal("Hospital Updated", existing.Name);
        Assert.Equal(2, existing.PriceListId);
        repo.Verify(x => x.Update(existing), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateReferralEntity — OQ-7: command has no EntityType property

    [Fact]
    public void UpdateReferralEntityCommand_HasNoEntityTypeProperty()
    {
        var prop = typeof(UpdateReferralEntityCommand).GetProperty("EntityType");
        Assert.Null(prop);
    }

    #endregion

    #region DeleteReferralEntity

    [Fact]
    public async Task DeleteReferralEntity_NonExistent_ThrowsNotFound()
    {
        var repo = new Mock<IReferralEntityRepository>();
        repo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferralEntity?)null);

        var handler = new DeleteReferralEntityCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new(999), default));
    }

    [Fact]
    public async Task DeleteReferralEntity_AlreadyDeleted_ThrowsBusinessRule()
    {
        var repo = new Mock<IReferralEntityRepository>();
        var existing = new ReferralEntity { Id = 1, Name = "Old", IsDeleted = true };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = new DeleteReferralEntityCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new(1), default));
    }

    [Fact]
    public async Task DeleteReferralEntity_SetsIsDeleted()
    {
        var repo = new Mock<IReferralEntityRepository>();
        var uow = new Mock<IUnitOfWork>();
        var existing = new ReferralEntity { Id = 1, Name = "To Delete", IsDeleted = false };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = new DeleteReferralEntityCommandHandler(repo.Object, uow.Object);
        await handler.Handle(new(1), default);

        Assert.True(existing.IsDeleted);
        repo.Verify(x => x.Update(existing), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Function 5 — out-of-pool lab id rejection

    [Fact]
    public async Task AddTest_OutOfPoolLabId_ThrowsBusinessRule()
    {
        var testRepo = new Mock<IRepository<Domain.Entities.Core.Test>>();
        var referralRepo = new Mock<IReferralEntityRepository>();

        var doctorEntity = new ReferralEntity
        {
            Id = 10,
            Name = "Dr. Bad",
            EntityType = ReferralEntityType.TreatingDoctor
        };
        referralRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctorEntity);

        var handler = new AddTestCommandHandler(testRepo.Object, referralRepo.Object, new Mock<IUnitOfWork>().Object);
        var cmd = new AddTestCommand("CBC", "R", "R", "G", null, 100m, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null, true, null, 50m, null, 35m, 10);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(cmd, default));
        testRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Core.Test>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddTest_ValidOutsourcedSamplesLab_Succeeds()
    {
        var testRepo = new Mock<IRepository<Domain.Entities.Core.Test>>();
        var referralRepo = new Mock<IReferralEntityRepository>();
        var uow = new Mock<IUnitOfWork>();

        var labEntity = new ReferralEntity
        {
            Id = 5,
            Name = "External Lab",
            EntityType = ReferralEntityType.OutsourcedSamples,
            PriceList = new PriceList { Id = 1, IsLabToLab = true }
        };
        referralRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(labEntity);

        Domain.Entities.Core.Test? saved = null;
        testRepo.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Core.Test>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Core.Test, CancellationToken>((t, _) => { t.Id = 1; saved = t; });

        var handler = new AddTestCommandHandler(testRepo.Object, referralRepo.Object, uow.Object);
        var cmd = new AddTestCommand("CBC", "R", "R", "G", null, 100m, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null, true, null, 50m, null, 35m, 5);

        await handler.Handle(cmd, default);
        Assert.NotNull(saved);
        Assert.Equal(5, saved!.OutsourcedLabReferralEntityId);
    }

    [Fact]
    public async Task UpdateTest_OutOfPoolLabId_ThrowsBusinessRule()
    {
        var testRepo = new Mock<IRepository<Domain.Entities.Core.Test>>();
        var referralRepo = new Mock<IReferralEntityRepository>();
        var uow = new Mock<IUnitOfWork>();

        var existingTest = new Domain.Entities.Core.Test { Id = 1, Name = "Old" };
        testRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTest);

        var doctorEntity = new ReferralEntity
        {
            Id = 10,
            Name = "Dr. Bad",
            EntityType = ReferralEntityType.TreatingDoctor
        };
        referralRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctorEntity);

        var handler = new UpdateTestCommandHandler(testRepo.Object, referralRepo.Object, uow.Object);
        var cmd = new UpdateTestCommand(1, "CBC", "R", "R", "G", null, 100m, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null, true, null, 50m, null, 35m, 10);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(cmd, default));
        testRepo.Verify(x => x.Update(It.IsAny<Domain.Entities.Core.Test>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTest_NullLabId_WhenSentOutsideLab_NullsOutTheLabId()
    {
        var testRepo = new Mock<IRepository<Domain.Entities.Core.Test>>();
        var referralRepo = new Mock<IReferralEntityRepository>();
        var uow = new Mock<IUnitOfWork>();

        var existingTest = new Domain.Entities.Core.Test { Id = 1, Name = "Old" };
        testRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTest);

        var handler = new UpdateTestCommandHandler(testRepo.Object, referralRepo.Object, uow.Object);
        var cmd = new UpdateTestCommand(1, "CBC", "R", "R", "G", null, 100m, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null, true, null, 50m, null, 35m, null);

        await handler.Handle(cmd, default);

        // When SentOutsideLab=true but OutsourcedLabReferralEntityId=null, handler proceeds
        // (validator should catch this, but handler test verifies no crash)
        Assert.Null(existingTest.OutsourcedLabReferralEntityId);
    }

    #endregion
}
