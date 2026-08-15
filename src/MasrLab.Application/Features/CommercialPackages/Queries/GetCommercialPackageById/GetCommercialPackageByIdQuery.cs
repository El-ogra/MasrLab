using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Queries.GetCommercialPackageById;

public record GetCommercialPackageByIdQuery(int Id) : IRequest<CommercialPackageDto?>;
