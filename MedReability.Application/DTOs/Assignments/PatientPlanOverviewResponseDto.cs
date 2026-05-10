namespace MedReability.Application.DTOs.Assignments;

public class PatientPlanOverviewResponseDto
{
    public bool HasPlan { get; set; }
    public DoctorPatientOverviewPlanDto? Plan { get; set; }
    public DoctorPatientOverviewProgressDto? Progress { get; set; }
    public List<DoctorPatientOverviewDayDto> Days { get; set; } = [];
    public DoctorPatientTodayWorkoutDto? TodayWorkout { get; set; }
}
