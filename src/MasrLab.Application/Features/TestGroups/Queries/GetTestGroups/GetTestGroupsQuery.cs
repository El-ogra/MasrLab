using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;

public record GetTestGroupsQuery : IRequest<IReadOnlyList<TestGroupDto>>;
