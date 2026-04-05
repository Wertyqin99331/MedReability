using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.TrainingPlans;

public class UpdatePatientTrainingPlanRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public List<CreatePatientTrainingPlanDayRequestDto> Days { get; set; } = [];
}
