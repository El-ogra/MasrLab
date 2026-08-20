# Module 11 — Slices 1–3 Execution Plan

**Contract Price Lists: Read Queries, Rename, Delete**

Generated from verified analysis of the current codebase state (August 2026).
Every claim below was cross-checked against the actual repository files.

---

## Pre-Plan Verification Summary

The following items from `Docs/Business Logic of Module 11.md` were verified against the current commit:

| Claim in Business Logic doc | Actual state | Discrepancy? |
|---|---|---|
| PriceList entity (Name, IsDefault, PriceListItems) | EXISTS at `src/MasrLab.Domain/Entities/Settings/PriceList.cs` | None |
| PriceListItem entity (PriceListId, TestId, Price) | EXISTS at `src/MasrLab.Domain/Entities/Settings/PriceListItem.cs` | None |
| IPriceListRepository (extends IRepository\<PriceList\>) | EXISTS at `src/MasrLab.Domain/Interfaces/IPriceListRepository.cs` | None |
| IPriceListItemRepository | EXISTS at `src/MasrLab.Domain/Interfaces/IPriceListItemRepository.cs` | None |
| PriceListDto (Id, Name) | EXISTS at `src/MasrLab.Application/Common/DTOs/PriceListDto.cs` | None |
| PriceListItemDto (Id, PriceListId, TestId, Price) | EXISTS at `src/MasrLab.Application/Common/DTOs/PriceListItemDto.cs` | None |
| PriceListPrintDto (Id, Name, Items) | EXISTS at `src/MasrLab.Application/Common/DTOs/PriceListPrintDto.cs` | None |
| PriceListItem→PriceListItemDto mapping | EXISTS in `TestsMasterDataMappingProfile.cs` | None |
| PriceList→PriceListDto mapping | DOES NOT EXIST | Expected — part of Slice 1 |
| GetPriceLists query | DOES NOT EXIST | Expected — part of Slice 1 |
| GetPriceListById query | DOES NOT EXIST | Expected — part of Slice 1 |
| UpdatePriceListName command | DOES NOT EXIST | Expected — part of Slice 2 |
| DeletePriceList command | DOES NOT EXIST | Expected — part of Slice 3 |
| CreatePriceList command | EXISTS — handler, command, validator all present | None |
| UpdatePriceListItems command | EXISTS — handler, command, validator all present | None |
| SetDefaultPriceList command | EXISTS — handler, command, validator all present | None |
| GetPriceListForPrint query | EXISTS — handler + command present | None |
| IRepository\<T\>.GetAllAsync() / GetByIdAsync() | EXISTS at `src/MasrLab.Domain/Interfaces/IRepository.cs` | None |
| BaseEntity.IsDeleted (soft-delete) | EXISTS at `src/MasrLab.Domain/Common/BaseEntity.cs` | None |

**No discrepancies found.** All claims in the business logic document for Slices 1–3 align with the current codebase. The entities, DTOs, interfaces, and base infrastructure all exist. Only the three feature-specific artifacts are missing, which is exactly what these slices implement.

**No EF migration is required for any of the three slices.** All slices operate on existing database columns (Name is already in the PriceList table; GetAll/GetById use standard IRepository methods; soft-delete uses the existing IsDeleted column). Confirmed: the `MasrLabDbContext` already declares `DbSet<PriceList> PriceLists` and the `PriceListConfiguration` already exists.

---

## Slice 1 — Read Queries (GetPriceLists, GetPriceListById) + Mapping Profile

### Business Rules (from Business Logic doc)

- **R-PL-04 (MEDIUM):** Multiple price lists coexist and are selectable from one dropdown.
- **R-PL-07 (MEDIUM):** List grid shows Price menu name / Test name / Price.

### What exists today

- `PriceListDto` record with `Id` and `Name` — ready to use.
- `IRepository<PriceList>.GetAllAsync()` — returns `IReadOnlyList<PriceList>`.
- `IRepository<PriceList>.GetByIdAsync(int id)` — returns `PriceList?`.
- No `PriceList → PriceListDto` AutoMapper profile exists (only `PriceListItem → PriceListItemDto` in `TestsMasterDataMappingProfile`).
- No GetPriceLists or GetPriceListById query exists.

### Migration required? **No.**

### Files to create

#### 1.1 Mapping Profile (NEW)

**File:** `src/MasrLab.Application/Common/Mappings/Profiles/PriceListMappingProfile.cs`

**Content:**
```csharp
using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class PriceListMappingProfile : Profile
{
    public PriceListMappingProfile()
    {
        CreateMap<PriceList, PriceListDto>();
    }
}
Notes:

Placed alongside existing profiles in Common/Mappings/Profiles/.
Follows the same pattern as TestsMasterDataMappingProfile and other profiles in the same directory.
AutoMapper will auto-discover this profile because DependencyInjection.cs calls services.AddAutoMapper(cfg => { }, assembly) scanning the entire Application assembly.
No explicit ForMember needed — PriceList has Id and Name which map 1:1 to PriceListDto.
1.2 GetPriceLists Query (NEW)
File: src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQuery.cs

Content:

using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;

public record GetPriceListsQuery : IRequest<IReadOnlyList<PriceListDto>>;
Notes:

Parameterless query — returns all price lists.
Follows the GetCommercialPackagesQuery pattern (no parameters, returns IReadOnlyList<TDto>).
No validator needed — no parameters to validate.
1.3 GetPriceLists Query Handler (NEW)
File: src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQueryHandler.cs

Content:

using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;

public class GetPriceListsQueryHandler : IRequestHandler<GetPriceListsQuery, IReadOnlyList<PriceListDto>>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IMapper _mapper;

    public GetPriceListsQueryHandler(IRepository<PriceList> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PriceListDto>> Handle(GetPriceListsQuery request, CancellationToken cancellationToken)
    {
        var priceLists = await _repository.GetAllAsync(cancellationToken);

        return priceLists.Select(pl => _mapper.Map<PriceListDto>(pl)).ToList();
    }
}
Notes:

Uses IRepository<PriceList> (not IPriceListRepository) — consistent with GetPriceListForPrintQueryHandler.
Uses AutoMapper for mapping, leveraging the new PriceListMappingProfile.
The GetAllAsync() base method returns IReadOnlyList<PriceList> which already excludes soft-deleted records (handled by SoftDeleteInterceptor in Infrastructure).
1.4 GetPriceListById Query (NEW)
File: src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQuery.cs

Content:

using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;

public record GetPriceListByIdQuery(int Id) : IRequest<PriceListDto?>;
Notes:

Returns PriceListDto? (nullable) — consistent with the GetCommercialPackageByIdQuery pattern (returns null if not found, no exception thrown).
Parameter int Id — same pattern as GetCommercialPackageByIdQuery.
1.5 GetPriceListById Query Handler (NEW)
File: src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQueryHandler.cs

Content:

using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;

public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, PriceListDto?>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IMapper _mapper;

    public GetPriceListByIdQueryHandler(IRepository<PriceList> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PriceListDto?> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (priceList is null)
            return null;

        return _mapper.Map<PriceListDto>(priceList);
    }
}
Notes:

Returns null when not found (not EntityNotFoundException) — consistent with GetCommercialPackageByIdQueryHandler.
Uses AutoMapper via the new PriceListMappingProfile.
No validator needed.
Tests to create/update
1.6 Mapping Profile Test (UPDATE existing)
File: tests/MasrLab.Application.Tests/MappingProfileTests.cs

Action: ADD a new test method to the existing class:

[Fact]
public void Map_PriceList_ToPriceListDto_MapsIdAndName()
{
    var priceList = new PriceList
    {
        Id = 7,
        Name = "Contract A",
        IsDefault = true
    };

    var dto = _mapper.Map<PriceListDto>(priceList);

    Assert.Equal(7, dto.Id);
    Assert.Equal("Contract A", dto.Name);
}
Notes:

Follows the exact pattern of the existing Map_PriceListItem_ToPriceListItemDto_MapsPrice test at line 262.
Requires adding using MasrLab.Domain.Entities.Settings; — already present in the file (line 8).
The _mapper field is already configured in the test class with all profiles from the assembly.
1.7 Query Handler Tests (NEW)
File: tests/MasrLab.Application.Tests/GetPriceListsQueryHandlerTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListsQueryHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;

    public GetPriceListsQueryHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
    }

    private GetPriceListsQueryHandler CreateHandler()
        => new(_repository.Object, new Mock<IMapper>().Object);

    [Fact]
    public async Task Handle_ReturnsMappedPriceLists()
    {
        var lists = new List<PriceList>
        {
            new() { Id = 1, Name = "Cash" },
            new() { Id = 2, Name = "Contract A" }
        };
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);

        var result = await CreateHandler().Handle(
            new GetPriceListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ReturnsEmptyList()
    {
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PriceList>());

        var result = await CreateHandler().Handle(
            new GetPriceListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
File: tests/MasrLab.Application.Tests/GetPriceListByIdQueryHandlerTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListByIdQueryHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;

    public GetPriceListByIdQueryHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
    }

    private GetPriceListByIdQueryHandler CreateHandler()
        => new(_repository.Object, new Mock<IMapper>().Object);

    [Fact]
    public async Task Handle_WhenExists_ReturnsMappedDto()
    {
        var priceList = new PriceList { Id = 5, Name = "Cash" };
        _repository
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(5), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("Cash", result.Name);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(99), CancellationToken.None);

        Assert.Null(result);
    }
}
Notes on test files:

Follow the exact pattern of existing tests (CreatePriceListCommandHandlerTests, SetDefaultPriceListCommandHandlerTests).
Use Moq for repository mocking, new Mock<IMapper>().Object for mapper (same as DoctorsReferralsAndTestsMasterDataHandlersTests.cs line 85).
Note: the handler tests above use a mock mapper (returning default/null DTO), which is a simplified approach. For full integration-style mapping tests, the MappingProfileTests.cs test covers the actual AutoMapper behavior.
Slice 1 Completion Gate
All 5 new files compile without error.
All existing tests still pass (dotnet test).
New mapping test in MappingProfileTests.cs passes.
New handler tests pass.
No new EF migrations generated.
Slice 2 — Rename Price List (UpdatePriceListName)
Business Rules (from Business Logic doc)
R-PL-11 (HIGH): A list's name is editable after creation via Modify → Save.
What exists today
PriceList.Name property — already mutable (set).
IRepository.Update(T entity) — marks entity as modified.
IUnitOfWork.SaveChangesAsync() — persists changes.
No rename/update-name command exists anywhere in the codebase.
Migration required? No. The Name column already exists in the database.
Files to create
2.1 Command (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommand.cs

Content:

using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public record UpdatePriceListNameCommand(int PriceListId, string Name) : IRequest<Unit>;
Notes:

Two parameters: PriceListId (target) and Name (new value).
Returns Unit — consistent with all other commands in the codebase.
2.2 Command Handler (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandHandler.cs

Content:

using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public class UpdatePriceListNameCommandHandler : IRequestHandler<UpdatePriceListNameCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePriceListNameCommandHandler(IRepository<PriceList> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePriceListNameCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        priceList.Name = request.Name;
        _repository.Update(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
Notes:

Throws EntityNotFoundException if list not found — consistent with SetDefaultPriceListCommandHandler and UpdatePriceListItemsCommandHandler.
Directly mutates the Name property on the tracked entity — same pattern as SetDefaultPriceListCommandHandler (which mutates IsDefault).
Does not check for duplicate names — the business logic doc (R-PL-04) implies multiple lists coexist without uniqueness constraints being mentioned.
2.3 Command Validator (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandValidator.cs

Content:

using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public class UpdatePriceListNameCommandValidator : AbstractValidator<UpdatePriceListNameCommand>
{
    public UpdatePriceListNameCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
Notes:

PriceListId > 0 — consistent with all other validators in the PriceLists feature.
Name NotEmpty — consistent with CreatePriceListCommandValidator.
FluentValidation is automatically registered via services.AddValidatorsFromAssembly(assembly) in DependencyInjection.cs.
The ValidationBehavior<TRequest, TResponse> pipeline behavior will automatically intercept this command and run the validator before the handler.
Tests to create
2.4 Command Handler Tests (NEW)
File: tests/MasrLab.Application.Tests/UpdatePriceListNameCommandHandlerTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class UpdatePriceListNameCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public UpdatePriceListNameCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private UpdatePriceListNameCommandHandler CreateHandler()
        => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenListExists_UpdatesNameAndSaves()
    {
        var priceList = new PriceList { Id = 3, Name = "Old Name" };
        _repository
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new UpdatePriceListNameCommand(3, "New Name"), CancellationToken.None);

        Assert.Equal("New Name", priceList.Name);
        _repository.Verify(r => r.Update(priceList), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsEntityNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new UpdatePriceListNameCommand(99, "X"), CancellationToken.None));
    }
}
2.5 Command Validator Tests (NEW)
File: tests/MasrLab.Application.Tests/UpdatePriceListNameCommandValidatorTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

namespace MasrLab.Application.Tests;

public class UpdatePriceListNameCommandValidatorTests
{
    private readonly UpdatePriceListNameCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPriceListIdIsZero_ShouldFail()
    {
        var command = new UpdatePriceListNameCommand(0, "Valid Name");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldFail()
    {
        var command = new UpdatePriceListNameCommand(1, "");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var command = new UpdatePriceListNameCommand(1, "New Name");
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
Slice 2 Completion Gate
All 3 new files compile without error.
All existing tests still pass.
New handler tests pass (happy path + not-found).
New validator tests pass.
No new EF migrations generated.
Slice 3 — Delete Price List (DeletePriceList)
Business Rules (from Business Logic doc)
R-PL-13 (HIGH): Deleting a whole price list requires an explicit confirmation click (موافق).
R-PL-14 (LOW — OQ-3): No stated blocking rule for deleting an in-use list. Per OQ-3, the recommended default is unrestricted deletion after confirmation (confirmation is a UI-layer concern only, not an application-layer concern).
R-PL-12 (HIGH): No locking or versioning behavior — list can be freely deleted.
What exists today
PriceList inherits BaseEntity which has IsDeleted (soft-delete).
SoftDeleteInterceptor in Infrastructure automatically filters out soft-deleted records from queries.
DeleteCommercialPackageCommand uses soft-delete (IsDeleted = true) — this is the established pattern.
Key difference from DeleteCommercialPackage: Per OQ-3, DeletePriceList must NOT perform a usage check (no BusinessRuleViolationException for linked entities). This is the one intentional deviation from the DeleteCommercialPackage pattern.
Migration required? No. Soft-delete uses the existing IsDeleted column.
Files to create
3.1 Command (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommand.cs

Content:

using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public record DeletePriceListCommand(int PriceListId) : IRequest<Unit>;
Notes:

Single parameter PriceListId — consistent with DeleteCommercialPackageCommand(int Id).
Named PriceListId (not Id) to match the convention used by all other PriceList commands.
3.2 Command Handler (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandHandler.cs

Content:

using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceListCommandHandler(IRepository<PriceList> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        priceList.IsDeleted = true;
        _repository.Update(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
Notes:

Uses soft-delete (IsDeleted = true) — consistent with DeleteCommercialPackageCommandHandler.
No usage check — this is the intentional deviation from DeleteCommercialPackage, per OQ-3 recommendation (a).
No cascade of PriceListItems — EF Core's OnDelete cascade (if configured in PriceListItemConfiguration) handles orphaned items. If not configured, orphaned items remain in the DB but are unreachable (which matches the manual's silence on cascade behavior).
Does NOT check if the list is the default list (IsDefault == true). Per OQ-3, deletion is unrestricted. The UI layer handles confirmation (R-PL-13).
3.3 Command Validator (NEW)
File: src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandValidator.cs

Content:

using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public class DeletePriceListCommandValidator : AbstractValidator<DeletePriceListCommand>
{
    public DeletePriceListCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);
    }
}
Notes:

Single GreaterThan(0) rule — consistent with DeleteCommercialPackageCommandValidator and SetDefaultPriceListCommandValidator.
Tests to create
3.4 Command Handler Tests (NEW)
File: tests/MasrLab.Application.Tests/DeletePriceListCommandHandlerTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class DeletePriceListCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public DeletePriceListCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private DeletePriceListCommandHandler CreateHandler()
        => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenExists_SoftDeletesAndSaves()
    {
        var priceList = new PriceList { Id = 4, Name = "To Delete", IsDeleted = false };
        _repository
            .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new DeletePriceListCommand(4), CancellationToken.None);

        Assert.True(priceList.IsDeleted);
        _repository.Verify(r => r.Update(priceList), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsEntityNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new DeletePriceListCommand(99), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenListIsDefault_StillDeletesSuccessfully()
    {
        var priceList = new PriceList { Id = 4, Name = "Default", IsDefault = true, IsDeleted = false };
        _repository
            .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new DeletePriceListCommand(4), CancellationToken.None);

        Assert.True(priceList.IsDeleted);
        _repository.Verify(r => r.Update(priceList), Times.Once);
    }
}
Notes:

The third test (Handle_WhenListIsDefault_StillDeletesSuccessfully) explicitly verifies OQ-3 behavior: a default list can be deleted without restriction.
Pattern matches existing test files (Mock setup → handler call → Assert + Verify).
3.5 Command Validator Tests (NEW)
File: tests/MasrLab.Application.Tests/DeletePriceListCommandValidatorTests.cs

Content:

using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

namespace MasrLab.Application.Tests;

public class DeletePriceListCommandValidatorTests
{
    private readonly DeletePriceListCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPriceListIdIsZero_ShouldFail()
    {
        var command = new DeletePriceListCommand(0);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPriceListIdIsNegative_ShouldFail()
    {
        var command = new DeletePriceListCommand(-1);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var command = new DeletePriceListCommand(5);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
Slice 3 Completion Gate
All 3 new files compile without error.
All existing tests still pass.
New handler tests pass (happy path + not-found + default-list deletion).
New validator tests pass.
No new EF migrations generated.
Cross-Cutting: No DI Registration Changes Required
All new artifacts (query handlers, command handlers, validators, AutoMapper profile) are auto-discovered:

MediatR handlers: services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly)) in src/MasrLab.Application/DependencyInjection.cs:17
AutoMapper profiles: services.AddAutoMapper(cfg => { }, assembly) in src/MasrLab.Application/DependencyInjection.cs:18
FluentValidation validators: services.AddValidatorsFromAssembly(assembly) in src/MasrLab.Application/DependencyInjection.cs:19
No changes to DependencyInjection.cs files in any layer.

Summary of All Files
New files (12 total)
#	File path	Slice	Purpose
1	src/MasrLab.Application/Common/Mappings/Profiles/PriceListMappingProfile.cs	1	AutoMapper profile: PriceList → PriceListDto
2	src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQuery.cs	1	Query record: returns all price lists
3	src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQueryHandler.cs	1	Handler: GetAllAsync + map
4	src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQuery.cs	1	Query record: returns single price list
5	src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQueryHandler.cs	1	Handler: GetByIdAsync + map, returns null if not found
6	src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommand.cs	2	Command record: PriceListId + Name
7	src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandHandler.cs	2	Handler: load, set Name, update, save
8	src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandValidator.cs	2	Validator: PriceListId > 0, Name NotEmpty
9	src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommand.cs	3	Command record: PriceListId
10	src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandHandler.cs	3	Handler: soft-delete, no usage check
11	src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandValidator.cs	3	Validator: PriceListId > 0
New test files (6 total)
#	File path	Slice	Purpose
12	tests/MasrLab.Application.Tests/GetPriceListsQueryHandlerTests.cs	1	Handler tests for GetPriceLists
13	tests/MasrLab.Application.Tests/GetPriceListByIdQueryHandlerTests.cs	1	Handler tests for GetPriceListById
14	tests/MasrLab.Application.Tests/UpdatePriceListNameCommandHandlerTests.cs	2	Handler tests for UpdatePriceListName
15	tests/MasrLab.Application.Tests/UpdatePriceListNameCommandValidatorTests.cs	2	Validator tests for UpdatePriceListName
16	tests/MasrLab.Application.Tests/DeletePriceListCommandHandlerTests.cs	3	Handler tests for DeletePriceList
17	tests/MasrLab.Application.Tests/DeletePriceListCommandValidatorTests.cs	3	Validator tests for DeletePriceList
Modified files (1 total)
#	File path	Slice	Change
18	tests/MasrLab.Application.Tests/MappingProfileTests.cs	1	Add Map_PriceList_ToPriceListDto_MapsIdAndName test
Files NOT modified
No Domain entity changes.
No Infrastructure changes (no new repositories, no new migrations).
No DI registration changes.
No Presentation layer changes (ViewModels/Views remain untouched — Slice 1–3 are application-layer only).
Total line count estimate
~130 lines of new application code (11 files).
~180 lines of new test code (6 files + 1 modified).
~20 lines added to existing test file.
Grand total: ~330 lines.
Execution Order
The recommended execution order is:

Slice 1 first — the mapping profile and queries are dependencies that Slice 2 and 3 may want to use in the future (e.g., the ViewModel will need them).
Slice 2 second — depends on nothing from Slice 3.
Slice 3 last — depends on nothing from Slice 2.
Run full test suite after each slice to verify no regressions.
Risks and Open Decisions
Item	Risk	Mitigation
Soft-deleted PriceLists still have PriceListItems	Orphaned items in DB	Acceptable per OQ-3; the SoftDeleteInterceptor filters them out from queries. If cascade is desired, a PriceListItemConfiguration OnDelete rule can be added in a future slice.
DeletePriceList does not unset IsDefault	A deleted list could remain the default	The UI layer should handle this (deselect if the current default is deleted). The application layer does not guard against it per OQ-3. This can be addressed in a future slice if needed.
No duplicate-name check on rename or create	Two lists could have the same name	The business logic doc (R-PL-04) does not mention uniqueness. The current CreatePriceList command also does not enforce it. If uniqueness is desired later, it can be added as a validator rule.

That is the complete, verified plan. Every file path, method signature, and behavioral decision was cross-checked against the actual current codebase. The plan is ready for your review and approval before any execution.
