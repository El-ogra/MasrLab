using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.WorkSheets.Queries.GeneratePatientWorkSheet;

public record GeneratePatientWorkSheetQuery(int PatientId, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<WorkSheetDto>;
