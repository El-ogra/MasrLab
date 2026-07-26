using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class SampleCollection : BaseEntity
{
    public int SampleId { get; set; }
    public int PatientId { get; set; }
    public bool IsCollected { get; set; }
    public DateTime CollectedAt { get; set; }
}
