namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayExerciseProgressEntity
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }
    public UserEntity Patient { get; set; } = null!;

    public Guid PatientTrainingPlanId { get; set; }
    public PatientTrainingPlanEntity PatientTrainingPlanEntity { get; set; } = null!;

    public int DayNumber { get; set; }

    public Guid PatientTrainingPlanDayExerciseId { get; set; }
    public PatientTrainingPlanDayExerciseEntity PatientTrainingPlanDayExerciseEntity { get; set; } = null!;

    public DateTime CompletedAtUtc { get; set; }
}
