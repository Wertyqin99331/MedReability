namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
}
