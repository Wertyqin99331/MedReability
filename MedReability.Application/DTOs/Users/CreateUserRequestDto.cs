using System.ComponentModel.DataAnnotations;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MedReability.Application.DTOs.Users;

public class CreateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

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

    [Required]
    public UserRole Role { get; set; }
}
