using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;

// OQ-M4-9: defaults to today with a date picker for any previous day.
// OQ-M4-10: categories come from registration-time Patient.AccountType — never auto-detected.
public sealed record GetResultWorklistQuery(DateTime? Date = null, AccountType? Category = null)
    : IRequest<IReadOnlyList<WorklistPatientDto>>;

public sealed class GetResultWorklistQueryHandler : IRequestHandler<GetResultWorklistQuery, IReadOnlyList<WorklistPatientDto>>
{
    private readonly IWorklistReader _reader;
    private readonly IDateTimeService _dateTimeService;

    public GetResultWorklistQueryHandler(IWorklistReader reader, IDateTimeService dateTimeService)
    {
        _reader = reader;
        _dateTimeService = dateTimeService;
    }

    public Task<IReadOnlyList<WorklistPatientDto>> Handle(GetResultWorklistQuery request, CancellationToken cancellationToken)
        => _reader.GetAsync(request.Date ?? _dateTimeService.UtcNow.Date, request.Category, cancellationToken);
}
