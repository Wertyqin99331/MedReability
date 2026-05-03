using System.ComponentModel.DataAnnotations;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MedReability.Application.DTOs.Exercises;

public class CreateExerciseRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public List<IFormFile>? MediaFiles { get; set; }

    [Required]
    public List<string> Steps { get; set; } = [];

    [Required]
    public ExerciseType Type { get; set; }

    public List<string> ExerciseTypes { get; set; } = [];

    public List<string> BodyParts { get; set; } = [];

    public List<string> Inventory { get; set; } = [];

    public bool IsGlobal { get; set; }
}
