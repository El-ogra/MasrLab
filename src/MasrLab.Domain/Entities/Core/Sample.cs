using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class Sample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public string SampleType { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public SampleStatus CollectionStatus { get; set; }
    public int? CollectedByUserId { get; set; }
}
