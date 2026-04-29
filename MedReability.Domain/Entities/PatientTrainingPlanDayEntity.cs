namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDayEntity
{
    public Guid Id { get; set; }

    public Guid PatientTrainingPlanId { get; set; }
    public PatientTrainingPlanEntity PatientTrainingPlanEntity { get; set; } = null!;

    public int DayNumber { get; set; }
    public bool IsRestDay { get; set; }
    public string? Notes { get; set; }

    public List<PatientTrainingPlanDayExerciseEntity> Exercises { get; set; } = [];
}
