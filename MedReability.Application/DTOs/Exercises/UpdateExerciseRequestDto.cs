using System.ComponentModel.DataAnnotations;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MedReability.Application.DTOs.Exercises;

public class UpdateExerciseRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public List<string> Steps { get; set; } = [];

    [Required]
    public ExerciseType Type { get; set; }

    public List<IFormFile>? MediaFiles { get; set; }
}
