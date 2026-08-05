using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogout;

public class RecordLogoutCommandHandler : IRequestHandler<RecordLogoutCommand, Unit>
{
    private readonly IRepository<AttendanceLog> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordLogoutCommandHandler(IRepository<AttendanceLog> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordLogoutCommand request, CancellationToken cancellationToken)
    {
        var attendanceLog = await _repository.GetByIdAsync(request.AttendanceLogId, cancellationToken);
        if (attendanceLog is null)
            throw new InvalidOperationException($"AttendanceLog with Id {request.AttendanceLogId} not found.");

        attendanceLog.WorkPeriod = new DateRange(attendanceLog.WorkPeriod.Start, request.LogoutTime);

        var fivePm = request.LogoutTime.Date.AddHours(17);
        if (request.LogoutTime > fivePm)
        {
            attendanceLog.Overtime = request.LogoutTime - fivePm;
        }

        _repository.Update(attendanceLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
