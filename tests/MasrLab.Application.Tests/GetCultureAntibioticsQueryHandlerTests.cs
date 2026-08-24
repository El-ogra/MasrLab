using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibiotics;
using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibioticsAdmin;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public sealed class GetCultureAntibioticsQueryHandlerTests
{
    [Fact]
    public async Task Patient_query_applies_visibility_rules_and_maps_commercial_names()
    {
        var assignments = new[]
        {
            CreateAssignment(1, pregnant: false, children: false, name: "Universal"),
            CreateAssignment(2, pregnant: true, children: false, name: "PregnancyOnly"),
            CreateAssignment(3, pregnant: false, children: true, name: "ChildrenOnly"),
            CreateAssignment(4, pregnant: true, children: true, name: "Both"),
            CreateAssignment(5, pregnant: false, children: false, name: "Deleted", deleted: true)
        };
        var repository = new Mock<ICultureAntibioticRepository>();
        repository.Setup(x => x.GetByCultureTestIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        var result = await new GetCultureAntibioticsQueryHandler(repository.Object).Handle(
            new GetCultureAntibioticsQuery(12, PatientIsPregnant: false, PatientAgeYears: 30),
            CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(1, item.Id);
        Assert.Equal("AMX", item.Symbol);
        Assert.Equal("Amoxicillin", item.ScientificName);
        Assert.Equal("Universal", Assert.Single(item.CommercialNames).Name);
        repository.Verify(x => x.GetByCultureTestIdAsync(12, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Patient_query_returns_pregnancy_and_both_assignments_for_pregnant_adult()
    {
        var assignments = new[]
        {
            CreateAssignment(1, pregnant: false, children: false, name: "Universal"),
            CreateAssignment(2, pregnant: true, children: false, name: "PregnancyOnly"),
            CreateAssignment(3, pregnant: false, children: true, name: "ChildrenOnly"),
            CreateAssignment(4, pregnant: true, children: true, name: "Both")
        };
        var repository = new Mock<ICultureAntibioticRepository>();
        repository.Setup(x => x.GetByCultureTestIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        var result = await new GetCultureAntibioticsQueryHandler(repository.Object).Handle(
            new(12, PatientIsPregnant: true, PatientAgeYears: 30), CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 4 }, result.Select(x => x.Id));
    }

    [Fact]
    public async Task Admin_query_returns_all_active_assignments_without_patient_filter()
    {
        var assignments = new[]
        {
            CreateAssignment(1, pregnant: true, children: false, name: "PregnancyOnly"),
            CreateAssignment(2, pregnant: false, children: true, name: "ChildrenOnly"),
            CreateAssignment(3, pregnant: false, children: false, name: "Deleted", deleted: true)
        };
        var repository = new Mock<ICultureAntibioticRepository>();
        repository.Setup(x => x.GetByCultureTestIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        var result = await new GetCultureAntibioticsAdminQueryHandler(repository.Object).Handle(
            new(12), CancellationToken.None);

        Assert.Equal(new[] { 1, 2 }, result.Select(x => x.Id));
        Assert.Equal(new[] { "PregnancyOnly", "ChildrenOnly" }, result.SelectMany(x => x.CommercialNames).Select(x => x.Name));
    }

    private static CultureAntibiotic CreateAssignment(
        int id,
        bool pregnant,
        bool children,
        string name,
        bool deleted = false)
    {
        var assignment = CultureAntibiotic.Create(12, 7, "S", pregnant, children);
        assignment.Id = id;
        assignment.IsDeleted = deleted;
        assignment.Antibiotic = new Antibiotic
        {
            Id = 7,
            Name = "AMX",
            ScientificName = "Amoxicillin"
        };
        assignment.CommercialNames.Add(new CultureAntibioticCommercialName
        {
            Id = id,
            CultureAntibioticId = id,
            Name = name,
            Print = true
        });
        return assignment;
    }
}
