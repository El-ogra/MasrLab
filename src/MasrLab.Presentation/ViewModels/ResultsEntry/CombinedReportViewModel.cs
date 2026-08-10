using CommunityToolkit.Mvvm.ComponentModel;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;

namespace MasrLab.Presentation.ViewModels.ResultsEntry;

public partial class CombinedReportViewModel(IPrintService printService) : ObservableObject
{
    public Task PrintAsync(ClinicalReportPrintDto payload, string? printerName = null, CancellationToken ct = default) => printService.PrintAsync(PrintReportNames.CombinedResult, payload, printerName, ct);
}
