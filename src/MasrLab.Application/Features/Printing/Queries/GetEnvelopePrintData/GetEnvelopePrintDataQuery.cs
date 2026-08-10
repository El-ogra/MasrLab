using MasrLab.Application.Common.Printing;
using MediatR;

namespace MasrLab.Application.Features.Printing.Queries.GetEnvelopePrintData;

public sealed record GetEnvelopePrintDataQuery(int PatientVisitId) : IRequest<EnvelopePrintDto>;
public sealed record GetClinicalReportPrintDataQuery(int PatientVisitId) : IRequest<ClinicalReportPrintDto>;

public sealed class GetEnvelopePrintDataQueryHandler(IEnvelopePrintDataReader reader) : IRequestHandler<GetEnvelopePrintDataQuery, EnvelopePrintDto>
{
    public async Task<EnvelopePrintDto> Handle(GetEnvelopePrintDataQuery request, CancellationToken ct) =>
        await reader.GetEnvelopeAsync(request.PatientVisitId, ct) ?? throw new KeyNotFoundException("Patient visit was not found.");
}

public sealed class GetClinicalReportPrintDataQueryHandler(IEnvelopePrintDataReader reader) : IRequestHandler<GetClinicalReportPrintDataQuery, ClinicalReportPrintDto>
{
    public async Task<ClinicalReportPrintDto> Handle(GetClinicalReportPrintDataQuery request, CancellationToken ct) =>
        await reader.GetClinicalReportAsync(request.PatientVisitId, ct) ?? throw new KeyNotFoundException("Patient visit was not found.");
}
