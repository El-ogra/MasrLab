Module 11 — Slices 6 & 7 Combined Execution Plan
Custom Groups: Feature Triads + Print Query
Verified Starting State
Area / artifact	Current verified state	Slice 6/7 action
ManageTestGroupsCommand	Exists at src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/. Monolithic: accepts (int? Id, string GroupName, string TestIds). Creates/updates group and replaces all items in one shot. Items created with default Price = 0.	Delete (command, handler, validator). Replace with per-action triads.
ManageTestGroupsCommandHandler	Uses IRepository<TestGroup> + ITestGroupItemRepository + IUnitOfWork. On update: hard-deletes all existing items, re-inserts from CSV. On create: creates group, saves, then creates items.	Delete.
ManageTestGroupsCommandValidator	Validates GroupName not empty, TestIds not empty.	Delete.
TestGroupDto	src/MasrLab.Application/Common/DTOs/TestGroupDto.cs. Has Id, GroupName only. Not consumed by any code.	Modify — add ItemCount, TotalPrice, Items.
TestGroupItemDto	Has Id, TestGroupId, TestId, Price. Not consumed by any code.	Modify — add TestName, DisplayOrder.
ITestGroupRepository	Does not exist. TestGroup is accessed only via generic IRepository<TestGroup>.	Create — new interface extending IRepository<TestGroup>.
TestGroupRepository	Does not exist.	Create — implementation with Include(TestGroupItems).
AddTestsToVisitCommandHandler.ResolveSelectionGroupTestsAsync	Reads group items via _groupItemRepository.GetByTestGroupIdAsync(), returns ordered TestId list.	No modification — this is Slice 8's responsibility. Explicit boundary.
TestGroup entity	Has GroupName, [NotMapped] TotalGroupPrice, ICollection<TestGroupItem> TestGroupItems.	No change.
TestGroupItem entity	Has TestGroupId, TestId, Price, DisplayOrder. No navigation to Test.	No change.
TestGroupItemConfiguration	Has filtered unique index (TestGroupId, TestId) WHERE IsDeleted = 0. Explicit FKs to TestGroup (cascade) and Test (restrict).	No change.
TestGroupConfiguration	Maps GroupName, indexes on GroupName and IsDeleted.	No change.
Presentation layer	TestGroupsWindow is a placeholder ("not implemented yet"). Both ViewModels are empty shells. No code references ManageTestGroupsCommand.	No modification in this slice (out of scope).
Migration	Latest: 20260820120000_Slice4PriceListItemUniquenessAndForeignKeys. Slice 5 already applied 20260820094142_Slice5CustomGroupsSchemaUplift.	No migration needed. Schema is already correct from Slice 5.
Existing tests for ManageTestGroupsCommandHandler	Two tests in DoctorsReferralsAndTestsMasterDataHandlersTests.cs (lines 226–239): happy path + not-found path.	Remove these two tests.
Discrepancies found vs. Docs/Business Logic of Module 11.md
None. The Slice 6/7 scope aligns with the current codebase state.

Migration confirmation
No migration is required for either Slice 6 or Slice 7. The schema already has Price on TestGroupItems, the filtered unique index, and the explicit FKs — all added by Slice 5. Slice 6 and 7 only add Application/Infrastructure layer code (commands, queries, repository, DTOs).

Slice 6 — Custom Groups: Per-Action Feature Triads
Overview
Replace the monolithic ManageTestGroupsCommand with six focused command/query triads:

Group-level: AddTestGroup, RenameTestGroup, DeleteTestGroup
Item-level: AddTestToGroup, UpdateTestInGroup, RemoveTestFromGroup
Read: GetTestGroups, GetTestGroupById
Introduce ITestGroupRepository / TestGroupRepository for loading groups with their items.

Exact production changes
1. DELETE — ManageTestGroups/ folder (3 files)
Delete the entire folder src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/ containing:

ManageTestGroupsCommand.cs
ManageTestGroupsCommandHandler.cs
ManageTestGroupsCommandValidator.cs
Cross-layer impact: ManageTestGroupsCommand is NOT referenced by the Presentation layer (confirmed — zero hits). The only non-self reference is in tests/MasrLab.Application.Tests/DoctorsReferralsAndTestsMasterDataHandlersTests.cs (lines 226–239), which must also be removed (see test changes below).

2. NEW — src/MasrLab.Domain/Interfaces/ITestGroupRepository.cs
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestGroupRepository : IRepository<TestGroup>
{
    Task<IReadOnlyList<TestGroup>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
    Task<TestGroup?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
}
3. NEW — src/MasrLab.Infrastructure/Persistence/Repositories/TestGroupRepository.cs
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestGroupRepository : GenericRepository<TestGroup>, ITestGroupRepository
{
    public TestGroupRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestGroup>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TestGroups
            .AsNoTracking()
            .Include(g => g.TestGroupItems)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestGroup?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TestGroups
            .AsNoTracking()
            .Include(g => g.TestGroupItems)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}
4. MODIFY — src/MasrLab.Infrastructure/DependencyInjection.cs
Add after line 87 (ITestGroupItemRepository):

services.AddScoped<ITestGroupRepository, TestGroupRepository>();
5. MODIFY — src/MasrLab.Application/Common/DTOs/TestGroupDto.cs
Replace entire file:

namespace MasrLab.Application.Common.DTOs;

public record TestGroupDto
{
    public int Id { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal TotalPrice { get; init; }
    public IReadOnlyList<TestGroupItemDto> Items { get; init; } = Array.Empty<TestGroupItemDto>();
}
6. MODIFY — src/MasrLab.Application/Common/DTOs/TestGroupItemDto.cs
Replace entire file:

namespace MasrLab.Application.Common.DTOs;

public record TestGroupItemDto
{
    public int Id { get; init; }
    public int TestGroupId { get; init; }
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DisplayOrder { get; init; }
}
7. NEW — Command: AddTestGroup
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public record AddTestGroupCommand(string GroupName) : IRequest<int>;
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public class AddTestGroupCommandHandler : IRequestHandler<AddTestGroupCommand, int>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestGroupCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new TestGroup
        {
            GroupName = request.GroupName
        };

        await _repository.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Id;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public class AddTestGroupCommandValidator : AbstractValidator<AddTestGroupCommand>
{
    public AddTestGroupCommandValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty();
    }
}
8. NEW — Command: RenameTestGroup
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public record RenameTestGroupCommand(int Id, string GroupName) : IRequest<Unit>;
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public class RenameTestGroupCommandHandler : IRequestHandler<RenameTestGroupCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameTestGroupCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RenameTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.Id);

        group.GroupName = request.GroupName;
        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public class RenameTestGroupCommandValidator : AbstractValidator<RenameTestGroupCommand>
{
    public RenameTestGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.GroupName).NotEmpty();
    }
}
9. NEW — Command: DeleteTestGroup
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public record DeleteTestGroupCommand(int Id) : IRequest<Unit>;
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public class DeleteTestGroupCommandHandler : IRequestHandler<DeleteTestGroupCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestGroupCommandHandler(ITestGroupRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.Id);

        group.IsDeleted = true;
        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public class DeleteTestGroupCommandValidator : AbstractValidator<DeleteTestGroupCommand>
{
    public DeleteTestGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
10. NEW — Command: AddTestToGroup
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public record AddTestToGroupCommand(int TestGroupId, int TestId, decimal Price) : IRequest<int>;
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public class AddTestToGroupCommandHandler : IRequestHandler<AddTestToGroupCommand, int>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly ITestGroupItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestToGroupCommandHandler(
        ITestGroupRepository groupRepository,
        ITestGroupItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddTestToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.TestGroupId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId);

        var existingItems = await _itemRepository.GetByTestGroupIdAsync(request.TestGroupId, cancellationToken);
        var nextOrder = existingItems.Count > 0 ? existingItems.Max(i => i.DisplayOrder) + 1 : 1;

        var item = new TestGroupItem
        {
            TestGroupId = request.TestGroupId,
            TestId = request.TestId,
            Price = request.Price,
            DisplayOrder = nextOrder
        };

        await _itemRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public class AddTestToGroupCommandValidator : AbstractValidator<AddTestToGroupCommand>
{
    public AddTestToGroupCommandValidator()
    {
        RuleFor(x => x.TestGroupId).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
11. NEW — Command: UpdateTestInGroup
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public record UpdateTestInGroupCommand(int Id, decimal Price) : IRequest<Unit>;
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public class UpdateTestInGroupCommandHandler : IRequestHandler<UpdateTestInGroupCommand, Unit>
{
    private readonly IRepository<TestGroupItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTestInGroupCommandHandler(IRepository<TestGroupItem> itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateTestInGroupCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroupItem), request.Id);

        item.Price = request.Price;
        _itemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public class UpdateTestInGroupCommandValidator : AbstractValidator<UpdateTestInGroupCommand>
{
    public UpdateTestInGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
12. NEW — Command: RemoveTestFromGroup
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommand.cs

using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public record RemoveTestFromGroupCommand(int Id) : IRequest<Unit>;
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommandHandler.cs

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public class RemoveTestFromGroupCommandHandler : IRequestHandler<RemoveTestFromGroupCommand, Unit>
{
    private readonly IRepository<TestGroupItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTestFromGroupCommandHandler(IRepository<TestGroupItem> itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveTestFromGroupCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroupItem), request.Id);

        _itemRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommandValidator.cs

using FluentValidation;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public class RemoveTestFromGroupCommandValidator : AbstractValidator<RemoveTestFromGroupCommand>
{
    public RemoveTestFromGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
13. NEW — Query: GetTestGroups
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroups/GetTestGroupsQuery.cs

using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;

public record GetTestGroupsQuery : IRequest<IReadOnlyList<TestGroupDto>>;
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroups/GetTestGroupsQueryHandler.cs

using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;

public class GetTestGroupsQueryHandler : IRequestHandler<GetTestGroupsQuery, IReadOnlyList<TestGroupDto>>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;

    public GetTestGroupsQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
    }

    public async Task<IReadOnlyList<TestGroupDto>> Handle(GetTestGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllWithItemsAsync(cancellationToken);
        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return groups.Select(g => new TestGroupDto
        {
            Id = g.Id,
            GroupName = g.GroupName,
            ItemCount = g.TestGroupItems.Count,
            TotalPrice = g.TestGroupItems.Sum(i => i.Price),
            Items = g.TestGroupItems
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new TestGroupItemDto
                {
                    Id = i.Id,
                    TestGroupId = i.TestGroupId,
                    TestId = i.TestId,
                    TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                    Price = i.Price,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
        }).ToList();
    }
}
14. NEW — Query: GetTestGroupById
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupById/GetTestGroupByIdQuery.cs

using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;

public record GetTestGroupByIdQuery(int Id) : IRequest<TestGroupDto?>;
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupById/GetTestGroupByIdQueryHandler.cs

using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;

public class GetTestGroupByIdQueryHandler : IRequestHandler<GetTestGroupByIdQuery, TestGroupDto?>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;

    public GetTestGroupByIdQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
    }

    public async Task<TestGroupDto?> Handle(GetTestGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (group is null)
            return null;

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return new TestGroupDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            ItemCount = group.TestGroupItems.Count,
            TotalPrice = group.TestGroupItems.Sum(i => i.Price),
            Items = group.TestGroupItems
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new TestGroupItemDto
                {
                    Id = i.Id,
                    TestGroupId = i.TestGroupId,
                    TestId = i.TestId,
                    TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                    Price = i.Price,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
        };
    }
}
Slice 6 — Test changes
MODIFY — tests/MasrLab.Application.Tests/DoctorsReferralsAndTestsMasterDataHandlersTests.cs
Remove lines 226–239 (the two ManageTestGroups_* tests) and remove the using MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups; directive at line 7. No other changes to this file.

NEW — tests/MasrLab.Application.Tests/TestGroupCommandHandlerTests.cs
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupCommandHandlerTests
{
    private readonly Mock<ITestGroupRepository> _groupRepo = new();
    private readonly Mock<ITestGroupItemRepository> _itemRepo = new();
    private readonly Mock<IRepository<TestGroupItem>> _genericItemRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    // --- AddTestGroup ---
    [Fact]
    public async Task AddTestGroup_CreatesGroup_ReturnsId()
    {
        TestGroup? saved = null;
        _groupRepo.Setup(x => x.AddAsync(It.IsAny<TestGroup>(), It.IsAny<CancellationToken>()))
            .Callback<TestGroup, CancellationToken>((g, _) => { g.Id = 42; saved = g; });

        var handler = new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommand("Checkup"), default);

        Assert.Equal(42, result);
        Assert.NotNull(saved);
        Assert.Equal("Checkup", saved!.GroupName);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- RenameTestGroup ---
    [Fact]
    public async Task RenameTestGroup_UpdatesGroupName()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        _groupRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(5, "New"), default);

        Assert.Equal("New", group.GroupName);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task RenameTestGroup_ThrowsWhenNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(99, "X"), default));
    }

    // --- DeleteTestGroup ---
    [Fact]
    public async Task DeleteTestGroup_SoftDeletes()
    {
        var group = new TestGroup { Id = 7, GroupName = "ToDelete" };
        _groupRepo.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(7), default);

        Assert.True(group.IsDeleted);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task DeleteTestGroup_ThrowsWhenNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(99), default));
    }

    // --- AddTestToGroup ---
    [Fact]
    public async Task AddTestToGroup_CreatesItemWithNextDisplayOrder()
    {
        var group = new TestGroup { Id = 1 };
        _groupRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { DisplayOrder = 1 },
                new() { DisplayOrder = 2 }
            });

        TestGroupItem? saved = null;
        _itemRepo.Setup(x => x.AddAsync(It.IsAny<TestGroupItem>(), It.IsAny<CancellationToken>()))
            .Callback<TestGroupItem, CancellationToken>((i, _) => { i.Id = 100; saved = i; });

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(1, 55, 25m), default);

        Assert.Equal(100, result);
        Assert.NotNull(saved);
        Assert.Equal(3, saved!.DisplayOrder);
        Assert.Equal(25m, saved.Price);
    }

    [Fact]
    public async Task AddTestToGroup_ThrowsWhenGroupNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(99, 1, 10m), default));
    }

    // --- UpdateTestInGroup ---
    [Fact]
    public async Task UpdateTestInGroup_UpdatesPrice()
    {
        var item = new TestGroupItem { Id = 3, Price = 10m };
        _genericItemRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(3, 50m), default);

        Assert.Equal(50m, item.Price);
    }

    [Fact]
    public async Task UpdateTestInGroup_ThrowsWhenNotFound()
    {
        _genericItemRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroupItem?)null);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(99, 10m), default));
    }

    // --- RemoveTestFromGroup ---
    [Fact]
    public async Task RemoveTestFromGroup_DeletesItem()
    {
        var item = new TestGroupItem { Id = 4 };
        _genericItemRepo.Setup(x => x.GetByIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommand(4), default);

        _genericItemRepo.Verify(x => x.Delete(item), Times.Once);
    }

    [Fact]
    public async Task RemoveTestFromGroup_ThrowsWhenNotFound()
    {
        _genericItemRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroupItem?)null);

        var handler = new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommand(99), default));
    }
}
NEW — tests/MasrLab.Application.Tests/TestGroupValidatorTests.cs
using MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;
using MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;
using MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

namespace MasrLab.Application.Tests;

public class TestGroupValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddTestGroup_InvalidName_Fails(string name)
    {
        var v = new AddTestGroupCommandValidator();
        var result = v.Validate(new AddTestGroupCommand(name));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestGroup_ValidName_Passes()
    {
        var v = new AddTestGroupCommandValidator();
        var result = v.Validate(new AddTestGroupCommand("Checkup"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RenameTestGroup_ZeroId_Fails()
    {
        var v = new RenameTestGroupCommandValidator();
        var result = v.Validate(new RenameTestGroupCommand(0, "Name"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void RenameTestGroup_EmptyName_Fails(string name)
    {
        var v = new RenameTestGroupCommandValidator();
        var result = v.Validate(new RenameTestGroupCommand(1, name));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeleteTestGroup_ZeroId_Fails()
    {
        var v = new DeleteTestGroupCommandValidator();
        var result = v.Validate(new DeleteTestGroupCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestToGroup_NegativePrice_Fails()
    {
        var v = new AddTestToGroupCommandValidator();
        var result = v.Validate(new AddTestToGroupCommand(1, 1, -1m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestToGroup_Valid_Passes()
    {
        var v = new AddTestToGroupCommandValidator();
        var result = v.Validate(new AddTestToGroupCommand(1, 1, 50m));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTestInGroup_NegativePrice_Fails()
    {
        var v = new UpdateTestInGroupCommandValidator();
        var result = v.Validate(new UpdateTestInGroupCommand(1, -5m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RemoveTestFromGroup_ZeroId_Fails()
    {
        var v = new RemoveTestFromGroupCommandValidator();
        var result = v.Validate(new RemoveTestFromGroupCommand(0));
        Assert.False(result.IsValid);
    }
}
NEW — tests/MasrLab.Application.Tests/TestGroupQueryHandlerTests.cs
using MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;
using MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupQueryHandlerTests
{
    [Fact]
    public async Task GetTestGroups_ReturnsGroupsWithItemsAndTestNames()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC" };
        var test2 = new Test { Id = 20, Name = "Stool" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Checkup",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 30m, DisplayOrder = 2 }
            }
        };
        groupRepo.Setup(x => x.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroup> { group });

        var handler = new GetTestGroupsQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupsQuery(), default);

        Assert.Single(result);
        Assert.Equal("Checkup", result[0].GroupName);
        Assert.Equal(2, result[0].ItemCount);
        Assert.Equal(80m, result[0].TotalPrice);
        Assert.Equal("CBC", result[0].Items[0].TestName);
        Assert.Equal("Stool", result[0].Items[1].TestName);
    }

    [Fact]
    public async Task GetTestGroupById_ReturnsNullWhenNotFound()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new GetTestGroupByIdQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupByIdQuery(99), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTestGroupById_ReturnsGroupWithItems()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test = new Test { Id = 5, Name = "Urine" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var group = new TestGroup
        {
            Id = 3, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 50, TestGroupId = 3, TestId = 5, Price = 10m, DisplayOrder = 1 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupByIdQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupByIdQuery(3), default);

        Assert.NotNull(result);
        Assert.Equal("Panel", result!.GroupName);
        Assert.Single(result.Items);
        Assert.Equal("Urine", result.Items[0].TestName);
        Assert.Equal(10m, result.Items[0].Price);
        Assert.Equal(10m, result.TotalPrice);
    }
}
Slice 7 — Custom Groups: Print Query
Overview
Add a GetTestGroupForPrint query that produces a TestGroupPrintDto suitable for display/print, including TotalGroupPrice. Application layer only. No migration.

Exact production changes
15. NEW — src/MasrLab.Application/Common/DTOs/TestGroupPrintDto.cs
namespace MasrLab.Application.Common.DTOs;

public record TestGroupPrintDto
{
    public int Id { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public decimal TotalGroupPrice { get; init; }
    public IReadOnlyList<TestGroupItemDto> Items { get; init; } = Array.Empty<TestGroupItemDto>();
}
16. NEW — Query: GetTestGroupForPrint
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQuery.cs

using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;

public record GetTestGroupForPrintQuery(int TestGroupId) : IRequest<TestGroupPrintDto?>;
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQueryHandler.cs

using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;

public class GetTestGroupForPrintQueryHandler : IRequestHandler<GetTestGroupForPrintQuery, TestGroupPrintDto?>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;

    public GetTestGroupForPrintQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
    }

    public async Task<TestGroupPrintDto?> Handle(GetTestGroupForPrintQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithItemsAsync(request.TestGroupId, cancellationToken);
        if (group is null)
            throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId);

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return new TestGroupPrintDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            TotalGroupPrice = group.TestGroupItems.Sum(i => i.Price),
            Items = group.TestGroupItems
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new TestGroupItemDto
                {
                    Id = i.Id,
                    TestGroupId = i.TestGroupId,
                    TestId = i.TestId,
                    TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                    Price = i.Price,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
        };
    }
}
Slice 7 — Test changes
NEW — tests/MasrLab.Application.Tests/TestGroupPrintQueryHandlerTests.cs
using MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupPrintQueryHandlerTests
{
    [Fact]
    public async Task GetTestGroupForPrint_ReturnsPrintDtoWithTotalPrice()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC" };
        var test2 = new Test { Id = 20, Name = "Urine" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "RealLab",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 10m, DisplayOrder = 2 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal("RealLab", result!.GroupName);
        Assert.Equal(60m, result.TotalGroupPrice);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("CBC", result.Items[0].TestName);
        Assert.Equal(50m, result.Items[0].Price);
        Assert.Equal("Urine", result.Items[1].TestName);
        Assert.Equal(10m, result.Items[1].Price);
    }

    [Fact]
    public async Task GetTestGroupForPrint_ThrowsWhenNotFound()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new GetTestGroupForPrintQuery(99), default));
    }

    [Fact]
    public async Task GetTestGroupForPrint_EmptyGroup_ReturnsZeroTotal()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test>());

        var group = new TestGroup
        {
            Id = 5, GroupName = "Empty",
            TestGroupItems = new List<TestGroupItem>()
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(5), default);

        Assert.NotNull(result);
        Assert.Equal(0m, result!.TotalGroupPrice);
        Assert.Empty(result.Items);
    }
}
Complete file list
Create (Slice 6):
src/MasrLab.Domain/Interfaces/ITestGroupRepository.cs
src/MasrLab.Infrastructure/Persistence/Repositories/TestGroupRepository.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommandValidator.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroups/GetTestGroupsQuery.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroups/GetTestGroupsQueryHandler.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupById/GetTestGroupByIdQuery.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupById/GetTestGroupByIdQueryHandler.cs
tests/MasrLab.Application.Tests/TestGroupCommandHandlerTests.cs
tests/MasrLab.Application.Tests/TestGroupValidatorTests.cs
tests/MasrLab.Application.Tests/TestGroupQueryHandlerTests.cs
Create (Slice 7):
src/MasrLab.Application/Common/DTOs/TestGroupPrintDto.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQuery.cs
src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQueryHandler.cs
tests/MasrLab.Application.Tests/TestGroupPrintQueryHandlerTests.cs
Modify:
src/MasrLab.Infrastructure/DependencyInjection.cs (add 1 line: ITestGroupRepository registration)
src/MasrLab.Application/Common/DTOs/TestGroupDto.cs (add ItemCount, TotalPrice, Items)
src/MasrLab.Application/Common/DTOs/TestGroupItemDto.cs (add TestName, DisplayOrder)
tests/MasrLab.Application.Tests/DoctorsReferralsAndTestsMasterDataHandlersTests.cs (remove 2 ManageTestGroups tests + 1 using)
Delete:
src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/ManageTestGroupsCommand.cs
src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/ManageTestGroupsCommandHandler.cs
src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/ManageTestGroupsCommandValidator.cs
Slice 6 — Verification / Completion Gate
dotnet build MasrLab.sln — must be zero errors.
dotnet test MasrLab.sln — all unit tests pass.
Confirm no migration was generated or needed (dotnet ef migrations list shows no pending).
Confirm DIContainerResolutionTests passes (all new handlers/validators resolve from DI).
Confirm ManageTestGroupsCommand is fully removed — no compilation errors, no dangling references.
Slice 7 — Verification / Completion Gate
dotnet build MasrLab.sln — must be zero errors.
dotnet test MasrLab.sln — all unit tests pass.
Confirm GetTestGroupForPrintQueryHandler correctly throws EntityNotFoundException for missing group and returns correct DTO for existing group.
Explicit boundary: What Slice 6/7 does NOT touch
AddTestsToVisitCommandHandler.ResolveSelectionGroupTestsAsync — not modified. This method reads group items to resolve test IDs for visit attachment. Slice 8 will update it to use group-item prices. The method currently works correctly with the existing ITestGroupItemRepository.GetByTestGroupIdAsync.
Presentation layer — no ViewModels, XAML, or code-behind changes. The UI remains a placeholder.
TestGroup entity — no changes (already correct from Slice 5).
TestGroupItem entity — no changes.
EF configurations / migrations — no changes.
Slices 8–10 — not planned or executed.
Notes on already-complete items from Slices 1–5
TestGroupItem.Price — already exists from Slice 5.
TestGroup.TotalGroupPrice — already exists from Slice 5 ([NotMapped] computed property).
Filtered unique index on (TestGroupId, TestId) WHERE IsDeleted = 0 — already exists from Slice 5.
Explicit FKs TestGroupId → TestGroup (cascade) and TestId → Test (restrict) — already exist from Slice 5.
TestGroupItemDto.Price — already exists from Slice 5.
No duplication of any Slice 1–5 work in this plan.
