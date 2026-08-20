using System;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

[Obsolete("Use RenameTestGroupCommand instead. This will be removed in a future version.")]
public record ManageTestGroupsCommand(int TestGroupId, string NewName) : IRequest<Unit>;
