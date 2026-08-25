using MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;
using MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;
using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Catalog = MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;
using Manual = MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;
using Update = MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;
using Moq;

namespace MasrLab.Application.Tests;

public sealed class CultureAntibioticCommandHandlerTests
{
    [Fact]
    public async Task Add_catalog_antibiotic_creates_assignment_and_names()
    {
        var testRepository = TestRepositoryWithCulture();
        var antibioticRepository = new Mock<IAntibioticRepository>();
        antibioticRepository.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Antibiotic { Id = 7, Name = "AMX", ScientificName = "Amoxicillin" });
        var assignmentRepository = new Mock<ICultureAntibioticRepository>();
        assignmentRepository.Setup(x => x.ExistsAsync(12, 7, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        CultureAntibiotic? saved = null;
        assignmentRepository.Setup(x => x.AddAsync(It.IsAny<CultureAntibiotic>(), It.IsAny<CancellationToken>()))
            .Callback<CultureAntibiotic, CancellationToken>((entity, _) => saved = entity);
        var unitOfWork = new Mock<IUnitOfWork>();
        var commercialNames = new Mock<IRepository<CultureAntibioticCommercialName>>();

        await new AddAntibioticToCultureTestCommandHandler(
            testRepository.Object,
            antibioticRepository.Object,
            assignmentRepository.Object,
            commercialNames.Object,
            unitOfWork.Object).Handle(
                new(12, 7, "S", true, false, new[] { new Catalog.CommercialNameInput("Amoxil", true) }), default);

        Assert.NotNull(saved);
        Assert.Equal(12, saved!.CultureTestId);
        Assert.Equal(7, saved.AntibioticId);
        commercialNames.Verify(x => x.AddAsync(It.Is<CultureAntibioticCommercialName>(n => n.Name == "Amoxil" && n.Print), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Add_rejects_non_culture_and_duplicate_assignment()
    {
        var testRepository = new Mock<IRepository<Test>>();
        testRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Test { Id = 1, Group = "Chemistry" });
        var handler = new AddAntibioticToCultureTestCommandHandler(
            testRepository.Object,
            new Mock<IAntibioticRepository>().Object,
            new Mock<ICultureAntibioticRepository>().Object,
            new Mock<IRepository<CultureAntibioticCommercialName>>().Object,
            new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(
            new(1, 2, null, false, false, Array.Empty<Catalog.CommercialNameInput>()), default));

        testRepository = TestRepositoryWithCulture();
        var antibioticRepository = new Mock<IAntibioticRepository>();
        antibioticRepository.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Antibiotic { Id = 7, Name = "AMX" });
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.ExistsAsync(12, 7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        handler = new AddAntibioticToCultureTestCommandHandler(
            testRepository.Object, antibioticRepository.Object, assignments.Object,
            new Mock<IRepository<CultureAntibioticCommercialName>>().Object,
            new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(
            new(12, 7, null, false, false, Array.Empty<Catalog.CommercialNameInput>()), default));
    }

    [Fact]
    public async Task Manual_add_creates_unknown_antibiotic_and_reuses_known_symbol()
    {
        var testRepository = TestRepositoryWithCulture();
        var antibiotics = new Mock<IAntibioticRepository>();
        antibiotics.Setup(x => x.GetBySymbolAsync("AMX", It.IsAny<CancellationToken>())).ReturnsAsync((Antibiotic?)null);
        Antibiotic? created = null;
        antibiotics.Setup(x => x.AddAsync(It.IsAny<Antibiotic>(), It.IsAny<CancellationToken>()))
            .Callback<Antibiotic, CancellationToken>((entity, _) => { entity.Id = 30; created = entity; });
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.ExistsBySymbolOrScientificNameAsync(12, "AMX", "Amoxicillin", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new AddManualAntibioticToCultureTestCommandHandler(
            testRepository.Object, antibiotics.Object, assignments.Object,
            new Mock<IRepository<CultureAntibioticCommercialName>>().Object,
            new Mock<IUnitOfWork>().Object);

        await handler.Handle(new(12, " AMX ", "Amoxicillin", null, false, false, Array.Empty<Manual.CommercialNameInput>()), default);
        Assert.NotNull(created);
        antibiotics.Verify(x => x.AddAsync(It.IsAny<Antibiotic>(), It.IsAny<CancellationToken>()), Times.Once);

        antibiotics.Reset();
        antibiotics.Setup(x => x.GetBySymbolAsync("AMX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Antibiotic { Id = 31, Name = "AMX", ScientificName = "Amoxicillin" });
        assignments.Setup(x => x.ExistsBySymbolOrScientificNameAsync(12, "AMX", "Amoxicillin", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        handler = new AddManualAntibioticToCultureTestCommandHandler(
            testRepository.Object, antibiotics.Object, assignments.Object,
            new Mock<IRepository<CultureAntibioticCommercialName>>().Object,
            new Mock<IUnitOfWork>().Object);
        await handler.Handle(new(12, "AMX", "Amoxicillin", null, false, false, Array.Empty<Manual.CommercialNameInput>()), default);
        antibiotics.Verify(x => x.AddAsync(It.IsAny<Antibiotic>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Manual_add_rejects_symbol_or_scientific_name_collision()
    {
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.ExistsBySymbolOrScientificNameAsync(12, "AMX", "Amoxicillin", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new AddManualAntibioticToCultureTestCommandHandler(
            TestRepositoryWithCulture().Object,
            new Mock<IAntibioticRepository>().Object,
            assignments.Object,
            new Mock<IRepository<CultureAntibioticCommercialName>>().Object,
            new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(
            new(12, "AMX", "Amoxicillin", null, false, false, Array.Empty<Manual.CommercialNameInput>()), default));
    }

    [Fact]
    public async Task Update_replaces_fields_and_commercial_names()
    {
        var assignment = CultureAntibiotic.Create(12, 7, "old", false, false);
        assignment.Id = 20;
        assignment.CommercialNames.Add(new CultureAntibioticCommercialName { Id = 1, CultureAntibioticId = 20, Name = "OldName", Print = false });
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.GetWithCommercialNamesAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);
        var names = new Mock<IRepository<CultureAntibioticCommercialName>>();
        var handler = new UpdateCultureAntibioticCommandHandler(assignments.Object, names.Object, new Mock<IUnitOfWork>().Object);

        await handler.Handle(new(20, "new", true, true, new[] { new Update.CommercialNameInput("OldName", true), new Update.CommercialNameInput("NewName", false) }), default);

        Assert.Equal("new", assignment.SensitivityText);
        Assert.True(assignment.Pregnant);
        Assert.True(assignment.Children);
        Assert.True(assignment.CommercialNames.First().Print);
        names.Verify(x => x.AddAsync(It.Is<CultureAntibioticCommercialName>(n => n.Name == "NewName"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_uses_repository_delete_and_saves()
    {
        var assignment = CultureAntibiotic.Create(12, 7);
        assignment.Id = 20;
        var commercialName = new CultureAntibioticCommercialName { Id = 21, CultureAntibioticId = 20, Name = "Amoxil" };
        assignment.CommercialNames.Add(commercialName);
        var assignments = new Mock<ICultureAntibioticRepository>();
        assignments.Setup(x => x.GetWithCommercialNamesAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);
        var commercialNames = new Mock<IRepository<CultureAntibioticCommercialName>>();
        var handler = new DeleteCultureAntibioticCommandHandler(assignments.Object, commercialNames.Object, new Mock<IUnitOfWork>().Object);
        await handler.Handle(new(20), default);
        assignments.Verify(x => x.Delete(assignment), Times.Once);
        commercialNames.Verify(x => x.Delete(commercialName), Times.Once);
    }

    private static Mock<IRepository<Test>> TestRepositoryWithCulture()
    {
        var repository = new Mock<IRepository<Test>>();
        repository.Setup(x => x.GetByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Test { Id = 12, Group = CultureGroup.Name });
        return repository;
    }
}
