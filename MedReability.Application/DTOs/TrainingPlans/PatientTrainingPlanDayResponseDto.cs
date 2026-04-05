namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayResponseDto
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public bool IsRestDay { get; set; }
    public string? Notes { get; set; }
    public List<PatientTrainingPlanDayExerciseResponseDto> Exercises { get; set; } = [];
}
