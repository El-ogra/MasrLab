using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.AddCommercialPackage;

public record AddCommercialPackageCommand(
    string Name,
    string TestIds,
    string Prices
) : IRequest<int>;
