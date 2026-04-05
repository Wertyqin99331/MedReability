namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewResponseDto
{
    public DoctorPatientOverviewPatientDto Patient { get; set; } = null!;
    public bool HasPlan { get; set; }
    public DoctorPatientOverviewPlanDto? Plan { get; set; }
    public DoctorPatientOverviewProgressDto? Progress { get; set; }
    public List<DoctorPatientOverviewDayDto> Days { get; set; } = [];
    public DoctorPatientTodayWorkoutDto? TodayWorkout { get; set; }
}
