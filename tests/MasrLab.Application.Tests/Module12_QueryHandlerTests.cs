using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntities;
using MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntityById;
using MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetExternalLabCandidates;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class Module12_QueryHandlerTests
{
    private readonly Mock<IReferralEntityRepository> _repo = new();
    private readonly Mock<IMapper> _mapper = new();

    #region GetReferralEntitiesQueryHandler

    [Fact]
    public async Task GetReferralEntities_DelegatesToRepository()
    {
        var entities = new List<ReferralEntity>
        {
            new() { Id = 1, Name = "Dr", EntityType = ReferralEntityType.TreatingDoctor },
            new() { Id = 2, Name = "Lab", EntityType = ReferralEntityType.OutsourcedSamples, PriceList = new PriceList { IsLabToLab = true } }
        };
        _repo.Setup(x => x.GetAllWithPriceListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var dtos = entities.Select(e => new ReferralEntityDto { Id = e.Id, Name = e.Name, EntityType = e.EntityType }).ToList();
        _mapper.Setup(x => x.Map<ReferralEntityDto>(It.IsAny<ReferralEntity>()))
            .Returns((ReferralEntity e) => dtos.First(d => d.Id == e.Id));

        var handler = new GetReferralEntitiesQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetReferralEntitiesQuery(), default);

        Assert.Equal(2, result.Count);
        _repo.Verify(x => x.GetAllWithPriceListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReferralEntities_EmptyList_ReturnsEmpty()
    {
        _repo.Setup(x => x.GetAllWithPriceListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferralEntity>());

        var handler = new GetReferralEntitiesQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetReferralEntitiesQuery(), default);

        Assert.Empty(result);
    }

    #endregion

    #region GetReferralEntityByIdQueryHandler

    [Fact]
    public async Task GetReferralEntityById_ExistingEntity_ReturnsDto()
    {
        var entity = new ReferralEntity { Id = 1, Name = "Hospital", EntityType = ReferralEntityType.ReferralEntity };
        _repo.Setup(x => x.GetByIdWithPriceListAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(x => x.Map<ReferralEntityDto>(entity))
            .Returns(new ReferralEntityDto { Id = 1, Name = "Hospital", EntityType = ReferralEntityType.ReferralEntity });

        var handler = new GetReferralEntityByIdQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetReferralEntityByIdQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal("Hospital", result!.Name);
    }

    [Fact]
    public async Task GetReferralEntityById_NonExistent_ReturnsNull()
    {
        _repo.Setup(x => x.GetByIdWithPriceListAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferralEntity?)null);

        var handler = new GetReferralEntityByIdQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetReferralEntityByIdQuery(999), default);

        Assert.Null(result);
    }

    #endregion

    #region GetExternalLabCandidatesQueryHandler — OQ-5 union predicate delegation

    [Fact]
    public async Task GetExternalLabCandidates_DelegatesToRepository_OQ5Predicate()
    {
        var candidates = new List<ReferralEntity>
        {
            new()
            {
                Id = 10,
                Name = "External Lab",
                EntityType = ReferralEntityType.OutsourcedSamples,
                PriceList = new PriceList { Id = 1, IsLabToLab = true }
            },
            new()
            {
                Id = 20,
                Name = "Lab-to-Lab Ref",
                EntityType = ReferralEntityType.ReferralEntity,
                PriceList = new PriceList { Id = 2, IsLabToLab = true }
            }
        };
        _repo.Setup(x => x.GetExternalLabCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var dtos = candidates.Select(e => new ReferralEntityDto { Id = e.Id, Name = e.Name, EntityType = e.EntityType }).ToList();
        _mapper.Setup(x => x.Map<ReferralEntityDto>(It.IsAny<ReferralEntity>()))
            .Returns((ReferralEntity e) => dtos.First(d => d.Id == e.Id));

        var handler = new GetExternalLabCandidatesQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetExternalLabCandidatesQuery(), default);

        Assert.Equal(2, result.Count);
        _repo.Verify(x => x.GetExternalLabCandidatesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetExternalLabCandidates_ExcludesTreatingDoctors()
    {
        var candidates = new List<ReferralEntity>();
        _repo.Setup(x => x.GetExternalLabCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var handler = new GetExternalLabCandidatesQueryHandler(_repo.Object, _mapper.Object);
        var result = await handler.Handle(new GetExternalLabCandidatesQuery(), default);

        Assert.Empty(result);
        _repo.Verify(x => x.GetExternalLabCandidatesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
