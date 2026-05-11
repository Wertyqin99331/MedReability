namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayExerciseProgressResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid PatientTrainingPlanId { get; set; }
    public int DayNumber { get; set; }
    public Guid DayExerciseId { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}
