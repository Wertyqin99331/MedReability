namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientOverviewPatientDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
