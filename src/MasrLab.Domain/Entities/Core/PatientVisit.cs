using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class PatientVisit : BaseEntity
{
    public DateTime VisitDate { get; set; }
    public int PatientId { get; set; }
    public VisitStatus Status { get; set; }
    public int RegisteredByUserId { get; set; }
    public string LabId { get; set; } = string.Empty;
    public SampleStatus SampleStatus { get; set; }
    public bool TakenOutsideLab { get; set; }
}
