using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.DeleteCommercialPackage;

public record DeleteCommercialPackageCommand(int Id) : IRequest<Unit>;
