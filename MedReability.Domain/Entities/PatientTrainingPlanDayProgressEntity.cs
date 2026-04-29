namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayProgressEntity
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }
    public UserEntity Patient { get; set; } = null!;

    public Guid PatientTrainingPlanId { get; set; }
    public PatientTrainingPlanEntity PatientTrainingPlanEntity { get; set; } = null!;

    public int DayNumber { get; set; }

    public int? StateRating { get; set; }
    public string? Notes { get; set; }

    public DateTime CompletedAtUtc { get; set; }
}
