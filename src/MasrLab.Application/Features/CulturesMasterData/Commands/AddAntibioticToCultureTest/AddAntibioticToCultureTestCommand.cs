using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;

public record AddAntibioticToCultureTestCommand(
    int CultureTestId,
    int AntibioticId,
    string? SensitivityText,
    bool Pregnant,
    bool Children,
    IReadOnlyList<CommercialNameInput> CommercialNames) : IRequest<Unit>;

public record CommercialNameInput(string Name, bool Print);
