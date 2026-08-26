using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.Cultures.Queries.GetMicrobiologyReport;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class MicrobiologyReportReader : IMicrobiologyReportReader
{
    private readonly MasrLabDbContext _context;

    public MicrobiologyReportReader(MasrLabDbContext context) => _context = context;

    public async Task<MicrobiologyReportDto?> GetAsync(int cultureId, CancellationToken cancellationToken = default)
    {
        var culture = await _context.Cultures.AsNoTracking()
            .Where(c => c.Id == cultureId)
            .Select(c => new
            {
                c.Id,
                c.VisitTestResultItemId,
                c.SampleType,
                c.CultureCondition,
                c.ColonyCount,
                c.ShowSensitivityInReport,
                c.ShowReferenceInReport,
                c.ShowCommercialNameInReport,
                c.OrganismA,
                c.OrganismB,
                c.OrganismC
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (culture is null)
            return null;

        // The culture's test identity unlocks the M13 master data for this culture test.
        var cultureTestId = await (
                from item in _context.VisitTestResultItems.AsNoTracking()
                join visitTest in _context.VisitTests.AsNoTracking()
                    on item.VisitTestId equals visitTest.Id
                where item.Id == culture.VisitTestResultItemId
                select visitTest.TestId)
            .FirstOrDefaultAsync(cancellationToken);

        // M4-BR-17 microscopic block (Bacteria included — it is system-derived).
        // OQ-M4-5: unchecked microscopic rows are excluded from every rendered report.
        var microscopicRows = await _context.MicroscopicFindings.AsNoTracking()
            .Where(f => f.CultureId == cultureId && !f.IsDeleted && f.IncludeInPrint)
            .OrderBy(f => f.RowKey)
            .Select(f => new { f.RowKey, f.Value, f.ReferenceRange })
            .ToListAsync(cancellationToken);

        // OQ-M4-13: one four-category block per recorded organism slot.
        var sensitivities = await _context.Sensitivities.AsNoTracking()
            .Where(s => s.CultureId == cultureId && !s.IsDeleted)
            .Select(s => new { s.OrganismSlot, s.AntibioticId, s.SensitivityLevel, s.InhibitionZoneOverride })
            .ToListAsync(cancellationToken);
        var antibioticIds = sensitivities.Select(s => s.AntibioticId).Distinct().ToList();

        var antibioticNames = await _context.Antibiotics.AsNoTracking()
            .Where(a => antibioticIds.Contains(a.Id))
            .Select(a => new { a.Id, a.Name })
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        // OQ-M4-12: default inhibition zone falls back to the M13 master sensitivity text.
        var masterTexts = await _context.CultureAntibiotics.AsNoTracking()
            .Where(ca => ca.CultureTestId == cultureTestId && antibioticIds.Contains(ca.AntibioticId))
            .Select(ca => new { ca.AntibioticId, ca.SensitivityText })
            .ToListAsync(cancellationToken);
        var defaultZoneByAntibiotic = masterTexts
            .GroupBy(x => x.AntibioticId)
            .ToDictionary(g => g.Key, g => g.First().SensitivityText ?? string.Empty);

        var commercialNames = await (
                from n in _context.Set<Domain.Entities.Culture.CultureAntibioticCommercialName>().AsNoTracking()
                join ca in _context.CultureAntibiotics.AsNoTracking()
                    on n.CultureAntibioticId equals ca.Id
                where ca.CultureTestId == cultureTestId
                      && n.Print
                      && antibioticIds.Contains(ca.AntibioticId)
                select new { ca.AntibioticId, n.Name })
            .ToListAsync(cancellationToken);
        var commercialByAntibiotic = commercialNames
            .GroupBy(n => n.AntibioticId)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)));

        var organismLabelFor = (OrganismSlot slot, string? name) =>
            $"({slot}) {(string.IsNullOrWhiteSpace(name) ? "—" : name)}";

        var blocks = new List<OrganismSensitivityBlockDto>();
        foreach (var (slot, organismName) in new[]
                 {
                     (OrganismSlot.A, culture.OrganismA),
                     (OrganismSlot.B, culture.OrganismB),
                     (OrganismSlot.C, culture.OrganismC)
                 })
        {
            var rows = sensitivities
                .Where(s => s.OrganismSlot == slot)
                .OrderBy(s => antibioticNames.GetValueOrDefault(s.AntibioticId)?.Name ?? s.AntibioticId.ToString())
                .Select(s =>
                {
                    var zone = !string.IsNullOrWhiteSpace(s.InhibitionZoneOverride)
                        ? s.InhibitionZoneOverride!
                        : defaultZoneByAntibiotic.GetValueOrDefault(s.AntibioticId, string.Empty);
                    return new SensitivityRowDto(
                        s.AntibioticId,
                        antibioticNames.TryGetValue(s.AntibioticId, out var a) ? a.Name : $"#{s.AntibioticId}",
                        culture.ShowCommercialNameInReport ? commercialByAntibiotic.GetValueOrDefault(s.AntibioticId, string.Empty) : string.Empty,
                        s.SensitivityLevel.ToString(),
                        culture.ShowReferenceInReport ? zone : string.Empty);
                })
                .ToList();
            blocks.Add(new OrganismSensitivityBlockDto(organismLabelFor(slot, organismName), rows));
        }

        return new MicrobiologyReportDto(
            culture.Id,
            culture.SampleType,
            culture.CultureCondition,
            culture.ColonyCount,
            culture.ShowSensitivityInReport,
            culture.ShowReferenceInReport,
            culture.ShowCommercialNameInReport,
            microscopicRows
                .Select(f => new MicrobiologyFindingRowDto(f.RowKey.ToString(), f.Value, f.ReferenceRange))
                .ToList(),
            blocks);
    }
}
