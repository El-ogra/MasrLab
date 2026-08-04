using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;

public record GenerateTestLogQuery(int TestId, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<IReadOnlyList<TestLogEntryDto>>;
