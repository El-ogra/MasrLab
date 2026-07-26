using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Settings;

public class Printer : BaseEntity
{
    public string PrinterName { get; set; } = string.Empty;
    public PrinterPurposeType PurposeType { get; set; }
}
