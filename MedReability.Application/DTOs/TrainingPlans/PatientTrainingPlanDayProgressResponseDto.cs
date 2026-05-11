namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayProgressResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid PatientTrainingPlanId { get; set; }
    public int DayNumber { get; set; }
    public int? WellBeingRating { get; set; }
    public int? WorkoutDifficultyRating { get; set; }
    public bool? HadPain { get; set; }
    public int? PainIntensityRating { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}
