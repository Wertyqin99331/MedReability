using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.TrainingPlans;

public class CreatePatientTrainingPlanDayExerciseRequestDto
{
    [Required]
    public Guid ExerciseId { get; set; }

    [Required]
    public int Order { get; set; }

    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}
