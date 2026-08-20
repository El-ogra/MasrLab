using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public record AddReferralEntityCommand(
    string Name,
    ReferralEntityType EntityType,
    string? ContactPerson,
    string? Phone,
    string? Fax,
    string? Address,
    string? City,
    decimal? Discount,
    decimal? Commission,
    int? PriceListId,
    decimal AccountBalance
) : IRequest<Unit>;
