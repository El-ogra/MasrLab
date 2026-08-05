using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestWorkSheet;

public class GenerateTestWorkSheetQueryHandler : IRequestHandler<GenerateTestWorkSheetQuery, WorkSheetDto>
{
    private readonly IRepository<WorkSheet> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GenerateTestWorkSheetQueryHandler(IRepository<WorkSheet> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WorkSheetDto> Handle(GenerateTestWorkSheetQuery request, CancellationToken cancellationToken)
    {
        var workSheet = new WorkSheet
        {
            Type = WorkSheetType.Tests,
            Period = new DateRange(request.PeriodStart, request.PeriodEnd),
            TestIds = request.TestId.ToString(),
            PatientVisitIds = string.Empty
        };

        await _repository.AddAsync(workSheet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkSheetDto>(workSheet);
    }
}
