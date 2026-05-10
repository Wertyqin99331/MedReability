namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewResponseDto : PatientPlanOverviewResponseDto
{
    public DoctorPatientOverviewPatientDto Patient { get; set; } = null!;
}
