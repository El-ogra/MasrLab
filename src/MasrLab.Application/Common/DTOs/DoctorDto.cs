namespace MasrLab.Application.Common.DTOs;

public record DoctorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public decimal CommissionPercent { get; init; }
}
