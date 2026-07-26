using MediatR;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;

public record GenerateTestLogQuery : IRequest<IReadOnlyList<object>>;
