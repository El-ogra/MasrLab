using MasrLab.Domain.Common.Enums;
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
    string Unit,
    string? TestCode,
    string? HistoryName,
    string? ArabicName,
    string? Branch,
    string? LogGroup,
    string? SampleType,
    bool SeeReport,
    bool PrintWithOther,
    bool AddWithGroup,
    bool IsMainTest,
    int TestTimeDays,
    int ArrangeNo,
    ReferenceType ReferenceType,
    decimal? LabToLabPrice,
    string? BarcodeName,
    string? Tube1,
    string? Tube2,
    string? Tube3,
    bool SentOutsideLab,
    string? OutsourcedLabName,
    decimal? OutsourcedCostPrice,
    string? PatientQuestion
) : IRequest<Unit>;
