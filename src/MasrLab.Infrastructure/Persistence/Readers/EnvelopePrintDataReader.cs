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
                          select new { patient.Name, PatientCode = patient.LabId, LaboratoryNumber = visit.LabId, visit.Id, visit.VisitDate })
            .SingleOrDefaultAsync(ct);
        return data is null ? null : new EnvelopePrintDto
        {
            PatientName = data.Name, PatientCode = data.PatientCode, LaboratoryNumber = data.LaboratoryNumber,
            VisitNumber = data.Id, DeliveryDate = data.VisitDate, DeliveryTicketNumber = $"DLV-{data.Id:D6}"
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
                             join test in context.Tests.AsNoTracking() on vt.TestId equals test.Id
                             join result in context.TestResults.AsNoTracking() on vt.Id equals result.VisitTestId into resultJoin
                             from result in resultJoin.DefaultIfEmpty()
                             where vt.PatientVisitId == patientVisitId
                             select new ClinicalResultLineDto(test.ReportName == "" ? test.Name : test.ReportName,
                                 result == null ? "" : result.Value, result == null ? test.Unit : result.Unit,
                                 result == null ? "" : result.ReferenceRange)).ToListAsync(ct);
        var organisms = await (from culture in context.Cultures.AsNoTracking()
                               join vt in context.VisitTests.AsNoTracking() on culture.VisitTestId equals vt.Id
                               where vt.PatientVisitId == patientVisitId
                               select new[] { culture.OrganismA, culture.OrganismB, culture.OrganismC }).ToListAsync(ct);
        return new ClinicalReportPrintDto { PatientName = visit.Name, LaboratoryNumber = visit.LabId, VisitDate = visit.VisitDate,
            Results = results, CultureSummary = string.Join(", ", organisms.SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x))) };
    }
}
