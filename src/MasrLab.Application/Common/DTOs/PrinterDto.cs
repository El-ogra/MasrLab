using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record PrinterDto
{
    public int Id { get; init; }
    public string PrinterName { get; init; } = string.Empty;
    public PrinterPurposeType PurposeType { get; init; }
}
