using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedReability.Application.DTOs.Users;

public class UpdateUserProfileRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string Patronymic { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}
