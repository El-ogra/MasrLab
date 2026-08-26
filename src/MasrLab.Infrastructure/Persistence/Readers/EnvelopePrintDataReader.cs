using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Printing;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class EnvelopePrintDataReader(MasrLabDbContext context) : IEnvelopePrintDataReader
{
    public async Task<EnvelopePrintDto?> GetEnvelopeAsync(int patientVisitId, CancellationToken ct = default)
    {
        var data = await (from visit in context.PatientVisits.AsNoTracking()
                          join patient in context.Patients.AsNoTracking() on visit.PatientId equals patient.Id
                          where visit.Id == patientVisitId
                          select new { patient.Name, PatientCode = patient.LabId, LaboratoryNumber = visit.LabId, visit.Id, visit.VisitDate, visit.PromisedDeliveryAt })
            .SingleOrDefaultAsync(ct);
        if (data is null)
            return null;

        var settingValues = await context.SystemSettings.AsNoTracking()
            .Where(setting => setting.SettingKey == "Envelope_UseBarcode"
                           || setting.SettingKey == "Envelope_BarcodeWidth"
                           || setting.SettingKey == "Envelope_BarcodeHeight")
            .ToDictionaryAsync(setting => setting.SettingKey, setting => setting.SettingValue, ct);

        return new EnvelopePrintDto
        {
            PatientName = data.Name, PatientCode = data.PatientCode, LaboratoryNumber = data.LaboratoryNumber,
            VisitNumber = data.Id, DeliveryDate = data.PromisedDeliveryAt ?? data.VisitDate, DeliveryTicketNumber = $"DLV-{data.Id:D6}",
            BarcodeSettings = new EnvelopeBarcodeSettingsDto
            {
                UseBarcode = settingValues.TryGetValue("Envelope_UseBarcode", out var enabled) && bool.TryParse(enabled, out var useBarcode) && useBarcode,
                BarcodeWidth = GetIntInRangeOrDefault(settingValues, "Envelope_BarcodeWidth", 1, 600, 300),
                BarcodeHeight = GetIntInRangeOrDefault(settingValues, "Envelope_BarcodeHeight", 1, 180, 100)
            }
        };
    }

    public async Task<ClinicalReportPrintDto?> GetClinicalReportAsync(int patientVisitId, CancellationToken ct = default)
    {
        var visit = await (from v in context.PatientVisits.AsNoTracking()
                           join p in context.Patients.AsNoTracking() on v.PatientId equals p.Id
                           where v.Id == patientVisitId
                           select new { p.Name, v.LabId, v.VisitDate }).SingleOrDefaultAsync(ct);
        if (visit is null) return null;
        var results = await (from vt in context.VisitTests.AsNoTracking()
                              join resultItem in context.VisitTestResultItems.AsNoTracking() on vt.Id equals resultItem.VisitTestId into resultItemJoin
                              from resultItem in resultItemJoin.DefaultIfEmpty()
                              join result in context.TestResults.AsNoTracking() on resultItem.Id equals result.VisitTestResultItemId into resultJoin
                              from result in resultJoin.DefaultIfEmpty()
                              where vt.PatientVisitId == patientVisitId
                              select new ClinicalResultLineDto(
                                   vt.ReportNameSnapshot ?? vt.TestNameSnapshot,
                                   result == null ? "" : result.Value,
                                   result == null ? resultItem.ComponentUnit : result.Unit,
                                   result == null ? "" : result.ReferenceRange,
                                   "", "")).ToListAsync(ct);
        var organisms = await (from culture in context.Cultures.AsNoTracking()
                               join resultItem in context.VisitTestResultItems.AsNoTracking() on culture.VisitTestResultItemId equals resultItem.Id
                               join vt in context.VisitTests.AsNoTracking() on resultItem.VisitTestId equals vt.Id
                               where vt.PatientVisitId == patientVisitId
                               select new[] { culture.OrganismA, culture.OrganismB, culture.OrganismC }).ToListAsync(ct);
        return new ClinicalReportPrintDto { PatientName = visit.Name, LaboratoryNumber = visit.LabId, VisitDate = visit.VisitDate,
            Results = results, CultureSummary = string.Join(", ", organisms.SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x))) };
    }

    private static int GetIntInRangeOrDefault(IReadOnlyDictionary<string, string> settings, string key, int minimum, int maximum, int defaultValue) =>
        settings.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) && parsed >= minimum && parsed <= maximum ? parsed : defaultValue;
}
