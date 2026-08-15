using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.UpdateCommercialPackage;

public record UpdateCommercialPackageCommand(
    int Id,
    string Name,
    string TestIds,
    string Prices
) : IRequest<Unit>;
