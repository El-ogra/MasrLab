using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;

public record UpdateCultureAntibioticCommand(
    int Id,
    string? SensitivityText,
    bool Pregnant,
    bool Children,
    IReadOnlyList<CommercialNameInput> CommercialNames) : IRequest<Unit>;

public record CommercialNameInput(string Name, bool Print);
