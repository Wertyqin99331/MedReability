namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientAssignmentsQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public string? Search { get; set; }
}
