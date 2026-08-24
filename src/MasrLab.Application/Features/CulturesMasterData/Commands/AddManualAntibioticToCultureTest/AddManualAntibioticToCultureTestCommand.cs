using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;

public record AddManualAntibioticToCultureTestCommand(
    int CultureTestId,
    string Symbol,
    string ScientificName,
    string? SensitivityText,
    bool Pregnant,
    bool Children,
    IReadOnlyList<CommercialNameInput> CommercialNames) : IRequest<Unit>;

public record CommercialNameInput(string Name, bool Print);
