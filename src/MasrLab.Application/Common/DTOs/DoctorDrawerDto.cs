namespace MasrLab.Application.Common.DTOs;

public record DoctorDrawerDto
{
    public int DoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public decimal TotalIncome { get; init; }
    public decimal CommissionAmount { get; init; }
}
