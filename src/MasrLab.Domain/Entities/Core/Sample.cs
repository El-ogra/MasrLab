using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class Sample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public string SampleType { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string CollectionStatus { get; set; } = string.Empty;
}
