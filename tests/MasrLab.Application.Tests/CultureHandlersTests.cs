using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;
using MasrLab.Application.Features.Cultures.Commands.AddNewCulture;
using MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;
using MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;
using MasrLab.Application.Features.Cultures.Queries.GetCultureResult;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class CultureHandlersTests
{
    [Fact]
    public async Task EnterCultureResult_records_existing_culture_and_missing_culture_fails()
    {
        var repo = new Mock<ICultureRepository>(); var culture = Culture.Create(2); repo.Setup(x => x.GetWithSensitivitiesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(culture);
        var handler = new EnterCultureResultCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object);
        await handler.Handle(new(1, "E. coli", null, null, "aerobic", 20), default);
        Assert.Equal(CultureStatus.Recorded, culture.Status); Assert.Equal("E. coli", culture.OrganismA);
        repo.Setup(x => x.GetWithSensitivitiesAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Culture?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(new(2, null, null, null, "a", 0), default));
    }

    [Fact]
    public async Task AddNewCulture_persists_recorded_culture()
    {
        var repo = new Mock<IRepository<Culture>>(); Culture? saved = null; repo.Setup(x => x.AddAsync(It.IsAny<Culture>(), It.IsAny<CancellationToken>())).Callback<Culture, CancellationToken>((c, _) => saved = c);
        await new AddNewCultureCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object).Handle(new(3, "Blood", "E. coli", null, null, "aerobic", 12), default);
        Assert.NotNull(saved); Assert.Equal(3, saved!.VisitTestResultItemId); Assert.Equal(CultureStatus.Recorded, saved.Status); Assert.Equal("Blood", saved.SampleType);
    }

    [Fact]
    public async Task AddAntibioticToCulture_delegates_sensitivity_and_query_handlers_map_or_return_empty()
    {
        var service = new Mock<ICultureSensitivityService>();
        await new AddAntibioticToCultureCommandHandler(service.Object).Handle(new(1, 2, SensitivityLevel.HighlySensitive), default);
        service.Verify(x => x.RecordSensitivityAsync(1, 2, (int)SensitivityLevel.HighlySensitive, It.IsAny<CancellationToken>()), Times.Once);

        var antibiotics = new Mock<IAntibioticRepository>(); var mapper = new Mock<IMapper>(); var antibiotic = new Antibiotic { Name = "Amoxicillin" };
        mapper.Setup(x => x.Map<AntibioticDto>(antibiotic)).Returns(new AntibioticDto { Name = "Amoxicillin" });
        antibiotics.Setup(x => x.SearchByNameAsync("amox", It.IsAny<CancellationToken>())).ReturnsAsync(new[] { antibiotic });
        var filtered = await new FilterAntibioticsQueryHandler(antibiotics.Object, mapper.Object).Handle(new("amox"), default);
        Assert.Single(filtered); Assert.Equal("Amoxicillin", filtered[0].Name);
        antibiotics.Setup(x => x.SearchByNameAsync("none", It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Antibiotic>());
        Assert.Empty(await new FilterAntibioticsQueryHandler(antibiotics.Object, mapper.Object).Handle(new("none"), default));
    }

    [Fact]
    public async Task GetCultureResult_maps_existing_culture_and_missing_culture_fails()
    {
        var repo = new Mock<ICultureRepository>(); var mapper = new Mock<IMapper>(); var culture = Culture.Create(1); mapper.Setup(x => x.Map<CultureResultDto>(culture)).Returns(new CultureResultDto { SampleType = "Blood" });
        repo.Setup(x => x.GetWithSensitivitiesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(culture);
        var handler = new GetCultureResultQueryHandler(repo.Object, mapper.Object);
        var result = await handler.Handle(new(1), default);
        Assert.NotNull(result); Assert.Equal("Blood", result!.SampleType);
        repo.Setup(x => x.GetWithSensitivitiesAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Culture?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(new(2), default));
    }
}
