using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PatientTrainingPlanStatus Status { get; set; }
    public DateOnly StartDate { get; set; }
}
