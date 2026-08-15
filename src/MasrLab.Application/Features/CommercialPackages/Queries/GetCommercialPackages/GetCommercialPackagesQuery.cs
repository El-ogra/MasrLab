using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Queries.GetCommercialPackages;

public record GetCommercialPackagesQuery : IRequest<IReadOnlyList<CommercialPackageDto>>;
