namespace MasrLab.Infrastructure.Persistence.Views;

public class PatientHistoryView
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int PatientVisitId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? TestName { get; set; }
    public string? ResultValue { get; set; }
}
