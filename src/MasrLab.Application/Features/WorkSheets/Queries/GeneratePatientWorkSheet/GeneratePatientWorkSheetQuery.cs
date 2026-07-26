using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.WorkSheets.Queries.GeneratePatientWorkSheet;

public record GeneratePatientWorkSheetQuery : IRequest<WorkSheetDto>;
