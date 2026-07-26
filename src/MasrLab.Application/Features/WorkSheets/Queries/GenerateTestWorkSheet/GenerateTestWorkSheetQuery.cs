using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestWorkSheet;

public record GenerateTestWorkSheetQuery(int TestId, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<WorkSheetDto>;
