using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.WorkSheets.Queries.GeneratePatientWorkSheet;

public class GeneratePatientWorkSheetQueryHandler : IRequestHandler<GeneratePatientWorkSheetQuery, WorkSheetDto>
{
    private readonly IRepository<WorkSheet> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GeneratePatientWorkSheetQueryHandler(IRepository<WorkSheet> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkSheetDto> Handle(GeneratePatientWorkSheetQuery request, CancellationToken cancellationToken)
    {
        var workSheet = new WorkSheet
        {
            Type = WorkSheetType.Patients,
            Period = new DateRange(request.PeriodStart, request.PeriodEnd),
            PatientVisitIds = request.PatientId.ToString(),
            TestIds = string.Empty
        };

        await _repository.AddAsync(workSheet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkSheetDto
        {
            Id = workSheet.Id,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Type = workSheet.Type,
            PatientVisitIds = workSheet.PatientVisitIds,
            TestIds = workSheet.TestIds
        };
    }
}
