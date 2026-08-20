using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;

public record GetTestGroupByIdQuery(int Id) : IRequest<TestGroupDto?>;
