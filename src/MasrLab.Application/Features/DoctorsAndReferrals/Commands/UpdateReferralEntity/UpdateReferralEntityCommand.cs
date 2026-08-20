using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;

public record UpdateReferralEntityCommand(
    int Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Fax,
    string? Address,
    string? City,
    decimal? Discount,
    decimal? Commission,
    int? PriceListId
) : IRequest<Unit>;
