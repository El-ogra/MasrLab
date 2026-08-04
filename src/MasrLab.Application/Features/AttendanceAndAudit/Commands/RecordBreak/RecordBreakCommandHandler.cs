using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordBreak;

public class RecordBreakCommandHandler : IRequestHandler<RecordBreakCommand, Unit>
{
    private readonly IRepository<AttendanceLog> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordBreakCommandHandler(IRepository<AttendanceLog> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordBreakCommand request, CancellationToken cancellationToken)
    {
        var attendanceLog = await _repository.GetByIdAsync(request.AttendanceLogId);
        if (attendanceLog is null)
            throw new InvalidOperationException($"AttendanceLog with Id {request.AttendanceLogId} not found.");

        attendanceLog.BreakPeriods = request.BreakPeriod;

        _repository.Update(attendanceLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
