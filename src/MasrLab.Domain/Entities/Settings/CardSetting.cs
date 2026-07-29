using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Settings;

public class CardSetting : BaseEntity
{
    public string CardTitle { get; set; } = string.Empty;
    public bool ShowPatientName { get; set; } = true;
    public bool ShowLabId { get; set; } = true;
    public bool ShowNationalId { get; set; } = true;
    public bool ShowPhone { get; set; } = true;
    public bool ShowAge { get; set; } = true;
    public bool ShowGender { get; set; } = true;
    public bool ShowAddress { get; set; }
    public string? HeaderColor { get; set; }
    public string? FontSize { get; set; } = "Medium";
}
