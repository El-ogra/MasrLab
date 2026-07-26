using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Queries.CalculateHighLowStatus;

public record CalculateHighLowStatusQuery(string Value, string ReferenceRange, Gender Gender, int AgeYears, int AgeMonths, AgeUnit AgeUnit) : IRequest<ResultStatus>;
