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
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public bool TakenOutsideLab { get; set; }

    // INV-03: If PatientVisit.DoctorId is null, the visit defaults to Patient.DoctorId.
    // This invariant will be enforced by a Domain Service in a later phase.
}
