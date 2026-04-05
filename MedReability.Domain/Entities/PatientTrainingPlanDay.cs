namespace MedReability.Domain.Entities;

public class PatientTrainingPlanDay
{
    public Guid Id { get; set; }

    public Guid PatientTrainingPlanId { get; set; }
    public PatientTrainingPlan PatientTrainingPlan { get; set; } = null!;

    public int DayNumber { get; set; }
    public bool IsRestDay { get; set; }
    public string? Notes { get; set; }

    public List<PatientTrainingPlanDayExercise> Exercises { get; set; } = [];
}
