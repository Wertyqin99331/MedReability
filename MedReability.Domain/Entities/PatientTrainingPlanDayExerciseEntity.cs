namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayExerciseEntity
{
    public Guid Id { get; set; }

    public Guid PatientTrainingPlanDayId { get; set; }
    public PatientTrainingPlanDayEntity PatientTrainingPlanDayEntity { get; set; } = null!;

    public int Order { get; set; }

    public Guid ExerciseId { get; set; }
    public ExerciseEntity ExerciseEntity { get; set; } = null!;

    public int? Sets { get; set; }
    public int? RestBetweenSetsInSeconds { get; set; }
    public int? RestAfterInSeconds { get; set; }
    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}
