using MedReability.Domain.Enums;

namespace MedReability.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
