namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientAssignmentListItemDto
{
    public Guid AssignmentId { get; set; }
    public AssignmentUserDto Doctor { get; set; } = new();
    public AssignmentUserDto Patient { get; set; } = new();
}

public class AssignmentUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
