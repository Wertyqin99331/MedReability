using MedReability.Domain.Enums;

namespace MedReability.Domain.Entities;

public class PatientTrainingPlanEntity
{
    public Guid Id { get; set; }

    public Guid ClinicId { get; set; }
    public ClinicEntity ClinicEntity { get; set; } = null!;

    public Guid PatientId { get; set; }
    public UserEntity Patient { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public UserEntity CreatedByUser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public PatientTrainingPlanStatus Status { get; set; } = PatientTrainingPlanStatus.Assigned;
    public bool IsDeleted { get; set; }

    public List<PatientTrainingPlanDayEntity> Days { get; set; } = [];
}
