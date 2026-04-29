using MedReability.Application.DTOs.Exercises;

namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayExerciseResponseDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public Guid ExerciseId { get; set; }
    public ExerciseResponseDto ExerciseEntity { get; set; } = null!;
    public int? Sets { get; set; }
    public int? RestBetweenSets { get; set; }
    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}
