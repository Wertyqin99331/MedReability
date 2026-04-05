using MedReability.Domain.Enums;

namespace MedReability.Domain.Entities;

public class PatientTrainingPlan
{
    public Guid Id { get; set; }

    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public Guid PatientId { get; set; }
    public User Patient { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public PatientTrainingPlanStatus Status { get; set; } = PatientTrainingPlanStatus.Assigned;
    public bool IsDeleted { get; set; }

    public List<PatientTrainingPlanDay> Days { get; set; } = [];
}
