namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanDayProgressResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid PatientTrainingPlanId { get; set; }
    public int DayNumber { get; set; }
    public int? StateRating { get; set; }
    public string? Notes { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}
