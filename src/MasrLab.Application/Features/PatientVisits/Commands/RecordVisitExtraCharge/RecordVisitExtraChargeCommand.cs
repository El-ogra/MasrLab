using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitExtraCharge;

public record RecordVisitExtraChargeCommand(
    int ReceiptId,
    string Description,
    decimal Amount,
    int UserId
) : IRequest<int>;
