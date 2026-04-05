using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.TrainingPlans;

public class CreatePatientTrainingPlanDayRequestDto
{
    [Required]
    public int DayNumber { get; set; }

    [Required]
    public bool IsRestDay { get; set; }

    public string? Notes { get; set; }

    public List<CreatePatientTrainingPlanDayExerciseRequestDto> Exercises { get; set; } = [];
}
