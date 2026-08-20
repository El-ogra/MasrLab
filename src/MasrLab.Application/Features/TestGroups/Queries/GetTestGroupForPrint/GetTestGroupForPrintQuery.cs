using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;

public record GetTestGroupForPrintQuery(int TestGroupId) : IRequest<TestGroupPrintDto?>;
