namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewProgressDto
{
    public int CompletedDaysCount { get; set; }
    public int PlannedTrainingDaysCount { get; set; }
    public int CompletionPercent { get; set; }
}
