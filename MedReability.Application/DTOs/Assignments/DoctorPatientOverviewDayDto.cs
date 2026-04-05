namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewDayDto
{
    public DateOnly Date { get; set; }
    public int? DayNumber { get; set; }
    public DoctorPatientOverviewDayType DayType { get; set; } = DoctorPatientOverviewDayType.Empty;
    public bool HasTraining { get; set; }
    public bool IsCompleted { get; set; }
    public int? StateRating { get; set; }
    public string? Notes { get; set; }
}
