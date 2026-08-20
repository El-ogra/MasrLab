using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;
using CoreTest = MasrLab.Domain.Entities.Core.Test;
using CoreTestComponent = MasrLab.Domain.Entities.Core.TestComponent;

namespace MasrLab.Application.Tests;

public class DoctorsReferralsAndTestsMasterDataHandlersTests
{
    private static AddTestCommand NewTest() => new("CBC", "CBC report", "CBC receipt", "Hematology", "B1", 120m, "24h", false, "mg",
        null, null, null, null, null, null, false, false, false, false, 1, 0, ReferenceType.General, null, null, null, null, null, false, null, null, null, 35.50m);
    private static UpdateTestCommand ChangedTest(int id = 1) => new(id, "CRP", "CRP report", "CRP receipt", "Chemistry", null, 220m, "48h", true, "mg/L",
        null, null, null, null, null, null, false, false, false, false, 2, 1, ReferenceType.General, null, null, null, null, null, false, null, null, null, 40m);

    [Fact]
    public async Task AddDoctor_persists_requested_doctor()
    {
        var repo = new Mock<IRepository<Doctor>>(); Doctor? saved = null; repo.Setup(x => x.AddAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>())).Callback<Doctor, CancellationToken>((d, _) => saved = d);
        await new AddDoctorCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(new("Dr Mona", "01012345678", "Cairo", 15m), default);
        Assert.NotNull(saved); Assert.Equal("Dr Mona", saved!.Name); Assert.Equal(15m, saved.CommissionPercent);
    }

    [Fact]
    public async Task AddDoctor_rejects_invalid_commission()
    {
        var repo = new Mock<IRepository<Doctor>>();
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => new AddDoctorCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(new("Dr Mona", null, null, 101m), default));
        repo.Verify(x => x.AddAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddReferralEntity_persists_contact_and_balance()
    {
        var repo = new Mock<IRepository<ReferralEntity>>(); ReferralEntity? saved = null; repo.Setup(x => x.AddAsync(It.IsAny<ReferralEntity>(), It.IsAny<CancellationToken>())).Callback<ReferralEntity, CancellationToken>((e, _) => saved = e);
        await new AddReferralEntityCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(new("Clinic", ReferralEntityType.ReferralEntity, "Sara", "01012345678", "fax", "Giza", 2, 99m), default);
        Assert.NotNull(saved); Assert.Equal("Clinic", saved!.Name); Assert.Equal(2, saved.PriceListId); Assert.Equal(99m, saved.AccountBalance);
    }

    [Fact]
    public async Task AddReferralEntity_rejects_invalid_phone()
    {
        var repo = new Mock<IRepository<ReferralEntity>>();
        await Assert.ThrowsAnyAsync<Exception>(() => new AddReferralEntityCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(new("Clinic", ReferralEntityType.ReferralEntity, null, "bad", null, null, 2, 0), default));
        repo.Verify(x => x.AddAsync(It.IsAny<ReferralEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePriceListItems_replaces_old_items_with_requested_items()
    {
        var lists = new Mock<IRepository<PriceList>>(); var items = new Mock<IRepository<PriceListItem>>(); var old = new PriceListItem { Id = 3 }; var list = new PriceList { Id = 1, PriceListItems = new List<PriceListItem> { old } }; PriceListItem? added = null;
        lists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(list); items.Setup(x => x.AddAsync(It.IsAny<PriceListItem>(), It.IsAny<CancellationToken>())).Callback<PriceListItem, CancellationToken>((i, _) => added = i);
        await new UpdatePriceListItemsCommandHandler(lists.Object, items.Object, new Mock<IUnitOfWork>().Object).Handle(new(1, new() { new(4, 75m) }), default);
        items.Verify(x => x.Delete(old), Times.Once); Assert.NotNull(added); Assert.Equal(4, added!.TestId); Assert.Equal(75m, added.Price);
    }

    [Fact]
    public async Task UpdatePriceListItems_throws_for_missing_price_list()
    {
        var lists = new Mock<IRepository<PriceList>>(); lists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((PriceList?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new UpdatePriceListItemsCommandHandler(lists.Object, new Mock<IRepository<PriceListItem>>().Object, new Mock<IUnitOfWork>().Object).Handle(new(1, new()), default));
    }

    [Fact]
    public async Task GetPriceListForPrint_returns_list_and_mapped_items()
    {
        var repo = new Mock<IPriceListRepository>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();
        var item = new PriceListItem { Id = 5, TestId = 2, Price = 12m };
        var test = new CoreTest { Id = 2, Name = "CBC", Group = "Hematology" };
        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Cash", PriceListItems = new List<PriceListItem> { item } });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest> { test });
        mapper.Setup(x => x.Map<PriceListItemDto>(item))
            .Returns(new PriceListItemDto { Id = 5, TestId = 2, Price = 12m });
        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);
        Assert.NotNull(result);
        Assert.Equal("Cash", result!.Name);
        Assert.Single(result.Items);
        Assert.Equal(12m, result.Items[0].Price);
        Assert.Equal("Hematology", result.Items[0].TestGroupName);
    }

    [Fact]
    public async Task GetPriceListForPrint_throws_for_missing_list()
    {
        var repo = new Mock<IPriceListRepository>();
        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => new GetPriceListForPrintQueryHandler(
                repo.Object,
                new Mock<IRepository<CoreTest>>().Object,
                new Mock<IMapper>().Object)
            .Handle(new(1), default));
    }

    [Fact]
    public async Task AddReferenceValue_persists_gender_specific_range()
    {
        var refs = new Mock<IReferenceValueRepository>(); ReferenceValue? saved = null;
        refs.Setup(x => x.AddAsync(It.IsAny<ReferenceValue>(), It.IsAny<CancellationToken>())).Callback<ReferenceValue, CancellationToken>((r, _) => saved = r);
        refs.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue>());
        var compRepo = new Mock<IRepository<CoreTestComponent>>();
        compRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new CoreTestComponent { Id = id, TestId = 2, ResultEntryKind = ResultEntryKind.Ordinary });
        await new AddReferenceValueCommandHandler(refs.Object, compRepo.Object, new Mock<IUnitOfWork>().Object).Handle(
            new(2, 1, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9", null, null, null, null, null, false, "high", "low"), default);
        Assert.NotNull(saved); Assert.Equal(ReferenceValueGender.Male, saved!.Gender); Assert.Equal("1-9", saved.NormalRange);
    }

    [Fact]
    public async Task AddReferenceValue_throws_when_overlap_exists()
    {
        var refs = new Mock<IReferenceValueRepository>();
        var existing = new ReferenceValue { Id = 1, TestId = 2, TestComponentId = 1, Gender = ReferenceValueGender.Male, AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years };
        refs.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue> { existing });
        var compRepo = new Mock<IRepository<CoreTestComponent>>();
        compRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new CoreTestComponent { Id = id, TestId = 2, ResultEntryKind = ResultEntryKind.Ordinary });
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => new AddReferenceValueCommandHandler(refs.Object, compRepo.Object, new Mock<IUnitOfWork>().Object).Handle(
            new(2, 1, ReferenceValueGender.Male, 5, 15, AgeUnit.Years, "5-15", null, null, null, null, null, false, null, null), default));
    }

    [Fact]
    public async Task AddReferenceValue_throws_when_no_constraint_overlaps_with_range()
    {
        var refs = new Mock<IReferenceValueRepository>();
        var existing = new ReferenceValue { Id = 1, TestId = 2, TestComponentId = 1, Gender = ReferenceValueGender.Both, AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years };
        refs.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue> { existing });
        var compRepo = new Mock<IRepository<CoreTestComponent>>();
        compRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new CoreTestComponent { Id = id, TestId = 2, ResultEntryKind = ResultEntryKind.Ordinary });
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => new AddReferenceValueCommandHandler(refs.Object, compRepo.Object, new Mock<IUnitOfWork>().Object).Handle(
            new(2, 1, ReferenceValueGender.Male, 1, 29, AgeUnit.Days, "1-29", null, null, null, null, null, false, null, null), default));
    }

    [Fact]
    public async Task AddReferenceValue_propagates_save_failure()
    {
        var refs = new Mock<IReferenceValueRepository>();
        refs.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue>());
        var uow = new Mock<IUnitOfWork>(); uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("write failed"));
        var compRepo = new Mock<IRepository<CoreTestComponent>>();
        compRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new CoreTestComponent { Id = id, TestId = 2, ResultEntryKind = ResultEntryKind.Ordinary });
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AddReferenceValueCommandHandler(refs.Object, compRepo.Object, uow.Object).Handle(
            new(2, 1, ReferenceValueGender.Female, 1, 9, AgeUnit.Years, "1-9", null, null, null, null, null, false, null, null), default));
    }

    [Fact]
    public async Task GetTestWithReferences_returns_test_and_mapped_references()
    {
        var tests = new Mock<IRepository<CoreTest>>(); var refs = new Mock<IReferenceValueRepository>(); var mapper = new Mock<IMapper>(); var test = new CoreTest { Id = 1, Name = "CBC", Price = 20m, CostPrice = 15m }; var reference = new ReferenceValue { Id = 2, TestId = 1, NormalRange = "normal" };
        tests.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(test); refs.Setup(x => x.GetByTestIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue> { reference }); mapper.Setup(x => x.Map<ReferenceValueDto>(reference)).Returns(new ReferenceValueDto { Id = 2, TestId = 1, NormalRange = "normal" });
        var result = await new GetTestWithReferencesQueryHandler(tests.Object, refs.Object, mapper.Object).Handle(new(1), default);
        Assert.NotNull(result); Assert.Equal("CBC", result!.Name); Assert.Equal(15m, result.CostPrice); Assert.Single(result.ReferenceValues);
    }

    [Fact]
    public async Task GetTestWithReferences_returns_null_for_missing_test()
    {
        var tests = new Mock<IRepository<CoreTest>>(); tests.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CoreTest?)null);
        var result = await new GetTestWithReferencesQueryHandler(tests.Object, new Mock<IReferenceValueRepository>().Object, new Mock<IMapper>().Object).Handle(new(1), default);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTestWithReferences_preserves_null_CostPrice()
    {
        var tests = new Mock<IRepository<CoreTest>>(); var refs = new Mock<IReferenceValueRepository>(); var mapper = new Mock<IMapper>(); var test = new CoreTest { Id = 1, Name = "CBC", Price = 20m, CostPrice = null };
        tests.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(test); refs.Setup(x => x.GetByTestIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReferenceValue>());
        var result = await new GetTestWithReferencesQueryHandler(tests.Object, refs.Object, mapper.Object).Handle(new(1), default);
        Assert.NotNull(result); Assert.Null(result!.CostPrice);
    }

    [Fact]
    public async Task UpdateTest_changes_all_editable_fields()
    {
        var repo = new Mock<IRepository<CoreTest>>(); var test = new CoreTest { Id = 1, Name = "Old" }; repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        await new UpdateTestCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(ChangedTest(), default);
        Assert.Equal("CRP", test.Name); Assert.Equal(220m, test.Price); Assert.True(test.LabToLabFlag); Assert.Equal(40m, test.CostPrice); repo.Verify(x => x.Update(test), Times.Once);
    }

    [Fact]
    public async Task UpdateTest_throws_for_missing_test()
    {
        var repo = new Mock<IRepository<CoreTest>>(); repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CoreTest?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new UpdateTestCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(ChangedTest(), default));
    }

    [Fact]
    public async Task AddTest_persists_requested_test()
    {
        var repo = new Mock<IRepository<CoreTest>>(); CoreTest? saved = null; repo.Setup(x => x.AddAsync(It.IsAny<CoreTest>(), It.IsAny<CancellationToken>())).Callback<CoreTest, CancellationToken>((t, _) => { t.Id = 1; saved = t; });
        await new AddTestCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(NewTest(), default);
        Assert.NotNull(saved); Assert.Equal("CBC", saved!.Name); Assert.Equal(120m, saved.Price); Assert.Equal(35.50m, saved.CostPrice);
    }

    [Fact]
    public async Task AddTest_propagates_save_failure()
    {
        var uow = new Mock<IUnitOfWork>(); uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("write failed"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AddTestCommandHandler(new Mock<IRepository<CoreTest>>().Object, uow.Object).Handle(NewTest(), default));
    }

    [Fact]
    public async Task AddTest_creates_first_component_automatically()
    {
        var repo = new Mock<IRepository<CoreTest>>();
        CoreTest? savedTest = null;
        repo.Setup(x => x.AddAsync(It.IsAny<CoreTest>(), It.IsAny<CancellationToken>()))
            .Callback<CoreTest, CancellationToken>((t, _) => { t.Id = 5; savedTest = t; });

        await new AddTestCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(NewTest(), default);

        Assert.NotNull(savedTest);
        Assert.Single(savedTest!.TestComponents);
        var component = savedTest.TestComponents.First();
        Assert.Equal("CBC", component.Name);
        Assert.Equal("mg", component.Unit);
        Assert.Equal(1, component.DisplayOrder);
        Assert.Equal(ResultEntryKind.Ordinary, component.ResultEntryKind);
    }

}
