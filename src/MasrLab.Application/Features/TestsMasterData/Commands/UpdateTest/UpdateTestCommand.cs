using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;

public record UpdateTestCommand(
    int Id,
    string Name,
    string ReportName,
    string ReceiptName,
    string Group,
    string? Barcode,
    decimal Price,
    string TurnaroundTime,
    bool LabToLabFlag,
    string Unit
) : IRequest<Unit>;
