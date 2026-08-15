using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;
using CoreTestComponent = MasrLab.Domain.Entities.Core.TestComponent;

namespace MasrLab.Application.Tests;

public class ReferenceValueHandlersTests
{
    private readonly Mock<IReferenceValueRepository> _refsRepoMock = new();
    private readonly Mock<IRepository<CoreTestComponent>> _compRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    #region AddReferenceValueCommand Tests

    [Fact]
    public async Task AddReferenceValue_persists_new_reference_value()
    {
        // Arrange
        ReferenceValue? saved = null;
        _refsRepoMock.Setup(x => x.AddAsync(It.IsAny<ReferenceValue>(), It.IsAny<CancellationToken>()))
            .Callback<ReferenceValue, CancellationToken>((r, _) => saved = r);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9",
            10m, 50m, "mg/dL", "L", "H", false, "high comment", "low comment");

        // Act
        await handler.Handle(command, default);

        // Assert
        Assert.NotNull(saved);
        Assert.Equal(2, saved!.TestId);
        Assert.Equal(ReferenceValueGender.Male, saved.Gender);
        Assert.Equal(1, saved.AgeMin);
        Assert.Equal(9, saved.AgeMax);
        Assert.Equal(AgeUnit.Years, saved.AgeUnit);
        Assert.Equal("1-9", saved.NormalRange);
        Assert.Equal(10m, saved.LowLimit);
        Assert.Equal(50m, saved.HighLimit);
        Assert.Equal("mg/dL", saved.TestUnit);
        Assert.Equal("L", saved.LowFlag);
        Assert.Equal("H", saved.HighFlag);
        Assert.False(saved.ForPregnantOnly);
        Assert.Equal("high comment", saved.HighComment);
        Assert.Equal("low comment", saved.LowComment);
    }

    [Fact]
    public async Task AddReferenceValue_throws_when_overlap_exists()
    {
        // Arrange - نفس الجنس + نفس الوحدة + تداخل أعمار
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Male, 5, 15, AgeUnit.Years, "5-15",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task AddReferenceValue_throws_when_no_constraint_overlaps_with_range()
    {
        // Arrange - نطاق "بدون قيد" (0,0) يتداخل مع نطاق بعمر محدد
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Both,
            AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Male, 1, 29, AgeUnit.Days, "1-29",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task AddReferenceValue_allows_overlapping_numbers_with_different_age_units()
    {
        // Arrange - أرقام متقاربة لكن بوحدتي عمر مختلفتين (Days وYears)
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 29, AgeUnit = AgeUnit.Days
        };
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        ReferenceValue? saved = null;
        _refsRepoMock.Setup(x => x.AddAsync(It.IsAny<ReferenceValue>(), It.IsAny<CancellationToken>()))
            .Callback<ReferenceValue, CancellationToken>((r, _) => saved = r);

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Male, 0, 120, AgeUnit.Years, "0-120",
            null, null, null, null, null, false, null, null);

        // Act
        await handler.Handle(command, default);

        // Assert - يُسمح بالحفظ لأن وحدات العمر مختلفة
        Assert.NotNull(saved);
        Assert.Equal(AgeUnit.Years, saved!.AgeUnit);
    }

    [Fact]
    public async Task AddReferenceValue_allows_same_age_unit_different_ranges()
    {
        // Arrange - نطاقان بنفس الوحدة بأعمار غير متداخلة
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 5, AgeUnit = AgeUnit.Days
        };
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        ReferenceValue? saved = null;
        _refsRepoMock.Setup(x => x.AddAsync(It.IsAny<ReferenceValue>(), It.IsAny<CancellationToken>()))
            .Callback<ReferenceValue, CancellationToken>((r, _) => saved = r);

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Male, 6, 10, AgeUnit.Days, "6-10",
            null, null, null, null, null, false, null, null);

        // Act
        await handler.Handle(command, default);

        // Assert - يُسمح بالحفظ لأن النطاقين لا يتداخلان
        Assert.NotNull(saved);
        Assert.Equal(6, saved!.AgeMin);
        Assert.Equal(10, saved.AgeMax);
    }

    [Fact]
    public async Task AddReferenceValue_propagates_save_failure()
    {
        // Arrange
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new AddReferenceValueCommand(
            2, null, ReferenceValueGender.Female, 1, 9, AgeUnit.Years, "1-9",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, default));
    }

    #endregion

    #region PT Analysis Scenarios Test

    [Fact]
    public async Task AddReferenceValue_pt_analysis_scenarios()
    {
        // Arrange - بيانات PT الفعلية من الصورة المرجعية
        // 3 نطاقات لنفس التحليل (PT) بنفس الجنس (Bo) بوحدات عمر مختلفة
        var existingRefs = new List<ReferenceValue>();

        _refsRepoMock.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => existingRefs);

        _refsRepoMock.Setup(x => x.AddAsync(It.IsAny<ReferenceValue>(), It.IsAny<CancellationToken>()))
            .Callback<ReferenceValue, CancellationToken>((r, _) =>
            {
                r.Id = existingRefs.Count + 1;
                existingRefs.Add(r);
            });

        var handler = new AddReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);

        // Act - حفظ النطاق الأول: 1-29 Days
        await handler.Handle(new AddReferenceValueCommand(
            1, null, ReferenceValueGender.Both, 1, 29, AgeUnit.Days, "1-29",
            null, null, null, null, null, false, null, null), default);

        // Act - حفظ النطاق الثاني: 1-12 Months
        await handler.Handle(new AddReferenceValueCommand(
            1, null, ReferenceValueGender.Both, 1, 12, AgeUnit.Months, "1-12",
            null, null, null, null, null, false, null, null), default);

        // Act - حفظ النطاق الثالث: 0-120 Years
        await handler.Handle(new AddReferenceValueCommand(
            1, null, ReferenceValueGender.Both, 0, 120, AgeUnit.Years, "0-120",
            null, null, null, null, null, false, null, null), default);

        // Assert - يجب أن تُحفظ النطاقات الثلاثة بنجاح
        Assert.Equal(3, existingRefs.Count);
        Assert.Equal(AgeUnit.Days, existingRefs[0].AgeUnit);
        Assert.Equal(AgeUnit.Months, existingRefs[1].AgeUnit);
        Assert.Equal(AgeUnit.Years, existingRefs[2].AgeUnit);
    }

    #endregion

    #region UpdateReferenceValueCommand Tests

    [Fact]
    public async Task UpdateReferenceValue_updates_existing_reference_value()
    {
        // Arrange
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years,
            NormalRange = "1-9"
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            1, 2, null, ReferenceValueGender.Female, 5, 15, AgeUnit.Years, "5-15",
            20m, 80m, "g/L", "L", "H", true, "high", "low");

        // Act
        await handler.Handle(command, default);

        // Assert
        Assert.Equal(ReferenceValueGender.Female, existing.Gender);
        Assert.Equal(5, existing.AgeMin);
        Assert.Equal(15, existing.AgeMax);
        Assert.Equal("5-15", existing.NormalRange);
        Assert.Equal(20m, existing.LowLimit);
        Assert.Equal(80m, existing.HighLimit);
        Assert.Equal("g/L", existing.TestUnit);
        Assert.Equal("L", existing.LowFlag);
        Assert.Equal("H", existing.HighFlag);
        Assert.True(existing.ForPregnantOnly);
        Assert.Equal("high", existing.HighComment);
        Assert.Equal("low", existing.LowComment);
        _refsRepoMock.Verify(x => x.Update(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateReferenceValue_throws_when_not_found()
    {
        // Arrange
        _refsRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferenceValue?)null);

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            999, 2, null, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task UpdateReferenceValue_throws_when_overlap_with_another()
    {
        // Arrange - نطاقان بنفس الجنس + نفس الوحدة + تداخل أعمار
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        var another = new ReferenceValue
        {
            Id = 2, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 5, AgeMax = 15, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing, another });

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            1, 2, null, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task UpdateReferenceValue_allows_same_id_no_overlap()
    {
        // Arrange - تحديث السجل الحالي لا يُعتبر تداخلاً مع نفسه
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            1, 2, null, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9 updated",
            null, null, null, null, null, false, null, null);

        // Act
        await handler.Handle(command, default);

        // Assert - يجب أن يُحدّث بنجاح
        Assert.Equal("1-9 updated", existing.NormalRange);
        _refsRepoMock.Verify(x => x.Update(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateReferenceValue_allows_no_constraint_when_no_other_ranges()
    {
        // Arrange - تحديث نطاق ليكون "بدون قيد" عند عدم وجود نطاقات أخرى
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Both,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            1, 2, null, ReferenceValueGender.Both, 0, 0, AgeUnit.Years, "All ages",
            null, null, null, null, null, false, null, null);

        // Act
        await handler.Handle(command, default);

        // Assert - يجب أن يُحدّث بنجاح (بدون قيد + لا نطاقات أخرى)
        Assert.Equal(0, existing.AgeMin);
        Assert.Equal(0, existing.AgeMax);
        Assert.Equal("All ages", existing.NormalRange);
    }

    [Fact]
    public async Task UpdateReferenceValue_propagates_save_failure()
    {
        // Arrange
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue> { existing });
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        var handler = new UpdateReferenceValueCommandHandler(_refsRepoMock.Object, _compRepoMock.Object, _uowMock.Object);
        var command = new UpdateReferenceValueCommand(
            1, 2, null, ReferenceValueGender.Male, 1, 9, AgeUnit.Years, "1-9",
            null, null, null, null, null, false, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, default));
    }

    #endregion

    #region DeleteReferenceValueCommand Tests

    [Fact]
    public async Task DeleteReferenceValue_deletes_existing_reference_value()
    {
        // Arrange
        var existing = new ReferenceValue
        {
            Id = 1, TestId = 2, Gender = ReferenceValueGender.Male,
            AgeMin = 1, AgeMax = 9, AgeUnit = AgeUnit.Years
        };
        _refsRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new DeleteReferenceValueCommandHandler(_refsRepoMock.Object, _uowMock.Object);
        var command = new DeleteReferenceValueCommand(1);

        // Act
        await handler.Handle(command, default);

        // Assert
        _refsRepoMock.Verify(x => x.Delete(existing), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReferenceValue_throws_when_not_found()
    {
        // Arrange
        _refsRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferenceValue?)null);

        var handler = new DeleteReferenceValueCommandHandler(_refsRepoMock.Object, _uowMock.Object);
        var command = new DeleteReferenceValueCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(command, default));
    }

    #endregion

    #region GetReferenceValuesByTestIdQuery Tests

    [Fact]
    public async Task GetReferenceValuesByTestId_returns_mapped_list()
    {
        // Arrange
        var references = new List<ReferenceValue>
        {
            new() { Id = 1, TestId = 2, Gender = ReferenceValueGender.Male, NormalRange = "1-9" },
            new() { Id = 2, TestId = 2, Gender = ReferenceValueGender.Female, NormalRange = "1-8" }
        };
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(references);

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(x => x.Map<List<ReferenceValueDto>>(It.IsAny<List<ReferenceValue>>()))
            .Returns(new List<ReferenceValueDto>
            {
                new() { Id = 1, TestId = 2, Gender = ReferenceValueGender.Male, NormalRange = "1-9" },
                new() { Id = 2, TestId = 2, Gender = ReferenceValueGender.Female, NormalRange = "1-8" }
            });

        var handler = new GetReferenceValuesByTestIdQueryHandler(_refsRepoMock.Object, mapperMock.Object);

        // Act
        var result = await handler.Handle(new GetReferenceValuesByTestIdQuery(2), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("1-9", result[0].NormalRange);
        Assert.Equal("1-8", result[1].NormalRange);
    }

    [Fact]
    public async Task GetReferenceValuesByTestId_returns_empty_when_no_references()
    {
        // Arrange
        _refsRepoMock.Setup(x => x.GetByTestIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(x => x.Map<List<ReferenceValueDto>>(It.IsAny<List<ReferenceValue>>()))
            .Returns(new List<ReferenceValueDto>());

        var handler = new GetReferenceValuesByTestIdQueryHandler(_refsRepoMock.Object, mapperMock.Object);

        // Act
        var result = await handler.Handle(new GetReferenceValuesByTestIdQuery(999), default);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion
}
