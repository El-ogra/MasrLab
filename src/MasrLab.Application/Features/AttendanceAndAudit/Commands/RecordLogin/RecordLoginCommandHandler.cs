using MediatR;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;

public class RecordLoginCommandHandler : IRequestHandler<RecordLoginCommand, Unit>
{
    private readonly IRepository<AttendanceLog> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordLoginCommandHandler(IRepository<AttendanceLog> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordLoginCommand request, CancellationToken cancellationToken)
    {
        var attendanceLog = new AttendanceLog
        {
            UserId = request.UserId,
            WorkPeriod = new DateRange(request.LoginTime, request.LoginTime)
        };

        await _repository.AddAsync(attendanceLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
