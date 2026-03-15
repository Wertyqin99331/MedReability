using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}
