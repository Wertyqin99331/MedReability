namespace MedReability.Application.DTOs.Auth;

public class MeResponseDto
{
    public Guid UserId { get; set; }
    public Guid ClinicId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
