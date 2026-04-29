using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.Users;

public class SetUserPasswordRequestDto
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
