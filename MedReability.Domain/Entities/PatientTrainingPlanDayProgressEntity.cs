namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayProgressEntity
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }
    public UserEntity Patient { get; set; } = null!;

    public Guid PatientTrainingPlanId { get; set; }
    public PatientTrainingPlanEntity PatientTrainingPlanEntity { get; set; } = null!;

    public int DayNumber { get; set; }

    public int? WellBeingRating { get; set; }
    public int? WorkoutDifficultyRating { get; set; }
    public bool? HadPain { get; set; }
    public int? PainIntensityRating { get; set; }

    public DateTime CompletedAtUtc { get; set; }
}
