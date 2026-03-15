using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public Guid ClinicId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
