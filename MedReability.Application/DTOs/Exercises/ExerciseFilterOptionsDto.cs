using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Exercises;

public class ExerciseFilterOptionsDto
{
    public List<ExerciseType> TrackingTypes { get; set; } = [];
    public List<string> ExerciseTypes { get; set; } = [];
    public List<string> BodyParts { get; set; } = [];
    public List<string> Inventory { get; set; } = [];
}
