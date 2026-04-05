using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.TrainingPlans;

public class PatientTrainingPlanResponseDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public PatientTrainingPlanStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public List<PatientTrainingPlanDayResponseDto> Days { get; set; } = [];
}
