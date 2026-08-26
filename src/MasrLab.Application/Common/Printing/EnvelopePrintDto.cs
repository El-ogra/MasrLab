using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Common.Printing;

public sealed record EnvelopePrintDto : IPrintPayload
{
    public string PatientName { get; init; } = string.Empty;
    public string PatientCode { get; init; } = string.Empty;
    public string LaboratoryNumber { get; init; } = string.Empty;
    public int VisitNumber { get; init; }
    public DateTime DeliveryDate { get; init; }
    public string DeliveryTicketNumber { get; init; } = string.Empty;
    public EnvelopeBarcodeSettingsDto BarcodeSettings { get; init; } = new();
}

public sealed record ClinicalReportPrintDto : IPrintPayload
{
    public string PatientName { get; init; } = string.Empty;
    public string LaboratoryNumber { get; init; } = string.Empty;
    public DateTime VisitDate { get; init; }
    public IReadOnlyList<ClinicalResultLineDto> Results { get; init; } = [];
    public string? CultureSummary { get; init; }

    // M4-BR-09 header enrichment (additive — Slice 11).
    public string PatientBarcode { get; init; } = string.Empty;
    public string AgeSex { get; init; } = string.Empty;
    public string ReferredBy { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }
    public DateTime? PrintedAtUtc { get; init; }
    public string DoctorSignatureLine { get; init; } = string.Empty;
}

public sealed record ClinicalResultLineDto(string TestName, string Value, string Unit, string ReferenceRange, string Flag = "", string Comment = "");

public interface IEnvelopePrintDataReader
{
    Task<EnvelopePrintDto?> GetEnvelopeAsync(int patientVisitId, CancellationToken ct = default);
    Task<ClinicalReportPrintDto?> GetClinicalReportAsync(int patientVisitId, CancellationToken ct = default);
}
