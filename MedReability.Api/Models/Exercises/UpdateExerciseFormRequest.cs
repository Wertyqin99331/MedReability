using System.ComponentModel.DataAnnotations;
using MedReability.Domain.Enums;

namespace MedReability.Api.Models.Exercises;

public class UpdateExerciseFormRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public List<string> Steps { get; set; } = [];

    [Required]
    public ExerciseType Type { get; set; }

    public List<string> ExerciseTypes { get; set; } = [];

    public List<string> BodyParts { get; set; } = [];

    public List<string> Inventory { get; set; } = [];

    public List<IFormFile>? MediaFiles { get; set; }
}
