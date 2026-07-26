using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
}
