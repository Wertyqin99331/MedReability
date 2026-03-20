namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientListItemDto
{
    public Guid AssignmentId { get; set; }
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
