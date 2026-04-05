namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayExercise
{
    public Guid Id { get; set; }

    public Guid PatientTrainingPlanDayId { get; set; }
    public PatientTrainingPlanDay PatientTrainingPlanDay { get; set; } = null!;

    public int Order { get; set; }

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}
