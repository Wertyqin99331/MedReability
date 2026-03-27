using System.ComponentModel.DataAnnotations;
using MedReability.Domain.Enums;

namespace MedReability.Api.Models.Exercises;

public class CreateExerciseFormRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public IFormFile? Media { get; set; }

    [Required]
    public List<string> Steps { get; set; } = [];

    [Required]
    public ExerciseType Type { get; set; }

    public bool IsGlobal { get; set; }
}
