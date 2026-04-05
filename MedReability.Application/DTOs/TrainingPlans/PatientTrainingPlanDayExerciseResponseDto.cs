using MedReability.Application.DTOs.Exercises;

namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayExerciseResponseDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public Guid ExerciseId { get; set; }
    public ExerciseResponseDto Exercise { get; set; } = null!;
    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}
