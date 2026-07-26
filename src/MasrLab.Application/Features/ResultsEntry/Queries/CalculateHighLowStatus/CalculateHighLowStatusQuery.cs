using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Queries.CalculateHighLowStatus;

public record CalculateHighLowStatusQuery : IRequest<ResultStatus>;
