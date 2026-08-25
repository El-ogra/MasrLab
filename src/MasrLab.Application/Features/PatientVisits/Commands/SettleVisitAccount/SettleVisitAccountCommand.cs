using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.SettleVisitAccount;

// OQ-M2-5: backs both UI entry points ("خلاص" and "تصفية الحساب") with one command.
public record SettleVisitAccountCommand(
    int PatientVisitId,
    int UserId
) : IRequest<int>;
