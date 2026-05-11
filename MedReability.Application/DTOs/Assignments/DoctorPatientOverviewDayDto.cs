namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewDayDto
{
    public DateOnly Date { get; set; }
    public int? DayNumber { get; set; }
    public DoctorPatientOverviewDayType DayType { get; set; } = DoctorPatientOverviewDayType.Empty;
    public bool HasTraining { get; set; }
    public bool IsCompleted { get; set; }
    public int? WellBeingRating { get; set; }
    public int? WorkoutDifficultyRating { get; set; }
    public bool? HadPain { get; set; }
    public int? PainIntensityRating { get; set; }
}
