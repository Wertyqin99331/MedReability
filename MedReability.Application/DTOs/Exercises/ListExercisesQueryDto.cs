using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Exercises;

public class ListExercisesQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool All { get; set; }
    public string? Search { get; set; }
    public List<ExerciseType>? Types { get; set; }
    public List<string>? ExerciseTypes { get; set; }
    public List<string>? BodyParts { get; set; }
    public List<string>? Inventory { get; set; }
}
